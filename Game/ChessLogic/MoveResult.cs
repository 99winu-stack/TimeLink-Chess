using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessLogic
{
    public class MoveResult
    {
        public Guid? CapturedPieceId { get; }
        public bool CaptureOrPawn { get; }

        public MoveResult(Guid? capturedPieceId, bool captureOrPawn)
        {
            CapturedPieceId = capturedPieceId;
            CaptureOrPawn = captureOrPawn;
        }
    }
}
