using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessLogic
{
    public class GameState
    {
        public Board Board { get; }
        public Player CurentPlayer { get; private set; }

        public GameState(Player player, Board board)
        {
            CurentPlayer = player;
            Board = board;
        }
    }
}
