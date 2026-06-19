namespace ChessLogic
{
    /// <summary>
    /// define the player
    /// used to determine pieces ownership, player turns, the winner or draw
    /// </summary>
    public enum Player
    {
        None,
        White,
        Black
    }

    public static class PlayerExtensions
    {
        public static Player Opponent(this Player player)
        {
            return player switch
            {
                Player.White => Player.Black,
                Player.Black => Player.White,
                _ => Player.None,
            };
        }
    }
}
