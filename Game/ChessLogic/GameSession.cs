namespace ChessLogic
{
    public class GameSession
    {
        private readonly Dictionary<BoardType, GameState> boardStates = new();
        private readonly Dictionary<BoardType, BoardTimer> boardTimers = new();
        private readonly Dictionary<(BoardType Board, Player Player), int> blockedHistoryIndices = new();
        public Player CurrentPlayer { get; private set; }
        public Result Result { get; private set; } = null;
        private const int BoardTimerLimit = 5;
        private int noCaptureOrPawnMoves = 0;
        public Player? DrawOfferBy { get; private set; }

        public bool IsGameOver()
        {
            return Result != null;
        }

        public GameSession(GameState initial)
        {
            boardStates[BoardType.Main] = initial;
            CurrentPlayer = initial.CurrentPlayer;
            boardTimers[BoardType.Main] = new BoardTimer();
        }

        public void SwitchPlayer()
        {
            CurrentPlayer = CurrentPlayer.Opponent();
        }

        public GameState GetBoardState(BoardType boardType) =>
            boardStates.TryGetValue(boardType, out var s) ? s : null;

        public int GetTimerCount(BoardType boardType, Player player)
        {
            if (!boardTimers.TryGetValue(boardType, out var timer))
            {
                return 0;
            }

            return player switch
            {
                Player.White => timer.White,
                Player.Black => timer.Black,
                _ => 0
            };
        }

        public bool IsTimerLimit(BoardType boardType, Player player)
        {
            if (!boardTimers.TryGetValue(boardType, out var timer))
            {
                return false;
            }

            if (player == Player.White)
            {
                return timer.White >= BoardTimerLimit;
            }
            else if (player == Player.Black)
            {
                return timer.Black >= BoardTimerLimit;
            }

            return false;
        }

        public List<BoardType> GetPlayableBoards()
        {
            return boardStates
                .Where(pair => pair.Value.CurrentPlayer == CurrentPlayer)
                .Select(pair => pair.Key)
                .ToList();
        }

        public BoardType? GetTimerForcedBoard()
        {
            foreach (BoardType boardType in GetPlayableBoards())
            {
                if (IsTimerLimit(boardType, CurrentPlayer))
                {
                    return boardType;
                }
            }

            return null;
        }

        public bool CanPlayOnBoard(BoardType boardType)
        {
            BoardType? forcedBoard = GetTimerForcedBoard();

            if (forcedBoard.HasValue)
            {
                return forcedBoard.Value == boardType;
            }

            return GetPlayableBoards().Contains(boardType);
        }

        public void UpdateBoardTimers(BoardType playedBoard, Player player)
        {
            List<BoardType> playableBoards = GetPlayableBoards();

            foreach (BoardType boardType in playableBoards)
            {
                BoardTimer timer = boardTimers[boardType];

                if (boardType == playedBoard)
                {
                    if (player == Player.White)
                    {
                        timer.White = 0;
                    }
                    else
                    {
                        timer.Black = 0;
                    }
                }
                else
                {
                    if (player == Player.White)
                    {
                        timer.White++;
                    }
                    else
                    {
                        timer.Black++;
                    }
                }
            }
        }

        public bool TimelineExist(MoveHistory selectedHistory)
        {   
            return selectedHistory.NextPlayer switch
            {
                Player.White => boardStates.ContainsKey(BoardType.TimelineWhite),
                Player.Black => boardStates.ContainsKey(BoardType.TimelineBlack),
                _ => false
            };
        }

        private void BlockOldHistoryEntries(Player player)
        {
            foreach (var pair in boardStates)
            {
                blockedHistoryIndices[(pair.Key, player)] =
                    pair.Value.MoveHistory.Count;
            }
        }

        public bool IsHistoryEntryBlocked(BoardType boardType, Player player, int historyIndex)
        {
            return blockedHistoryIndices.TryGetValue((boardType, player), out int blockedIndex)
                && historyIndex < blockedIndex;
        }

        public void RemovePieceFromAllBoards(Guid capturedId, Player player)
        {
            foreach (var state in boardStates.Values)
            {
                state.Board.RemovePieceWithID(capturedId);
            }

            BlockOldHistoryEntries(player);
        }

        public void PromotePieceOnAllBoards(Guid pawnId, PieceType newType, Player color)
        {
            foreach (var state in boardStates.Values)
            {
                Board board = state.Board;

                foreach (Position pos in board.PiecePositions())
                {
                    Piece piece = board[pos];

                    if (piece.Id != pawnId)
                    {
                        continue;
                    }

                    Piece promotedPiece = newType switch
                    {
                        PieceType.Knight => new Knight(color, pawnId),
                        PieceType.Bishop => new Bishop(color, pawnId),
                        PieceType.Rook => new Rook(color, pawnId),
                        _ => new Queen(color, pawnId)
                    };

                    promotedPiece.HasMoved = true;
                    board[pos] = promotedPiece;
                }
            }
        }

        private bool CanCreateTimeline(BoardType boardType, Player player)
        {
            GameState state = boardStates[boardType];

            if (boardStates.ContainsKey(player == Player.White
                ? BoardType.TimelineWhite
                : BoardType.TimelineBlack))
            {
                return false;
            }

            for (int i = 0; i < state.MoveHistory.Count; i++)
            {
                MoveHistory history = state.MoveHistory[i];

                if (history.NextPlayer != player)
                {
                    continue;
                }

                if (TimelineExist(history))
                {
                    continue;
                }

                if (IsHistoryEntryBlocked(boardType, player, i))
                {
                    continue;
                }

                GameState historyState = new GameState(
                    history.NextPlayer,
                    history.Board.Copy());

                if (!historyState.AllLegalMovesFor(player).Any())
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        public BoardType CreateTimeline(MoveHistory selectedHistory)
        {
            var newState = new GameState(selectedHistory.NextPlayer, selectedHistory.Board.Copy());
            var timeline = selectedHistory.NextPlayer == Player.White
                ? BoardType.TimelineWhite : BoardType.TimelineBlack;
            boardStates[timeline] = newState;
            boardTimers[timeline] = new BoardTimer();
            return timeline;
        }

        public bool CanWhiteCaptureKing()
        {
            foreach (var state in boardStates.Values)
            {
                Board board = state.Board;

                if (state.CurrentPlayer != Player.White)
                {
                    continue;
                }

                foreach (Position pos in board.PiecePositionsFor(Player.White))
                {
                    Piece piece = board[pos];

                    if (piece.CanCaptureOpponentKing(pos, board))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool CanBlackCaptureKing()
        {
            foreach (var state in boardStates.Values)
            {
                Board board = state.Board;

                if (state.CurrentPlayer != Player.Black)
                {
                    continue;
                }

                foreach (Position pos in board.PiecePositionsFor(Player.Black))
                {
                    Piece piece = board[pos];

                    if (piece.CanCaptureOpponentKing(pos, board))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public Result CheckForKingCapture()
        {
            bool whiteCanCapture = CanWhiteCaptureKing();
            bool blackCanCapture = CanBlackCaptureKing();

            if (CurrentPlayer == Player.White && whiteCanCapture)
            {
                return Result.Win(Player.White, EndReason.KingCapture);
            }
            else if (CurrentPlayer == Player.Black && blackCanCapture)
            {
                return Result.Win(Player.Black, EndReason.KingCapture);
            }

            return null;
        }

        public void CheckForGameOver()
        {
            if (Result != null)
            {
                return;
            }

            Result kingCaptureResult = CheckForKingCapture();

            if (kingCaptureResult != null)
            {
                Result = kingCaptureResult;
                return;
            }

            BoardType? forcedBoard = GetTimerForcedBoard();

            List<BoardType> boardsToCheck = forcedBoard.HasValue
                ? new List<BoardType> { forcedBoard.Value }
                : GetPlayableBoards();

            Result boardResult = CheckPlayableBoardsForGameOver(boardsToCheck);

            if (boardResult != null)
            {
                if (!forcedBoard.HasValue && boardsToCheck.Count == 1
                    && boardResult.Reason == EndReason.Checkmate
                    && CanCreateTimeline(boardsToCheck[0], CurrentPlayer))
                {
                    return;
                }

                Result = boardResult;
                return;
            }

            if (SeventyFiveMoveRule())
            {
                Result = Result.Draw(EndReason.SeventyFiveMoveRule);
                return;
            }

            if (FivefoldRepetition())
            {
                Result = Result.Draw(EndReason.FivefoldRepetition);
                return;
            }
        }

        public bool ShouldOfferDraw()
        {
            return FiftyMoveRule() || ThreefoldRepetition();
        }

        private Result CheckBoardForGameOver(BoardType boardType)
        {
            GameState state = boardStates[boardType];

            List<Move> legalMoves =
                state.AllLegalMovesFor(CurrentPlayer).ToList();

            bool isInCheck = state.Board.IsInCheck(CurrentPlayer);

            if (legalMoves.Any())
            {
                return null;
            }

            if (isInCheck)
            {
                return Result.Win(CurrentPlayer.Opponent(), EndReason.Checkmate);
            }

            return Result.Draw(EndReason.Stalemate);
        }

        private Result CheckPlayableBoardsForGameOver(List<BoardType> boardsToCheck)
        {
            bool hasLegalMoves = false;
            bool hasCheckmate = false;
            bool hasStalemate = false;

            foreach (BoardType boardType in boardsToCheck)
            {
                Result result = CheckBoardForGameOver(boardType);

                if (result == null)
                {
                    hasLegalMoves = true;
                }
                else if (result.Reason == EndReason.Checkmate)
                {
                    hasCheckmate = true;
                }
                else if (result.Reason == EndReason.Stalemate)
                {
                    hasStalemate = true;
                }
            }

            if (hasLegalMoves)
            {
                return null;
            }

            if (hasCheckmate)
            {
                return Result.Win(CurrentPlayer.Opponent(), EndReason.Checkmate);
            }

            if (hasStalemate)
            {
                return Result.Draw(EndReason.Stalemate);
            }

            return null;
        }

        public void UpdateMoveCounter(MoveResult result)
        {
            if (result.CaptureOrPawn)
            {
                noCaptureOrPawnMoves = 0;
            }
            else
            {
                noCaptureOrPawnMoves++;
            }
        }

        public bool FiftyMoveRule()
        {
            return noCaptureOrPawnMoves == 100;
        }

        public bool SeventyFiveMoveRule()
        {
            return noCaptureOrPawnMoves == 150;
        }

        public bool ThreefoldRepetition()
        {
            foreach (GameState state in boardStates.Values)
            {
                if (state.ThreefoldRepetition())
                {
                    return true;
                }
            }

            return false;
        }

        public bool FivefoldRepetition()
        {
            foreach (GameState state in boardStates.Values)
            {
                if (state.FivefoldRepetition())
                {
                    return true;
                }
            }

            return false;
        }

        public void Resign()
        {
            if (Result != null)
            {
                return;
            }

            Result = Result.Win(CurrentPlayer.Opponent(), EndReason.Resignation);
        }

        public bool OfferDraw()
        {
            if (Result != null || DrawOfferBy != null)
                return false;

            DrawOfferBy = CurrentPlayer;
            return true;
        }

        public void RejectDraw()
        {
            DrawOfferBy = null;
        }

        public void AcceptDraw()
        {
            if (Result != null || DrawOfferBy == null)
                return;

            Result = Result.Draw(EndReason.DrawAgreement);
            DrawOfferBy = null;
        }
    }
}
