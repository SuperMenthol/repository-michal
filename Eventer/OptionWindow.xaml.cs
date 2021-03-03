using System;
using System.Windows;

namespace Eventer
{
    public partial class OptionWindow : Window
    {
        public OptionWindow()
        {
            InitializeComponent();
            DaySlider.Value = Properties.Settings.Default.daysToShow;
            Txt_DaysToShow.Text = DaySlider.Value.ToString();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ChkBox_ResetDb.IsChecked == true)
            {
                new SQLiteDataAccess().DeleteAll();
            }

            if (DaySlider.Value != Properties.Settings.Default.daysToShow)
            {
                Properties.Settings.Default.daysToShow = Convert.ToInt32(DaySlider.Value);
            }

            this.Close();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e) { this.Close(); }

        private void ChkBox_ResetDb_Checked(object sender, RoutedEventArgs e)
        {
            Lbl_ResetTable.Visibility = Visibility.Visible;
        }

        private void ChkBox_ResetDb_Unchecked(object sender, RoutedEventArgs e)
        {
            Lbl_ResetTable.Visibility = Visibility.Hidden;
        }

        private void DaySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
        }
    }
}
