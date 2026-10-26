using System.Security.Cryptography.X509Certificates;

namespace ChessLogic
{
    public class GameSession
    {
        private readonly Dictionary<BoardType, GameState> boardStates = new();
        private readonly Dictionary<BoardType, BoardTimer> boardTimers = new();
        public Player CurrentPlayer { get; private set; }
        private const int BoardTimerLimit = 4;

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

        public List<BoardType> GetTimerForcedBoard()
        {
            return GetPlayableBoards()
                .Where(boardType => IsTimerLimit(boardType, CurrentPlayer))
                .ToList();
        }

        public bool CanPlayOnBoard(BoardType boardType)
        {
            List<BoardType> forcedBoards = GetTimerForcedBoard();

            if (forcedBoards.Count > 0)
            {
                return forcedBoards.Contains(boardType);
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

        public BoardType CreateTimeline(MoveHistory selectedHistory)
        {
            var newState = new GameState(selectedHistory.NextPlayer, selectedHistory.Board.Copy());
            var timeline = selectedHistory.NextPlayer == Player.White
                ? BoardType.TimelineWhite : BoardType.TimelineBlack;
            boardStates[timeline] = newState;
            boardTimers[timeline] = new BoardTimer();
            return timeline;
        }

        public void RemovePieceFromAllBoards(Guid capturedId)
        {
            foreach (var state in boardStates.Values)
            {
                state.Board.RemovePieceWithID(capturedId);
            }
        }

        public bool CanWhiteCaptureKing()
        {
            foreach (var state in boardStates.Values)
            {
                Board board = state.Board;

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
    }
}
