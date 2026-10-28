using ChessLogic;
using System.Windows;
using System.Windows.Controls;

namespace GameUI
{
    /// <summary>
    /// Interaktionslogik für GameOverMenu.xaml
    /// </summary>
    public partial class GameOverMenu : UserControl
    {
        public event Action<Option> OptionSelected;

        public GameOverMenu(GameSession gameSession)
        {
            InitializeComponent();

            Result result = gameSession.Result;
            WinnerText.Text = GetWinnerText(result.Winner);
            ReasonText.Text = GetReassonText(result.Reason, gameSession.CurrentPlayer);
        }

        private static string GetWinnerText(Player winner)
        {
            return winner switch
            {
                Player.White => "WHITE WINS!",
                Player.Black => "BLACK WINS!",
                _ => "DRAW"
            };
        }

        private static string PlayerString(Player player)
        {
            return player switch
            {
                Player.White => "WHITE",
                Player.Black => "BLACK",
                _ => ""
            };
        }

        private static string GetReassonText(EndReason reason, Player currentPlayer)
        {
            return reason switch
            {
                EndReason.Stalemate => $"STALEMATE - {PlayerString(currentPlayer)} CAN´T MOVE",
                EndReason.Checkmate => $"CHECKMATE - {PlayerString(currentPlayer)} CAN´T MOVE",
                EndReason.SeventyFiveMoveRule => "SEVENTY-FIVE-MOVE RULE",
                EndReason.InsufficientMaterial => "INSUFFICIENT MATERIAL",
                EndReason.FivefoldRepetition => "FIVEFOLD REPETITION",
                EndReason.KingCapture => "KING CAN BE CAPTURED",
                _ => ""
            };
        }

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            OptionSelected?.Invoke(Option.Restart);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            OptionSelected?.Invoke(Option.Exit);
        }
    }
}
