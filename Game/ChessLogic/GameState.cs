namespace ChessLogic
{
    public class GameState
    {
        public Board Board { get; }
        public Player CurrentPlayer { get; private set; }
        public Result Result { get; private set; } = null;
        private readonly List<MoveHistory> moveHistory = new List<MoveHistory>();
        public IReadOnlyList<MoveHistory> MoveHistory => moveHistory.AsReadOnly();
        private string stateString;
        private readonly Dictionary<string, int> stateHistory = new Dictionary<string, int>();

        public GameState(Player player, Board board)
        {
            CurrentPlayer = player;
            Board = board;

            stateString = new StateString(CurrentPlayer, board).ToString();
            stateHistory[stateString] = 1;
        }

        public IEnumerable<Move> LegalMovesForPiece(Position pos)
        {
            if (Board.IsEmpty(pos) || Board[pos].Color != CurrentPlayer)
            {
                return Enumerable.Empty<Move>();
            }

            Piece piece = Board[pos];
            IEnumerable<Move> MoveCandidates = piece.GetMoves(pos, Board);
            return MoveCandidates.Where(move => move.IsLegal(Board));
        }

        public MoveResult MakeMove(Move move)
        {
            Board.SetPawnSkipPosition(CurrentPlayer, null);
            MoveResult result = move.Execute(Board);

            moveHistory.Add(new MoveHistory(move, Board.Copy(), CurrentPlayer.Opponent()));

            if (result.CaptureOrPawn)
            {
                stateHistory.Clear();
            }

            CurrentPlayer = CurrentPlayer.Opponent();
            UpdateStateString();

            return result;
        }

        public IEnumerable<Move> AllLegalMovesFor(Player player)
        {
            IEnumerable<Move> moveCandidates = Board.PiecePositionsFor(player).SelectMany(pos =>
            {
                Piece piece = Board[pos];
                return piece.GetMoves(pos, Board);
            });

            return moveCandidates.Where(move => move.IsLegal(Board));
        }

        private void UpdateStateString()
        {
            stateString = new StateString(CurrentPlayer, Board).ToString();

            if (!stateHistory.ContainsKey(stateString))
            {
                stateHistory[stateString] = 1;
            }
            else
            {
                stateHistory[stateString]++;
            }
        }

        public bool ThreefoldRepetition()
        {
            return stateHistory[stateString] == 3;
        }
        public bool FivefoldRepetition()
        {
            return stateHistory[stateString] == 5;
        }
    }
}
