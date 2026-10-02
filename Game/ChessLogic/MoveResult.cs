using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessLogic
{
    public class MoveResult
    {
        public Piece CapturedPiece { get; }
        public bool CaptureOrPawn { get; }

        public MoveResult(Piece capturedPiece, bool captureOrPawn)
        {
            CapturedPiece = capturedPiece;
            CaptureOrPawn = captureOrPawn;
        }
    }
}
