namespace ChessLogic
{
    public class PawnPromotion : Move
    {
        public override MoveType Type => MoveType.PawnPromotion;
        public override Position FromPos { get; }
        public override Position ToPos { get; }
        private readonly PieceType newType;
        public PawnPromotion(Position from, Position to, PieceType newType)
        {
            FromPos = from;
            ToPos = to;
            this.newType = newType;
        }

        private Piece CreatePromotionPiece(Player color, Guid pawnId)
        {
            return newType switch
            {
                PieceType.Knight => new Knight(color, pawnId),
                PieceType.Bishop => new Bishop(color, pawnId),
                PieceType.Rook => new Rook(color, pawnId),
                _ => new Queen(color, pawnId)
            };
        }

        public override MoveResult Execute(Board board)
        {
            Piece pawn = board[FromPos];
            Piece capturedPiece = board[ToPos];

            board[FromPos] = null;

            Piece promotionPiece = CreatePromotionPiece(pawn.Color, pawn.Id);
            promotionPiece.HasMoved = true;
            board[ToPos] = promotionPiece;

            return new MoveResult(capturedPiece?.Id, true);
        }
    }
}
