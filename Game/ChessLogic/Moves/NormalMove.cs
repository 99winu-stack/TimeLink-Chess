namespace ChessLogic
{
    public class NormalMove : Move
    {
        public override MoveType Type => MoveType.Normal;
        public override Position FromPos { get; }
        public override Position ToPos { get; }

        public NormalMove(Position from, Position to)
        {
            FromPos = from;
            ToPos = to;
        }

        public override MoveResult Execute(Board board)
        {
            Piece piece = board[FromPos];
            Piece capturedPiece = board[ToPos];

            bool capture = capturedPiece != null;

            board[ToPos] = piece;
            board[FromPos] = null;
            piece.HasMoved = true;

            return new MoveResult(capturedPiece, capture || piece.Type == PieceType.Pawn);
        }
    }
}
