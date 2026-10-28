using System;
using System.Windows;
using System.Windows.Controls;

namespace GameUI
{
    public partial class ConfirmationMenu : UserControl
    {
        public event Action<bool> ConfirmationSelected;

        public ConfirmationMenu(string question)
        {
            InitializeComponent();
            QuestionText.Text = question;
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            ConfirmationSelected?.Invoke(true);
        }

        private void No_Click(object sender, RoutedEventArgs e)
        {
            ConfirmationSelected?.Invoke(false);
        }
    }
}
