using Dapper;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Eventer
{
    public class SQLiteDataAccess
    {
        DataTable tb = new DataTable();
        //string connection = 

        private static string LoadConnString()
        {
            return ConfigurationManager.ConnectionStrings["TimetableEventString"].ConnectionString;
        }

        public DataTable LoadAllEvents()
        {
            using (SQLiteConnection nConnection = new SQLiteConnection(LoadConnString()))
            {
                tb = new DataTable();
                var queryString = "SELECT ev_id,ev_title,ev_desc,ev_dt FROM UserEventTable";
                var nAdapter = new SQLiteDataAdapter(queryString,nConnection);

                nAdapter.Fill(tb);
                return tb;
            }
        }

        public List<TimetableEvent> LoadEvents()
        {
            using (SQLiteConnection nConnection = new SQLiteConnection(LoadConnString()))
            {
                nConnection.Open();
                var allEvents = nConnection.Query<TimetableEvent>("SELECT ev_id,ev_title,ev_desc,ev_dt FROM UserEventTable");
                nConnection.Close();
                return allEvents.ToList();
            }
        }

        public DataTable LoadUpcoming(DateTime startDate, int days)
        {
            using (SQLiteConnection nConnection = new SQLiteConnection(LoadConnString()))
            {
                tb = new DataTable();

                var queryString = "SELECT ev_id,ev_title,ev_desc,ev_dt, CAST((julianday(ev_dt)-julianday(@startDt)) AS Integer) AS 'daydiff' FROM UserEventTable WHERE daydiff<@daysAmt AND ev_dt>=@startDt";
                var nAdapter = new SQLiteDataAdapter(queryString, nConnection);
                
                nAdapter.SelectCommand.Parameters.AddWithValue("@daysAmt", days);
                nAdapter.SelectCommand.Parameters.AddWithValue("@startDt", startDate);

                nAdapter.Fill(tb);

                nAdapter.SelectCommand.Connection.Close();
                return tb;
            }
        }

        public void SaveEvent(TimetableEvent ev)
        {
            using (SQLiteConnection nConnection = new SQLiteConnection(LoadConnString()))
            {
                SQLiteCommand insertCmd = new SQLiteCommand();
                insertCmd.CommandText = "INSERT INTO UserEventTable (ev_title,ev_desc,ev_dt) VALUES (@title,@desc,@dt)";
                insertCmd.Connection = nConnection;

                insertCmd.Parameters.AddWithValue("@title", ev.title);
                insertCmd.Parameters.AddWithValue("@desc", ev.desc);
                insertCmd.Parameters.AddWithValue("@dt", ev.date);

                try
                {
                    insertCmd.Connection.Open();
                    insertCmd.ExecuteNonQuery();
                    insertCmd.Connection.Close();
                }
                catch (Exception ex) { throw new Exception(ex.Message); }
            }
        }

        public void DeleteAll()
        {
            using (SQLiteConnection nConnection = new SQLiteConnection(LoadConnString()))
            {
                nConnection.Execute("DELETE FROM UserEventTable");
                nConnection.Execute("DELETE FROM sqlite_sequence WHERE name = 'UserEventTable'");
            }
        }

        public void UpdateEvent(int ind, TimetableEvent ev)
        {
            using (SQLiteConnection nConnection = new SQLiteConnection(LoadConnString()))
            {
                string cmdText = "UPDATE UserEventTable SET ev_title=@title,ev_desc=@desc,ev_dt=@dt WHERE ev_id=@id";
                SQLiteCommand updateCmd = new SQLiteCommand(cmdText,nConnection);

                updateCmd.Parameters.AddWithValue("@id", ind);
                updateCmd.Parameters.AddWithValue("@title", ev.title);
                updateCmd.Parameters.AddWithValue("@desc", ev.desc);
                updateCmd.Parameters.AddWithValue("@dt", ev.date);

                try
                {
                    updateCmd.Connection.Open();
                    updateCmd.ExecuteNonQuery();
                    updateCmd.Connection.Close();
                }
                catch (Exception ex) { throw new Exception(ex.Message); }
            }
        }
    }

    public class TimetableEvent
    {
        public int id { get; set; }
        public string title { get; set; }
        public string desc { get; set; }
        public string date { get; set; }
    }

    public partial class MainWindow : Window
    {
        int editedIndex;
        List<string> mtList;

        public MainWindow()
        {
            InitializeComponent();
            InitializeMonthList();

            ClearCurrentlyEdited();
            AssignDateRelatedFields();

            LoadEventList();
            UpcomingEventsDataGrid.ItemsSource = new SQLiteDataAccess().LoadUpcoming(DateTime.Now, Properties.Settings.Default.daysToShow).AsDataView();

            MtComboBox.ItemsSource = mtList;
            New_CmbMt.ItemsSource = MtComboBox.ItemsSource;
        }

        //go to specific month
        private void GoToMonth_Click(object sender, RoutedEventArgs e)
        {
            if (ChkBox_Today.IsChecked == true)
            {
                UpcomingEventsDataGrid.ItemsSource = new SQLiteDataAccess().LoadUpcoming(DateTime.Now, Properties.Settings.Default.daysToShow).AsDataView();
            }

            if (int.TryParse(YrTextBox.Text, out int yrInt))
            {
                var nTime = new DateTime(yrInt, MtComboBox.SelectedIndex + 1, 1, 12, 00, 00);
                var days = DateTime.DaysInMonth(nTime.Year, nTime.Month);
                UpcomingEventsDataGrid.ItemsSource = new SQLiteDataAccess().LoadUpcoming(nTime, days).AsDataView();
            }
        }

        private void ChkBox_Today_Checked(object sender, RoutedEventArgs e)
        {
            MtComboBox.IsEnabled = false;
            YrTextBox.IsEnabled = false;

            YrTextBox.Text = string.Empty;
        }

        private void ChkBox_Today_Unchecked(object sender, RoutedEventArgs e)
        {
            MtComboBox.IsEnabled = true;
            YrTextBox.IsEnabled = true;
        }

        void UnlockTodayBox(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ChkBox_Today.IsChecked = false;
        }

        private void YrTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!NumericalStringsValidation("year",YrTextBox.Text)) { YrTextBox.Text = string.Empty; }
        }

        //adding and editing events
        private void AddEventButton_Click(object sender, RoutedEventArgs e)
        {
            var nEvent = new TimetableEvent();

            nEvent.title = TxtBlock_EvName.Text;
            nEvent.desc = TxtBlock_EvDesc.Text;
            nEvent.date = New_YrTextBox.Text + "-" + (New_CmbMt.SelectedIndex + 1) + "-" + New_TxtDay.Text + " " + New_HrTextBox.Text + ":" + New_MinTextBox.Text + ":" + "00";

            if (editedIndex > -1)
            {
                new SQLiteDataAccess().UpdateEvent(editedIndex, nEvent);
            }
            else
            {
                new SQLiteDataAccess().SaveEvent(nEvent);
            }

            var nEventDate = new DateTime(int.Parse(New_YrTextBox.Text), New_CmbMt.SelectedIndex + 1, int.Parse(New_TxtDay.Text));
            var tSpan = nEventDate.Subtract(DateTime.Now).TotalDays;

            LoadEventList();
            if (tSpan < Properties.Settings.Default.daysToShow) { LoadUpcomingEventDataGrid(); }

            ClearGoalInputFields();
            ClearCurrentlyEdited();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e) { ClearCurrentlyEdited(); }

        private void AllEventsDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            LoadGoalToEdit();
        }

        void LoadGoalToEdit()
        {
            var rowViewToLoad = (DataRowView)AllEventsDataGrid.SelectedItem;
            var aa = rowViewToLoad.Row;

            TxtBlock_EvName.Text = aa[1].ToString();
            TxtBlock_EvDesc.Text = aa[2].ToString();

            string fullDateString = aa[3].ToString();
            fullDateString = fullDateString.Replace(" ", "-");
            fullDateString = fullDateString.Replace(":", "-");
            string[] dateStringElements = fullDateString.Split(new char[] { '-' });

            New_YrTextBox.Text = dateStringElements[0];
            New_CmbMt.SelectedIndex = int.Parse(dateStringElements[1]) - 1;
            New_TxtDay.Text = dateStringElements[2];

            New_HrTextBox.Text = dateStringElements[3];
            New_MinTextBox.Text = dateStringElements[4];

            editedIndex = int.Parse(aa[0].ToString());
        }

        private void New_HrTextBox_LostFocus(object sender, RoutedEventArgs e) { if (!NumericalStringsValidation("hour", New_HrTextBox.Text)) { New_HrTextBox.Text = "00"; } }

        private void New_HrTextBox_GotFocus(object sender, RoutedEventArgs e) { New_HrTextBox.Text = string.Empty; }

        private void New_MinTextBox_LostFocus(object sender, RoutedEventArgs e) { if (!NumericalStringsValidation("minute", New_MinTextBox.Text)) { New_MinTextBox.Text = "00"; } }

        private void New_MinTextBox_GotFocus(object sender, RoutedEventArgs e) { New_MinTextBox.Text = string.Empty; }

        private void New_YrTextBox_LostFocus(object sender, RoutedEventArgs e) { if (!NumericalStringsValidation("year", New_YrTextBox.Text)) { New_YrTextBox.Text = string.Empty; } }

        private void New_TxtDay_LostFocus(object sender, RoutedEventArgs e) { if (!NumericalStringsValidation("day", New_TxtDay.Text)) { New_TxtDay.Text = string.Empty; } }

        //initialization
        private void AssignDateRelatedFields()
        {
            var today = DateTime.Now;
            CurrentDateTxt.Text += today.Day.ToString() + "." + today.Month.ToString() + "." + today.Year.ToString();

            UpcomingLbl.Text += " " + mtList[today.Month - 1] + " " + today.Year.ToString();
        }

        void LoadUpcomingEventDataGrid()
        {
            UpcomingEventsDataGrid.ItemsSource = new SQLiteDataAccess().LoadUpcoming(DateTime.Now, Properties.Settings.Default.daysToShow).AsDataView();
        }

        void LoadEventList()
        {
            AllEventsDataGrid.ItemsSource = new SQLiteDataAccess().LoadAllEvents().AsDataView();
        }

        //other
        private void InitializeMonthList()
        {
            mtList = new List<string> { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e) { this.Close(); }

        private void OptionButton_Click(object sender, RoutedEventArgs e)
        {
            var nOptions = new OptionWindow();
            nOptions.Show();
        }

        private void DiscardButton_Click(object sender, RoutedEventArgs e)
        {
            ClearGoalInputFields();
            ClearCurrentlyEdited();
        }

        private void ClearGoalInputFields()
        {
            TxtBlock_EvName.Text = string.Empty;
            TxtBlock_EvDesc.Text = string.Empty;
            New_YrTextBox.Text = string.Empty;
            New_TxtDay.Text = string.Empty;
            New_HrTextBox.Text = "00";
            New_MinTextBox.Text = "00";
        }

        private void ClearCurrentlyEdited() { editedIndex = -1; }

        private void New_CmbMt_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (New_TxtDay.Text != string.Empty)
            {
                if (!NumericalStringsValidation("day", New_TxtDay.Text))
                {
                    New_TxtDay.Text = string.Empty;
                }
            }
        }

        private void AllEventsDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            EditBtn.IsEnabled = AllEventsDataGrid.SelectedItem != null ? true : false;
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e) { LoadGoalToEdit(); }

        bool NumericalStringsValidation(string validationMode, string valString)
        {
            bool a = false;

            switch (validationMode)
            {
                case "hour":
                    if (int.TryParse(valString,out int hrInt) && hrInt < 24 && hrInt > -1) { a = true; }
                    break;
                case "minute":
                    if (int.TryParse(valString, out int minInt) && minInt < 60 && minInt > -1) { a = true; }
                    break;
                case "day":
                    if (int.TryParse(valString, out int dayInt))
                    {
                        if (New_CmbMt.SelectedIndex < 0) { if (dayInt < 32) { a = true; } }
                        else if (New_YrTextBox.Text == string.Empty) { if (dayInt <= DateTime.DaysInMonth(DateTime.Now.Year, New_CmbMt.SelectedIndex + 1)) { a = true; } }
                        else { if (dayInt <= DateTime.DaysInMonth(int.Parse(New_YrTextBox.Text), New_CmbMt.SelectedIndex + 1)) { a = true; } }
                    }
                    break;
                case "year":
                    if (int.TryParse(valString, out int yrInt) && yrInt < 10000 && yrInt > 1900) { a = true; }
                    break;
                default:
                    break;
            }

            return a;
        }

    }
}
