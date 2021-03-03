using System;
using System.Collections.Generic;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Timers;

namespace TimerApp
{
    public partial class MainWindow : Window
    {
        bool timerRunning;
        DateTime startTime;

        TextBox[] timeBoxes;

        int mainTimeInMilliseconds;
        int intervalTimeInMilliseconds;
        int longIntervalTime;
        int timeToMeasure;

        int repsForLongBreak;
        int currentLongSeries;

        bool intervalCountdown;

        int reps;
        bool paused;
        
        Timer aTimer;
        SolidColorBrush timeBoxBrush;
        SoundPlayerAction snd;

        bool timerConfigured;

        public MainWindow()
        {
            InitializeComponent();
            LoadInitialSettings();
        }

        private void ModeCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BreakTxtBox.Background = Brushes.White;
            RepTxtBox.Background = Brushes.White;
            timerConfigured = false;
            switch (ModeCmb.SelectedIndex)
            {
                case 0:
                    MainTxtBox.IsEnabled = false;
                    InterTxtBox.IsEnabled = false;

                    UpcomingTime1.Visibility = Visibility.Hidden;
                    UpcomingTime2.Visibility = Visibility.Hidden;
                    break;
                case 1:
                    MainTxtBox.IsEnabled = true;
                    InterTxtBox.IsEnabled = true;

                    UpcomingTime1.Visibility = Visibility.Visible;
                    UpcomingTime2.Visibility = Visibility.Visible;
                    break;
                case 2:
                    MainTxtBox.Text = "1500";
                    InterTxtBox.Text = "300";

                    UpcomingTime1.Visibility = Visibility.Visible;
                    UpcomingTime2.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }

        private void SetTimeTranslationText(int secInput) 
        { 
            int minRem = secInput % 60;
            if (secInput > 59)
            {
                TimeTranslation.Visibility = Visibility.Visible;

                if (secInput / 60 > 59)
                {
                    int hInput = secInput / 3600;
                    int hRem = secInput % 3600;
                    int remSec = (secInput - hInput * 3600) % 60;
                    TimeTranslation.Text = secInput.ToString() + " seconds translate to " + hInput + " hours, " + hRem / 60 + " minutes and " + remSec + " seconds";
                }
                else { TimeTranslation.Text = secInput.ToString() + " seconds translate to " + secInput / 60 + " minutes and " + minRem + " seconds"; }
            }
            else { TimeTranslation.Visibility = Visibility.Hidden; }
        }

        private void StartAtCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (StartAtCheckBox.IsChecked == true)
            {
                StartInTxtBox.Text = string.Empty;

                StartHrTxtBox.IsEnabled = true;
                StartMinTxtBox.IsEnabled = true;
                StartInTxtBox.IsEnabled = false;

                StartInCheckBox.IsChecked = false;
            }
            else
            {
                StartHrTxtBox.Text = string.Empty;
                StartMinTxtBox.Text = string.Empty;

                StartHrTxtBox.IsEnabled = false;
                StartMinTxtBox.IsEnabled = false;
                StartInTxtBox.IsEnabled = true;
            }
        }

        private void StartInCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (StartInCheckBox.IsChecked == true)
            {
                StartHrTxtBox.Text = string.Empty;
                StartMinTxtBox.Text = string.Empty;

                StartHrTxtBox.IsEnabled = false;
                StartMinTxtBox.IsEnabled = false;
                StartInTxtBox.IsEnabled = true;

                StartAtCheckBox.IsChecked = false;
            }
            else
            {
                StartInTxtBox.Text = string.Empty;

                StartHrTxtBox.IsEnabled = false;
                StartMinTxtBox.IsEnabled = false;
                StartInTxtBox.IsEnabled = false;
            }
        }

        private void BeginTimer()
        {
            var nTimer = new Timer();
            if (!timerConfigured) { ConfigureTimer(ModeCmb.SelectedIndex, nTimer); }

            UpdateLastChosenProperties();

            if (!timerRunning)
            {
                timerRunning = true;
                startTime = DateTime.Now;

                if (paused)
                {
                    timeToMeasure = (int)TimeSpan.Parse(MainTimeLeft.Text).TotalMilliseconds;
                    paused = false;
                }

                nTimer.Enabled = true;
            }
            else
            {
                paused = true;
                timerRunning = false;

                nTimer.Enabled = false;
                nTimer.Stop();
            }
        }

        private void BeginCountdown()
        {
            aTimer = new Timer();

            if (NumericalValidation(StartInTxtBox.Text)) { startTime = DateTime.Now.AddMinutes(int.Parse(StartInTxtBox.Text)); }
            else if (NumericalValidation(StartHrTxtBox.Text)) { startTime = GenerateStartTime(StartHrTxtBox.Text, StartMinTxtBox.Text); }

            aTimer.Elapsed += CountdownTimerEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private void CountdownTimerEvent(object sender, ElapsedEventArgs e)
        {
            var tSpan = TimeSpan.FromMilliseconds(timeToMeasure - (int)DateTime.Now.Subtract(startTime).TotalMilliseconds);
            Dispatcher.Invoke(new Action(() => AlertTxt.Text = tSpan.ToString(@"hh\:mm\:ss\.fff")));

            if (tSpan.TotalMilliseconds < 0)
            {
                foreach (var act in CountdownEnd()) { Dispatcher.Invoke(act); }
            }
        }

        private void IntervalTimerEvent(object sender, ElapsedEventArgs e)
        {
            if (timerRunning)
            {
                int eDate = timeToMeasure - (int)DateTime.Now.Subtract(startTime).TotalMilliseconds;
                var tSpan = TimeSpan.FromMilliseconds(eDate);

                foreach (var item in NormalIntervalActions(eDate)) { Dispatcher.Invoke(item); }
                if (tSpan.TotalMilliseconds < 0) { foreach (var item in SwitchIntervalTimer(intervalCountdown)) { Dispatcher.Invoke(item); } }
            }
        }

        private void TimerEvent(object sender, ElapsedEventArgs e)
        {
            if (timerRunning)
            {
                var eDate = timeToMeasure + e.SignalTime.Subtract(startTime).TotalMilliseconds;
                TimeSpan tSpan = TimeSpan.FromMilliseconds(eDate);

                Action evAction = new Action(() => MainTimeLeft.Text = tSpan.ToString(@"hh\:mm\:ss\.fff"));
                this.Dispatcher.Invoke(evAction);
            }
        }

        //action list methods
        private List<Action> CountdownEnd()
        {
            var actionList = new List<Action>();

            actionList.Add(new Action(() => AlertTxt.Visibility = Visibility.Hidden));
            actionList.Add(new Action(() => BeginTimer()));
            actionList.Add(new Action(() => aTimer.Enabled = false));
            actionList.Add(new Action(() => aTimer.Dispose()));

            return actionList;
        }

        private List<Action> NormalIntervalActions(int tS)
        {
            var actionList = new List<Action>();

            actionList.Add(new Action(() => MainTimeLeft.Text = TimeSpan.FromMilliseconds(tS).ToString(@"hh\:mm\:ss\.fff")));
            var cByte = Convert.ToByte(Math.Abs(tS) * 255/ timeToMeasure);

            actionList.Add(new Action(() => timeBoxBrush.Color = Color.Subtract(Brushes.Red.Color, Color.FromArgb(cByte, 0, 0, 0))));
            actionList.Add(new Action(() => MainEventsGrid.Background = timeBoxBrush));

            return actionList;
        }

        private List<Action> SwitchIntervalTimer(bool mode)
        {
            var actionList = new List<Action>();
            var mainSpan = TimeSpan.FromMilliseconds(mainTimeInMilliseconds);
            var interSpan = TimeSpan.FromMilliseconds(intervalTimeInMilliseconds);

            actionList.Add(new Action(() => SystemSounds.Exclamation.Play()));
            switch (mode)
            {
                case true:
                    actionList.Add(new Action(() => timeBoxes[0].Text = timeBoxes[1].Text));
                    actionList.Add(new Action(() => timeBoxes[1].Text = timeBoxes[2].Text));
                    actionList.Add(new Action(() => timeBoxes[2].Text = timeBoxes[0].Text));

                    actionList.Add(new Action(() => timeToMeasure = mainTimeInMilliseconds));
                    actionList.Add(new Action(() => intervalCountdown = false));
                    break;
                case false:
                    actionList.Add(new Action(() => reps++));

                    actionList.Add(new Action(() => timeBoxes[0].Text = timeBoxes[1].Text));
                    actionList.Add(new Action(() => timeBoxes[1].Text = mainSpan.ToString(@"hh\:mm\:ss\.fff")));
                    if (reps + 2 == (repsForLongBreak * currentLongSeries))
                    {
                        actionList.Add(new Action(() => timeBoxes[2].Text = TimeSpan.FromMilliseconds(longIntervalTime).ToString(@"hh\:mm\:ss\.fff")));
                        actionList.Add(new Action(() => currentLongSeries++));
                        actionList.Add(new Action(() => SrsText.Text = currentLongSeries.ToString()));
                    }
                    else
                    {
                        actionList.Add(new Action(() => timeBoxes[2].Text = interSpan.ToString(@"hh\:mm\:ss\.fff")));
                        actionList.Add(new Action(() => RpsText.Text = reps.ToString()));
                    }

                    actionList.Add(new Action(() => timeToMeasure = (int)TimeSpan.Parse(timeBoxes[0].Text).TotalMilliseconds));
                    actionList.Add(new Action(() => intervalCountdown = true));
                    break;
            }

            actionList.Add(new Action(() => startTime = DateTime.Now));

            return actionList;
        }

        private List<Action> ResetList()
        {
            var actionList = new List<Action>();

            actionList.Add(new Action(() => StartPauseBtn.Content = "Start"));
            actionList.Add(new Action(() => MainEventsGrid.Background = Brushes.White));
            actionList.Add(new Action(() => timeBoxes[0].Text = "00:00:00.000"));
            actionList.Add(new Action(() => timeBoxes[1].Text = "00:00:00.000"));
            actionList.Add(new Action(() => timeBoxes[2].Text = "00:00:00.000"));

            return actionList;
        }

        //button click handlers
        private void StartPauseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (StartInCheckBox.IsChecked == true || StartAtCheckBox.IsChecked == true) { BeginCountdown(); }
            else { BeginTimer(); }
            if (timerRunning) { StartPauseBtn.Content = "Pause"; }
            else { StartPauseBtn.Content = "Start"; }
        }
        private void ExitBtn_Click(object sender, RoutedEventArgs e) { this.Close(); }
        private void ResetButton_Click(object sender, RoutedEventArgs e) 
        { 
            timerRunning = false;
            timerConfigured = false;
            foreach (var item in ResetList()) { this.Dispatcher.Invoke(item); } 
        }

        //received focus handlers
        private void MainTxtBox_GotFocus(object sender, RoutedEventArgs e) { FocusHandler(MainTxtBox); }
        private void InterTxtBox_GotFocus(object sender, RoutedEventArgs e) { FocusHandler(InterTxtBox); }
        private void BreakTxtBox_GotFocus(object sender, RoutedEventArgs e) { FocusHandler(BreakTxtBox); }

        //lost focus handlers
        private void LostFocusHandler(TextBox txt)
        {
            if (!int.TryParse(txt.Text, out int secToConvert)) { txt.Text = string.Empty; }
            else { if (AlertTxt.Visibility == Visibility.Visible) { AlertTxt.Visibility = Visibility.Hidden; } SetTimeTranslationText(secToConvert); }
        }
        private void MainTxtBox_LostFocus(object sender, RoutedEventArgs e) { LostFocusHandler(MainTxtBox); }
        private void InterTxtBox_LostFocus(object sender, RoutedEventArgs e) { LostFocusHandler(InterTxtBox); }
        private void BreakTxtBox_LostFocus(object sender, RoutedEventArgs e) { LostFocusHandler(BreakTxtBox); }
        private void StartHrTxtBox_LostFocus(object sender, RoutedEventArgs e) { if (!NumericalValidation(StartHrTxtBox.Text) || int.Parse(StartHrTxtBox.Text) > 23) { StartHrTxtBox.Text = string.Empty; } }
        private void StartMinTxtBox_LostFocus(object sender, RoutedEventArgs e) { if (!NumericalValidation(StartHrTxtBox.Text) || int.Parse(StartHrTxtBox.Text) > 59) { StartHrTxtBox.Text = string.Empty; } }

        //technical
        private bool ConfigureTimer(int mode, Timer t) //0 - timer, 1 - interval, 2 - pomodoro, 3 - long interval
        {
            if (TimerBoxValidation(mode) > 0) { return false; }
            if (mode == 0)
            {
                currentLongSeries = 0;
                timeBoxes[1].Visibility = Visibility.Hidden;
                timeBoxes[2].Visibility = Visibility.Hidden;
                t.Elapsed += TimerEvent;
            }
            else
            {
                timeBoxes[1].Visibility = Visibility.Visible;
                timeBoxes[2].Visibility = Visibility.Visible;

                mainTimeInMilliseconds = ConvToInt(MainTxtBox.Text) * 1000;
                intervalTimeInMilliseconds = ConvToInt(InterTxtBox.Text) * 1000;

                timeBoxes[0].Text = TimeSpan.FromMilliseconds(mainTimeInMilliseconds).ToString(@"hh\:mm\:ss\.fff");
                timeBoxes[1].Text = TimeSpan.FromMilliseconds(intervalTimeInMilliseconds).ToString(@"hh\:mm\:ss\.fff");
                timeBoxes[2].Text = TimeSpan.FromMilliseconds(mainTimeInMilliseconds).ToString(@"hh\:mm\:ss\.fff");

                if (mode == 3)
                {
                    longIntervalTime = ConvToInt(BreakTxtBox.Text) * 1000;
                    repsForLongBreak = int.Parse(RepTxtBox.Text);
                    if (reps + 1 == repsForLongBreak) { timeBoxes[2].Text = TimeSpan.FromMilliseconds(longIntervalTime).ToString(@"hh\:mm\:ss\.fff"); }
                }

                intervalCountdown = false;
                timeToMeasure = mainTimeInMilliseconds;

                t.Elapsed += IntervalTimerEvent;
            }

            if (mode == 3)
            {
                currentLongSeries = 1;
                SrsHeader.Visibility = Visibility.Visible;
                SrsText.Visibility = Visibility.Visible;
                SrsText.Text = currentLongSeries.ToString();
            }
            else
            {
                currentLongSeries = 0;
                SrsHeader.Visibility = Visibility.Hidden;
                SrsText.Visibility = Visibility.Hidden;
            }

            t.AutoReset = true;
            timerConfigured = true;
            AlertTxt.Visibility = Visibility.Hidden;

            return true;
        }
        private void LoadInitialSettings()
        {
            timerConfigured = false;
            timeBoxes = new TextBox[] { MainTimeLeft, UpcomingTime1, UpcomingTime2 };

            RpsText.Text = reps.ToString();
            SrsText.Text = currentLongSeries.ToString();

            ModeCmb.SelectedIndex = Properties.Settings.Default.LastChosenMode;
            MainTxtBox.Text = Properties.Settings.Default.LastChosenMainTime.ToString();
            InterTxtBox.Text = Properties.Settings.Default.LastChosenInterTime.ToString();

            timeBoxBrush = new SolidColorBrush();
            snd = new SoundPlayerAction();
        }
        private void UpdateLastChosenProperties()
        {
            Properties.Settings.Default.LastChosenMainTime = MainTxtBox.Text;
            Properties.Settings.Default.LastChosenInterTime = InterTxtBox.Text;
            Properties.Settings.Default.LastChosenMode = ModeCmb.SelectedIndex;
            Properties.Settings.Default.Save();
        }
        private int ConvToInt(string inputString) { return int.Parse(inputString); }
        private bool NumericalValidation(string inputStr) { if (int.TryParse(inputStr, out int a)) { return true; } else { return false; } }
        private int TimerBoxValidation(int mode)
        {
            int e = 0;
            List<TextBox> stringsToCheck = new List<TextBox>();

            if (mode > 0)
            {
                stringsToCheck.Add(MainTxtBox);
                stringsToCheck.Add(InterTxtBox);
                if (mode == 3)
                {
                    stringsToCheck.Add(BreakTxtBox);
                    stringsToCheck.Add(RepTxtBox);
                }
            }
            if (StartInCheckBox.IsChecked == true) { stringsToCheck.Add(StartInTxtBox); }
            if (StartAtCheckBox.IsChecked == true) { stringsToCheck.Add(StartHrTxtBox); }

            foreach (var item in stringsToCheck) { if (!NumericalValidation(item.Text)) { e++; item.Background = Brushes.PaleVioletRed; } }

            return e;
        }
        private void FocusHandler(TextBox txt)
        {
            txt.SelectAll();
            if (int.TryParse(txt.Text, out int secToConvert)) { SetTimeTranslationText(secToConvert); }
        }

        private DateTime GenerateStartTime(string hString, string mString) { return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, int.Parse(hString), int.Parse(mString), 0); }
    }
}
