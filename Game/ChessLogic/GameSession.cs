namespace ChessLogic
{
    public class GameSession
    {
        private readonly Dictionary<BoardType, GameState> boardStates = new();

        public GameSession(GameState initial)
        {
            boardStates[BoardType.Main] = initial;
        }

        public GameState GetBoardState(BoardType boardType) =>
            boardStates.TryGetValue(boardType, out var s) ? s : null;

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
            return timeline;
        }

        public void RemovePieceFromAllBoards(Guid capturedId)
        {
            foreach (var state in boardStates.Values)
            {
                state.Board.RemovePieceWithID(capturedId);
            }
        }
    }
}
