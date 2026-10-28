using System;
using System.Windows;
using System.Windows.Controls;

namespace GameUI
{
    public partial class InformationMenu : UserControl
    {
        public event Action OkSelected;

        public InformationMenu(string message)
        {
            InitializeComponent();
            MessageText.Text = message;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            OkSelected?.Invoke();
        }
    }
}
