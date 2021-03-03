using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Kanban
{
    public class SQLDatabaseAccess
    {
        private static string DatabaseConnectionString() { return ConfigurationManager.ConnectionStrings["TechnicalDatabaseString"].ConnectionString; }
        private static string ModelConnectionString() { return ConfigurationManager.ConnectionStrings["ModelDatabaseString"].ConnectionString; }

        public DataTable LogonValidation(string username, string password)
        {
            DataTable dt = new DataTable();

            using (SqlConnection nConnection = new SqlConnection(DatabaseConnectionString()))
            {
                var loginCommand = new SqlCommand();
                loginCommand.Connection = nConnection;

                loginCommand.CommandText = "SELECT * FROM usertable WHERE [user_name]=@uname AND [password]=@pword";
                loginCommand.Parameters.Add("@uname", System.Data.SqlDbType.VarChar).Value = username;
                loginCommand.Parameters.Add("@pword", System.Data.SqlDbType.VarChar).Value = password;

                nConnection.Open();
                SqlDataReader usReader = loginCommand.ExecuteReader();

                SqlDataAdapter dataAdapter = new SqlDataAdapter();
                dataAdapter.SelectCommand = loginCommand;
                usReader.Close();
                dataAdapter.Fill(dt);

                return dt;
            }
        }

        public DataTable GetUserRoles()
        {
            DataTable dt = new DataTable();

            var selCmd = new SqlCommand();
            selCmd.CommandText = "SELECT [role_id],[role_title] FROM userroles";

            using (SqlConnection nConnection = new SqlConnection(DatabaseConnectionString()))
            {
                selCmd.Connection = nConnection;
                nConnection.Open();
                var nAdapter = new SqlDataAdapter();
                nAdapter.SelectCommand = selCmd;
                nAdapter.Fill(dt);
            }

            return dt;
        }

        public void UpdateUser(string username, string password)
        {
            var upCmd = new SqlCommand();
            upCmd.CommandText = "UPDATE usertable SET [user_name] = @uname, [password] = @upword WHERE [user_id]=@uid";
            upCmd.Parameters.Add("@uname", SqlDbType.VarChar).Value = username;
            upCmd.Parameters.Add("@upword", SqlDbType.VarChar).Value = password;
            upCmd.Parameters.Add("@uid", SqlDbType.Int).Value = Properties.Settings.Default.loggedUserId;

            using (SqlConnection nConnection = new SqlConnection(DatabaseConnectionString()))
            {
                upCmd.Connection = nConnection;
                try
                {
                    nConnection.Open();
                    upCmd.ExecuteNonQuery();
                    MessageBox.Show("User updated", "Success", MessageBoxButton.OK);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Failed", MessageBoxButton.OK);
                }
            }
        }

        public void UpdateUserRole(string username, int role)
        {
            var upCmd = new SqlCommand();
            upCmd.CommandText = "UPDATE usertable SET [user_role] = @urole WHERE [user_name]=@uname";
            upCmd.Parameters.Add("@urole", SqlDbType.Int).Value = role;
            upCmd.Parameters.Add("@uname", SqlDbType.VarChar).Value = username;

            using (SqlConnection nConnection = new SqlConnection(DatabaseConnectionString()))
            {
                upCmd.Connection = nConnection;

                try
                {
                    nConnection.Open();
                    upCmd.ExecuteNonQuery();
                    MessageBox.Show("User updated", "Success", MessageBoxButton.OK);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Failed", MessageBoxButton.OK);
                }
            }
        }
        public void CreateProject(string projNameString, string projDesc, string dbN = "")
        {
            using (SqlConnection nConnection = new SqlConnection(DatabaseConnectionString()))
            {
                string dPath = AppDomain.CurrentDomain.BaseDirectory;
                string dbName = GenerateDatabaseName(projNameString);

                var crtCommand = new SqlCommand();
                crtCommand.Connection = nConnection;
                crtCommand.CommandText = "CREATE DATABASE " + dbName + ";";

                try
                {
                    nConnection.Open();
                    crtCommand.ExecuteNonQuery();
                    MessageBox.Show("Database is in place", "Success", MessageBoxButton.OK);
                    AddProjectToProjectTable(projNameString, projDesc, dbName);
                    SQLDataAccess nProj = new SQLDataAccess(dbName);
                    nProj.AddCreatorToProject();
                }
                catch (System.Exception ex) { MessageBox.Show(ex.ToString(), "Failed", MessageBoxButton.OK); }
                finally { if (nConnection.State == ConnectionState.Open) { nConnection.Close(); } }
            }
        }

        public void DropProject(string projName)
        {
            using (SqlConnection nConnection = new SqlConnection(DatabaseConnectionString()))
            {
                var nCmd = new SqlCommand();

                nCmd.CommandText = "DROP DATABASE " + GetProjectsList().AsEnumerable().Where(x => (string)x[1] == projName).FirstOrDefault().Field<string>(4);
                nCmd.Connection = nConnection;
                try
                {
                    nConnection.Open();
                    nCmd.ExecuteNonQuery();
                    MessageBox.Show("Database dropped", "Success", MessageBoxButton.OK);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Failed", MessageBoxButton.OK);
                }

                nCmd.CommandText = "DELETE FROM projecttable WHERE project_name = @pname";
                nCmd.Parameters.Add("@pname", SqlDbType.VarChar).Value = projName;

                try
                {
                    nConnection.Open();
                    nCmd.ExecuteNonQuery();
                    MessageBox.Show("Record deleted from project table", "Success", MessageBoxButton.OK);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Failed", MessageBoxButton.OK);
                }
            }
        }

        private void AddProjectToProjectTable(string projName, string pDesc, string dbNameString)
        {
            using (SqlConnection nConnection = new SqlConnection(DatabaseConnectionString()))
            {
                var insCommand = new SqlCommand();
                insCommand.Connection = nConnection;
                insCommand.CommandText = "INSERT INTO projecttable (project_name, project_description, project_crt_tms, proj_db_name) VALUES (@pname, @pdesc, @ptms, @pstring)";
                insCommand.Parameters.Add("@pname", SqlDbType.VarChar).Value = projName;
                insCommand.Parameters.Add("@pdesc", SqlDbType.Text).Value = pDesc;
                insCommand.Parameters.Add("@ptms", SqlDbType.DateTime).Value = DateTime.Now;
                insCommand.Parameters.Add("@pstring", SqlDbType.VarChar).Value = dbNameString;

                try
                {
                    nConnection.Open();
                    insCommand.ExecuteNonQuery();
                }
                catch (System.Exception ex) { MessageBox.Show(ex.ToString(), "Failed", MessageBoxButton.OK); }
                finally { if (nConnection.State == ConnectionState.Open) { nConnection.Close(); } }
            }
        }

        private string GenerateDatabaseName(string projName)
        {
            projName = projName.ToUpper();
            projName = projName.Trim();
            projName = projName.Replace(" ", "_");
            projName = projName.Replace(" ", "_");
            if (projName.StartsWith("ii")) { projName = projName.Remove(0, 2); }

            return projName;
        }

        public DataTable GetProjectsList()
        {
            var dt = new DataTable();

            using (SqlConnection nConnection = new SqlConnection(DatabaseConnectionString()))
            {
                var projSelectString = "SELECT * FROM projecttable";
                var nCommand = new SqlCommand(projSelectString, nConnection);

                nConnection.Open();
                var nAdapter = new SqlDataAdapter();
                nAdapter.SelectCommand = nCommand;
                nAdapter.Fill(dt);
            }

            return dt;
        }

        public DataTable GetProjectUserLogins(List<string> userIdList)
        {
            DataTable dt = new DataTable();

            string paramString = "(";
            foreach (var item in userIdList) { paramString = paramString + "'" + item + "',"; }
            paramString = paramString.Remove(paramString.Length-1, 1);
            paramString = paramString + ")";

            using (SqlConnection nConnection = new SqlConnection(DatabaseConnectionString()))
            {
                var usListSelString = "SELECT user_id, user_name FROM usertable WHERE user_id IN " + paramString;
                var nCommand = new SqlCommand(usListSelString, nConnection);

                MessageBox.Show(usListSelString, "Query string", MessageBoxButton.OK);

                nConnection.Open();
                SqlDataAdapter nAdapter = new SqlDataAdapter();
                nAdapter.SelectCommand = nCommand;
                nAdapter.Fill(dt);
            }

            return dt;
        }

        public DataTable GetUser(string userName)
        {
            using (SqlConnection nConnection = new SqlConnection(DatabaseConnectionString()))
            {
                var nCommand = new SqlCommand();
                nCommand.Connection = nConnection;
                nCommand.CommandText = "SELECT [user_id],[user_name],[user_role] FROM usertable WHERE user_name = @uname";
                nCommand.Parameters.Add("@uname", SqlDbType.VarChar).Value = userName;

                var nAdapter = new SqlDataAdapter();
                var nTable = new DataTable();

                nAdapter.SelectCommand = nCommand;
                nConnection.Open();
                nAdapter.Fill(nTable);

                return nTable;
            }
        }

        public DataTable GetUser(int userId)
        {
            using (SqlConnection nConnection = new SqlConnection(DatabaseConnectionString()))
            {
                var nCommand = new SqlCommand();
                nCommand.Connection = nConnection;
                nCommand.CommandText = "SELECT [user_id],[user_name],[user_role] FROM usertable WHERE user_id = @uid";
                nCommand.Parameters.Add("@uid", SqlDbType.Int).Value = userId;

                var nAdapter = new SqlDataAdapter();
                var nTable = new DataTable();

                nAdapter.SelectCommand = nCommand;
                nConnection.Open();
                nAdapter.Fill(nTable);

                return nTable;
            }
        }

        public DataTable GetAllUserLogins()
        {
            DataTable dt = new DataTable();

            using (SqlConnection nConnection = new SqlConnection(DatabaseConnectionString()))
            {
                var usCommand = new SqlCommand();
                usCommand.Connection = nConnection;
                usCommand.CommandText = "SELECT [user_id], [user_name] FROM usertable";
                var nAdapter = new SqlDataAdapter();
                nAdapter.SelectCommand = usCommand;
                nAdapter.Fill(dt);
            }

            return dt;
        }

        public DataTable GetCredentials(int userId)
        {
            DataTable dt = new DataTable();

            SqlCommand selCmd = new SqlCommand { CommandText = "SELECT [user_name], [password] FROM usertable WHERE [user_id] = @uid" };
            selCmd.Parameters.Add("@uid", SqlDbType.Int).Value = userId;

            using (SqlConnection nConnection = new SqlConnection(DatabaseConnectionString()))
            {
                nConnection.Open();
                selCmd.Connection = nConnection;
                var nAdapter = new SqlDataAdapter();
                nAdapter.SelectCommand = selCmd;
                nAdapter.Fill(dt);
            }

            return dt;
        }

        public void InsertGlobalParameter(string tableName, List<KeyValuePair<string,string>> paramKvp)
        {
            var insCommand = new SqlCommand();
            var queryString = "INSERT INTO " + tableName + "(" + GetColumnName(tableName, 0) + "," + GetColumnName(tableName, 1) + "," + GetColumnName(tableName, 2) + ") VALUES (" +
                GetNextId(tableName) + ", " + paramKvp[0].Key + ", " + paramKvp[1].Key + ")";

            using (SqlConnection nConnection = new SqlConnection(ModelConnectionString()))
            {
                insCommand.Connection = nConnection;
                insCommand.CommandText = queryString;
                foreach (var item in paramKvp) { insCommand.Parameters.Add(item.Key, SqlDbType.VarChar).Value = item.Value; }

                try
                {
                    nConnection.Open();
                    insCommand.ExecuteNonQuery();
                    MessageBox.Show("Global parameter added", "Success", MessageBoxButton.OK);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Failed", MessageBoxButton.OK);
                }
            }
        }

        private string GetColumnName(string tableName, int columnId)
        {
            var sCommand = new SqlCommand();
            var dt = new DataTable();

            using (SqlConnection nConnection = new SqlConnection(ModelConnectionString()))
            {
                sCommand.Connection = nConnection;
                sCommand.CommandText = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + tableName + "' AND ORDINAL_POSITION = " + (columnId + 1).ToString();
                var nAdapter = new SqlDataAdapter();
                nAdapter.SelectCommand = sCommand;
                nAdapter.Fill(dt);
                return dt.Rows[0].Field<string>(0);
            }
        }

        private int GetNextId(string tableName)
        {
            var sCommand = new SqlCommand();
            var dt = new DataTable();

            using (SqlConnection nConnection = new SqlConnection(ModelConnectionString()))
            {
                sCommand.Connection = nConnection;
                sCommand.CommandText = "SELECT " + GetColumnName(tableName, 0) + " FROM " + tableName + " ORDER BY 1 ASC";
                nConnection.Open();

                var nAdapter = new SqlDataAdapter();
                nAdapter.SelectCommand = sCommand;
                nAdapter.Fill(dt);
            }

            var nList = dt.AsEnumerable().Select(x => (int)x[0]).ToList();
            if (nList.Count > 1)
            {
                for (int i = 1; i < nList.Count; i++) { if (nList[i] - nList[i - 1] > 1) { return nList[i - 1] + 1; } }
            }

            return nList[0] + 1;
        }

        public void CreateNewUser(string login, string password, int sysrole)
        {
            var insCmd = new SqlCommand();
            insCmd.CommandText = "INSERT INTO usertable VALUES (@login, @pword, @role)";
            insCmd.Parameters.Add("@login", SqlDbType.VarChar).Value = login;
            insCmd.Parameters.Add("@pword", SqlDbType.VarChar).Value = password;
            insCmd.Parameters.Add("@role", SqlDbType.Int).Value = sysrole;

            using (SqlConnection nConnection = new SqlConnection(DatabaseConnectionString()))
            {
                insCmd.Connection = nConnection;
                try
                {
                    nConnection.Open();
                    insCmd.ExecuteNonQuery();
                    MessageBox.Show("User added", "Success", MessageBoxButton.OK);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Failed", MessageBoxButton.OK);
                }
            }
        }
    }

    public partial class MainWindow : Window
    {
        DataTable projDataTable;
        SQLDatabaseAccess dbAccess;
        TechnicalMethodClass TechMeth;
        public MainWindow()
        {
            InitializeComponent();
            dbAccess = new SQLDatabaseAccess();
            TechMeth = new TechnicalMethodClass();
            InitializeProjectComboBox();
        }

        private void InitializeProjectComboBox()
        {
            projDataTable = dbAccess.GetProjectsList();
            var projectList = projDataTable.AsEnumerable().Select(x => x[1].ToString()).ToList();
            foreach (var item in projectList) { if (!ProjCmbBox.Items.Contains(item)) { ProjCmbBox.Items.Add(item); } }
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e) { Environment.Exit(0); }
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            bool authorized = false;
            string dbN = string.Empty;

            if (LoginTxtBox.Text != string.Empty && PasswordTxtBox.Text != string.Empty)
            {
                if (TechMeth.SanitizeLogonFields(new List<string>() { LoginTxtBox.Text, PasswordTxtBox.Text }))
                {
                    ManageAlertTextBlock(99);

                    var logRow = dbAccess.LogonValidation(LoginTxtBox.Text, PasswordTxtBox.Text);
                    if (logRow.Rows.Count > 0)
                    {
                        if (logRow.Rows[0].Field<int>(0) < 1) { ManageAlertTextBlock(1); }
                        else if (ProjCmbBox.SelectedIndex > 0)
                        {
                            DataRow dR = projDataTable.Select().Where(x => x.Field<string>("project_name") == ProjCmbBox.SelectedItem.ToString()).FirstOrDefault();
                            dbN = dR.Field<string>(4);
                            var projAccess = new SQLDataAccess(dbN);
                            authorized = projAccess.ProjectLogonAuthorization(logRow.Rows[0]);
                        }

                        if (logRow.Rows[0].Field<int>(3) == 1 || logRow.Rows[0].Field<int>(3) == 4) { authorized = true; } //usunąć, ma nie być hardcode
                        if (!authorized) { ManageAlertTextBlock(2); }
                        else
                        {
                            Properties.Settings.Default.loggedUserId = logRow.Rows[0].Field<int>(0);
                            Properties.Settings.Default.loggedRole = logRow.Rows[0].Field<int>(3);

                            if (ProjCmbBox.SelectedIndex == 0) { ProjectCreationWindow pcw = new ProjectCreationWindow(); pcw.Show(); }
                            else { ProjectMainWindow pmw = new ProjectMainWindow(dbN); pmw.Show(); }
                        }
                    }
                    
                }
                else { ManageAlertTextBlock(0); }
            }
        }

        private void ManageAlertTextBlock (int mode)
        {
            TextBlock objToManage = AlertTxt;

            objToManage.Visibility = mode == 99 ? Visibility.Hidden : Visibility.Visible;
            switch (mode)
            {
                case 0:
                    objToManage.Text = "The credentials contain illegal characters"; break;
                case 1:
                    objToManage.Text = "These credentials are invalid"; break;
                case 2:
                    objToManage.Text = "You are not authorized to access this option"; break;
            }
        }
    }

    public sealed class TechnicalMethodClass
    {
        List<string> unsafeStrings = new List<string>() { "=", "!", ";" };
        public bool SanitizeLogonFields(List<string> stringsToCheck)
        {
            foreach (string st in unsafeStrings)
            {
                foreach (var item in stringsToCheck) { if (item.Contains(st)) { return false;} }
            }

            return true;
        }
    }
}
