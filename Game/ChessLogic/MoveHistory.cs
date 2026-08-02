using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessLogic
{
    public class MoveHistory
    {
        public Move Move { get; }
        public Board Board { get; }

        public MoveHistory(Move move, Board board)
        {
            Move = move;
            Board = board;
        }
    }
}
