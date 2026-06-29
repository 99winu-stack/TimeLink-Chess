using ChessLogic;
using System.Windows.Controls;
using System.Windows.Input;

namespace GameUI
{
    /// <summary>
    /// Interaktionslogik für PromotionMenu.xaml
    /// </summary>
    public partial class PromotionMenu : UserControl
    {
        public event Action<PieceType> PieceSelected;
        public PromotionMenu(Player player)
        {
            InitializeComponent();

            QueenIMG.Source = Images.GetImage(player, PieceType.Queen);
            BishopIMG.Source = Images.GetImage(player, PieceType.Bishop);
            RookIMG.Source = Images.GetImage(player, PieceType.Rook);
            KnightIMG.Source = Images.GetImage(player, PieceType.Knight);
        }

        private void QueenIMG_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PieceSelected?.Invoke(PieceType.Queen);
        }
        private void BihopIMG_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PieceSelected?.Invoke(PieceType.Bishop);
        }
        private void RookIMG_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PieceSelected?.Invoke(PieceType.Rook);
        }
        private void KnightIMG_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PieceSelected?.Invoke(PieceType.Knight);
        }
    }
}
