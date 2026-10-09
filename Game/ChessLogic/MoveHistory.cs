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
        public Player NextPlayer {  get; }
        public Player? CapturePlayer { get; }

        public MoveHistory(Move move, Board board, Player nextPlayer, Player? capturePlayer)
        {
            Move = move;
            Board = board;
            NextPlayer = nextPlayer;
            CapturePlayer = capturePlayer;
        }

        public override string ToString()
        {
            return $"{Move.FromPos} -> {Move.ToPos}";
        }
    }
}