using System.Windows.Media;
using System.Windows.Media.Imaging;
using ChessLogic;

namespace GameUI
{
    public static class Images
    {
        private static readonly Dictionary<PieceType, ImageSource> whiteSources = new()
        {
            { PieceType.Pawn, LoadImage("Assets/pawnP.png") },
            { PieceType.Rook, LoadImage("Assets/rookP.png") },
            { PieceType.Knight, LoadImage("Assets/knightP.png") },
            { PieceType.Bishop, LoadImage("Assets/bishopP.png") },
            { PieceType.Queen, LoadImage("Assets/queenP.png") },
            { PieceType.King, LoadImage("Assets/kingP.png") },
        };
        private static readonly Dictionary<PieceType, ImageSource> blackSources = new()
        {
            { PieceType.Pawn, LoadImage("Assets/pawnB.png") },
            { PieceType.Rook, LoadImage("Assets/rookB.png") },
            { PieceType.Knight, LoadImage("Assets/knightB.png") },
            { PieceType.Bishop, LoadImage("Assets/bishopB.png") },
            { PieceType.Queen, LoadImage("Assets/queenB.png") },
            { PieceType.King, LoadImage("Assets/kingB.png") },
        };
        public static ImageSource LoadImage(string filePath)
        {
            return new BitmapImage(new Uri(filePath, UriKind.Relative));
        }

        public static ImageSource GetImage(Player color, PieceType type)
        {
            return color switch
            {
                Player.White => whiteSources[type],
                Player.Black => blackSources[type],
                _ => null
            };
        }

        public static ImageSource GetImage(Piece piece)
        {
            if(piece == null)
            {
                return null;
            }
            return GetImage(piece.Color, piece.Type);
        }
    }
}
