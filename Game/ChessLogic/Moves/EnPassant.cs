namespace ChessLogic
{
    public class EnPassant : Move
    {
        public override MoveType Type => MoveType.EnPassant;
        public override Position FromPos { get; }
        public override Position ToPos { get; }
        private readonly Position capturePos;

        public EnPassant(Position from, Position to)
        {
            FromPos = from;
            ToPos = to;
            capturePos = new Position(from.Row, to.Column);
        }

        public override MoveResult Execute(Board board)
        {
            Piece capturedPiece = board[capturePos];

            new NormalMove(FromPos, ToPos).Execute(board);
            board[capturePos] = null;

            return new MoveResult(capturedPiece, true);
        }
    }
}
