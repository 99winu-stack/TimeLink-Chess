namespace ChessLogic
{
    public enum EndReason
    {
        Checkmate,
        Stalemate,
        FiftyMoveRule,
        SeventyFiveMoveRule,
        InsufficientMaterial,
        ThreefoldRepetition,
        KingCapture,
        Resignation,
        DrawAgreement
    }
}
