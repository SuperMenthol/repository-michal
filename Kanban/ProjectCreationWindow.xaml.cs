using System.Windows;

namespace Kanban
{
    public partial class ProjectCreationWindow : Window
    {
        TechnicalMethodClass TechMeth;
        public ProjectCreationWindow()
        {
            InitializeComponent();
            TechMeth = new TechnicalMethodClass();
        }

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            var dbAccess = new SQLDatabaseAccess();
            if (Properties.Settings.Default.loggedRole == 1 || Properties.Settings.Default.loggedRole == 4) //zmienić, ma nie być hardcode
            {
                dbAccess.CreateProject(ProjTextBox.Text, ProjDescBox.Text); //sanitize projdesc
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) { this.Close(); }

        private void AutoChkBox_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
