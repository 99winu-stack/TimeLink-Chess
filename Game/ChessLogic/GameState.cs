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
        public Result Result { get; private set; } = null;

        private int noCaptureOrPawnMoves = 0;

        public GameState(Player player, Board board)
        {
            CurentPlayer = player;
            Board = board;
        }

        public IEnumerable<Move> LegalMovesForPiece(Position pos)
        {
            if (Board.IsEmpty(pos) || Board[pos].Color != CurentPlayer)
            {
                return Enumerable.Empty<Move>();
            }

            Piece piece = Board[pos];
            IEnumerable<Move> MoveCandidates = piece.GetMoves(pos, Board);
            return MoveCandidates.Where(move => move.IsLegal(Board));
        }

        public void MakeMove(Move move)
        {
            Board.SetPawnSkipPosition(CurentPlayer, null);
            bool captureOrPawn = move.Execute(Board);

            if (captureOrPawn)
            {
                noCaptureOrPawnMoves = 0;
            }
            else
            {
                noCaptureOrPawnMoves++;
            }

            CurentPlayer = CurentPlayer.Opponent(); 
            CheckForGameOver();
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

        private void CheckForGameOver()
        {
            if (!AllLegalMovesFor(CurentPlayer).Any())
            {
                if (Board.IsInCheck(CurentPlayer))
                {
                    Result = Result.Win(CurentPlayer.Opponent());
                }
                else
                {
                    Result = Result.Draw(EndReason.Stalemate);
                }
            }
            else if(Board.InsufficientMaterial())
            {
                Result = Result.Draw(EndReason.InsufficientMaterial);
            }
            else if (FiftyMoveRule())
            {
                Result = Result.Draw(EndReason.FiftyMoveRule);
            }
        }

        public bool IsGameOver()
        {
            return Result != null;
        }

        private bool FiftyMoveRule()
        {
            int fullMoves = noCaptureOrPawnMoves / 2;
            return fullMoves >= 50;
        }
    }
}
