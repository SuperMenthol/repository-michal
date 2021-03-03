using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Kanban
{
    public class SQLDataAccess
    {
        string connectionString;
        public SQLDataAccess(string dbName) { connectionString = "Data Source=.\\KANBANPROD; Initial Catalog=" + dbName + "; Trusted_Connection=True"; }

        public void AddCreatorToProject()
        {
            using (SqlConnection nConnection = new SqlConnection(connectionString))
            {
                var queryString = "INSERT INTO p_project_users (user_id,user_role,user_project_role) VALUES (@pid, @psysrole, @pprojrole)";
                SqlCommand userAddCommand = new SqlCommand(queryString, nConnection);
                userAddCommand.Parameters.Add("@pid", SqlDbType.Int).Value = Properties.Settings.Default.loggedUserId;
                userAddCommand.Parameters.Add("@psysrole", SqlDbType.Int).Value = Properties.Settings.Default.loggedRole;
                userAddCommand.Parameters.Add("@pprojrole", SqlDbType.Int).Value = 1;

                nConnection.Open();
                userAddCommand.ExecuteNonQuery();
            }
        }

        public void AddUserToProject(List<KeyValuePair<string, object>> paramKvp)
        {
            var insCmd = new SqlCommand();
            var queryString = "INSERT INTO p_project_users VALUES (";
            foreach (var item in paramKvp) { queryString = queryString + item.Key + ","; }
            queryString = queryString.Remove(queryString.Length - 1, 1) + ")";

            using (SqlConnection nConnection = new SqlConnection(connectionString))
            {
                insCmd.Connection = nConnection;
                insCmd.CommandText = queryString;
                foreach (var item in paramKvp) { insCmd.Parameters.AddWithValue(item.Key, item.Value); }

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

        public void RemoveUserFromProject(int userId)
        {
            var remCmd = new SqlCommand();
            var queryString = "DELETE FROM p_project_users WHERE user_id = @uid";
            remCmd.Parameters.Add("@uid", SqlDbType.Int).Value = userId;

            using (SqlConnection nConnection = new SqlConnection(connectionString))
            {
                remCmd.CommandText = queryString;
                remCmd.Connection = nConnection;

                try
                {
                    nConnection.Open();
                    remCmd.ExecuteNonQuery();
                    MessageBox.Show("User deleted", "Deleting user", MessageBoxButton.OK);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Failed", MessageBoxButton.OK);
                }
            }
        }

        public void UpdateProjectUser(List<int> paramList)
        {
            var upCmd = new SqlCommand();
            var queryString = "UPDATE p_project_users SET user_project_role = @urole WHERE user_id = @uid";
            upCmd.Parameters.Add("@uid", SqlDbType.Int).Value = paramList[0];
            upCmd.Parameters.Add("@urole", SqlDbType.Int).Value = paramList[1];

            using (SqlConnection nConnection = new SqlConnection(connectionString))
            {
                upCmd.Connection = nConnection;
                upCmd.CommandText = queryString;

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

        public bool ProjectLogonAuthorization(DataRow user)
        {
            var authCommand = new SqlCommand();

            using (SqlConnection nConnection = new SqlConnection(connectionString))
            {
                authCommand = new SqlCommand();
                authCommand.Connection = nConnection;
                authCommand.CommandText = "SELECT * FROM p_project_users WHERE user_id = @uid";
                authCommand.Parameters.Add("@uid", SqlDbType.Int).Value = user[0];

                nConnection.Open();
                var nReader = authCommand.ExecuteReader();
                if (!nReader.HasRows) { return false; } else { return true; }
            }
        }

        public DataSet GetConfigurationTables()
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            using (SqlConnection nConnection = new SqlConnection(connectionString))
            {
                nConnection.Open();
                var nAdapter = new SqlDataAdapter();
                List<string> tbNamesList = new List<string>();

                dt = new DataTable();
                dt.TableName = "Tables";
                var tbCommand = new SqlCommand();
                tbCommand.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES";
                tbCommand.Connection = nConnection;
                nAdapter.SelectCommand = tbCommand;
                nAdapter.Fill(dt);
                ds.Tables.Add(dt);
                foreach (var item in ds.Tables["Tables"].AsEnumerable()) { tbNamesList.Add(item.Field<string>(0)); }

                var selCommand = new SqlCommand();

                foreach (var item in tbNamesList)
                {
                    dt = new DataTable();
                    dt.TableName = item;

                    selCommand.CommandText = "SELECT * FROM " + item;
                    selCommand.Connection = nConnection;

                    nAdapter.SelectCommand = selCommand;
                    nAdapter.Fill(dt);
                    ds.Tables.Add(dt);
                }
            }

            return ds;
        }

        public void AddTaskToTable(List<KeyValuePair<string, object>> paramKvp)
        {
            var cmd = new SqlCommand();

            string queryString = "INSERT INTO p_project_tasks (";
            foreach (var item in paramKvp) { queryString = queryString + item.Key.Remove(0, 1) + ","; }
            queryString = queryString + "task_crt_tms) VALUES (";

            foreach (var item in paramKvp) { queryString = queryString + item.Key + ","; cmd.Parameters.AddWithValue(item.Key, item.Value); }
            queryString = queryString + "@crt)";
            cmd.Parameters.Add("@crt", SqlDbType.DateTime).Value = DateTime.Now;

            using (SqlConnection nConnection = new SqlConnection(connectionString))
            {
                cmd.Connection = nConnection;
                cmd.CommandText = queryString;
                MessageBox.Show(queryString, "Query to check", MessageBoxButton.OK);

                try 
                {
                    nConnection.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Insert failed", MessageBoxButton.OK); }
            }
        }

        public int GetTaskId(string taskName)
        {
            using (SqlConnection nConnection = new SqlConnection(connectionString))
            {
                var nCommand = new SqlCommand();
                nCommand.Connection = nConnection;
                nCommand.CommandText = "SELECT task_id FROM p_project_tasks WHERE task_title = @tname";
                nCommand.Parameters.Add("@tname", SqlDbType.VarChar).Value = taskName;

                var nAdapter = new SqlDataAdapter();
                var nTable = new DataTable();

                nAdapter.SelectCommand = nCommand;
                nConnection.Open();
                nAdapter.Fill(nTable);

                if (nTable.Rows.Count > -1) { return nTable.Rows[0].Field<int>(0); }
                else return 0;
            }
        }

        public void UpdateTask(List<KeyValuePair<string,object>> paramKvp)
        {
            var cmd = new SqlCommand();
            int limiter = paramKvp.Count - 1;

            string queryString = "UPDATE p_project_tasks SET ";
            for (int i = 0; i < limiter; i++)
            { queryString = queryString + paramKvp[i].Key.Remove(0, 1) + " = " + paramKvp[i].Key + ", "; }
            queryString = queryString + "task_upd_tms = @upd WHERE task_id = " + paramKvp[limiter].Key;

            for (int i = 0; i < paramKvp.Count; i++) { cmd.Parameters.AddWithValue(paramKvp[i].Key, paramKvp[i].Value); }
            cmd.Parameters.Add("@upd", SqlDbType.DateTime).Value = DateTime.Now;

            using (SqlConnection nConnection = new SqlConnection(connectionString))
            {
                cmd.Connection = nConnection;
                cmd.CommandText = queryString;
                MessageBox.Show(queryString, "Query to check", MessageBoxButton.OK);

                try
                {
                    nConnection.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Insert failed", MessageBoxButton.OK); }
            }
        }

        public void UpdateParameter(string tableName, int idInTable, List<KeyValuePair<string, object>> paramKvp)
        {
            var upCmd = new SqlCommand();

            string queryString = "UPDATE " + tableName + " SET ";
            for (int i = 0; i < paramKvp.Count; i++) { queryString = queryString + GetColumnName(tableName, i+1) + " = " + paramKvp[i].Key + ", "; }
            queryString = queryString.Remove(queryString.Length-2,1) + "WHERE " + GetColumnName(tableName, 0) + " = " + idInTable.ToString();

            for (int i = 0; i < paramKvp.Count; i++) { upCmd.Parameters.AddWithValue(paramKvp[i].Key, paramKvp[i].Value); }

            using (SqlConnection nConnection = new SqlConnection(connectionString))
            {
                upCmd.Connection = nConnection;
                upCmd.CommandText = queryString;
                MessageBox.Show(queryString, "Update statement", MessageBoxButton.OK);

                try
                {
                    nConnection.Open();
                    upCmd.ExecuteNonQuery();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Update failed", MessageBoxButton.OK); }
            }
        }

        public void InsertParameter(string tableName, List<KeyValuePair<string, object>> paramKvp)
        {
            var insCmd = new SqlCommand();

            string queryString = "INSERT INTO " + tableName + " (";
            for (int i = 0; i < 3; i++) { queryString = queryString + GetColumnName(tableName, i) + ","; }
            queryString = queryString.Remove(queryString.Length - 1, 1) + ") VALUES (@id,";
            for (int i = 0; i < 2; i++) { queryString = queryString + paramKvp[i].Key + ","; }
            queryString = queryString.Remove(queryString.Length - 1, 1) + ")";

            using (SqlConnection nConnection = new SqlConnection(connectionString))
            {
                insCmd.Connection = nConnection;
                insCmd.CommandText = queryString;
                insCmd.Parameters.AddWithValue("@id", GetNextId(tableName));
                foreach (var item in paramKvp) { insCmd.Parameters.AddWithValue(item.Key, item.Value); }

                MessageBox.Show(queryString, "Query", MessageBoxButton.OK);
                try
                {
                    nConnection.Open();
                    insCmd.ExecuteNonQuery();
                    MessageBox.Show("Parameter inserted", "Sucess", MessageBoxButton.OK);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Insert failed", MessageBoxButton.OK);
                }
            }
        }

        public void RemoveParameter(string tableName, int removedParameterId)
        {
            var remCmd = new SqlCommand();

            string queryString = "DELETE FROM " + tableName + " WHERE " + GetColumnName(tableName, 0) + " = @id";
            using (SqlConnection nConnection = new SqlConnection(connectionString))
            {
                remCmd.CommandText = queryString;
                remCmd.Connection = nConnection;
                remCmd.Parameters.Add("@id", SqlDbType.Int).Value = removedParameterId;

                try
                {
                    nConnection.Open();
                    remCmd.ExecuteNonQuery();
                    MessageBox.Show("Parameter deleted", "Success", MessageBoxButton.OK);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Failed", MessageBoxButton.OK);
                }
            }
        }

        private string GetColumnName(string tableName, int columnId)
        {
            var sCommand = new SqlCommand();
            var dt = new DataTable();

            using (SqlConnection nConnection = new SqlConnection(connectionString))
            {
                sCommand.Connection = nConnection;
                sCommand.CommandText = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + tableName + "' AND ORDINAL_POSITION = " + (columnId+1).ToString();
                var nAdapter = new SqlDataAdapter();
                nAdapter.SelectCommand = sCommand;
                nAdapter.Fill(dt);
                return dt.Rows[0].Field<string>(0);
            }
        }

        private int GetNextId (string tableName)
        {
            var sCommand = new SqlCommand();
            var dt = new DataTable();

            using (SqlConnection nConnection = new SqlConnection(connectionString))
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
    }

    public partial class ProjectMainWindow : Window
    {
        string dbName;
        SQLDataAccess sqDataAccess;
        DataSet projectConfigurationDataSet;
        List<StackPanel> statusPanelList;
        List<object> dependantObjectsList = new List<object>();

        public SQLDataAccess SqDataAccess { get { return sqDataAccess; } }
        public DataSet ProjectConfigurationDataSet { get { return projectConfigurationDataSet; } }

        public ProjectMainWindow(string database)
        {
            InitializeComponent();
            dbName = database;
            sqDataAccess = new SQLDataAccess(dbName);

            InitializeTaskDataset();
            InitializeTaskStatusGrids();
            LoadTasks();
        }

        public void LoadTasks()
        {
            foreach (var item in statusPanelList)
            {
                TextBlock sHeader = (TextBlock)item.Children[0];
                string headerString = sHeader.Text;

                item.Children.Clear();
                item.Children.Add(sHeader);

                var a = projectConfigurationDataSet.Tables["p_project_tasks"].AsEnumerable().Where(x => x["task_status"].Equals(GetItemId("c_taskstatus", headerString)));
                switch (FilterOptionCbx.SelectedIndex)
                {
                    case 0: break;
                    case 1:
                        TextBox tBox = (TextBox)dependantObjectsList[0];
                        if (int.TryParse(tBox.Text, out int id)) { a = a.Where(x => (int)x[0] == id); }
                        else { AlertTxt.Visibility = Visibility.Visible; }
                        break;
                    case 2:
                        tBox = (TextBox)dependantObjectsList[0];
                        a = a.Where(x => x.Field<string>(1).Contains(tBox.Text));
                        break;
                    case 3:
                        ComboBox cbx = (ComboBox)dependantObjectsList[0];
                        int typeId = GetItemId("c_tasktypes", (string)cbx.SelectedItem);
                        a = a.Where(x => (int)x["task_type"] == typeId);
                        break;
                    case 4:
                        cbx = (ComboBox)dependantObjectsList[0];
                        int prioId = GetItemId("c_taskpriorities", (string)cbx.SelectedItem);
                        a = a.Where(x => (int)x["task_priority"] == prioId);
                        break;
                    case 5:
                        cbx = (ComboBox)dependantObjectsList[0];
                        int compId = GetItemId("c_projectcomponents", (string)cbx.SelectedItem);
                        a = a.Where(x => (int)x["project_component"] == compId);
                        break;
                    case 6:
                        cbx = (ComboBox)dependantObjectsList[0];
                        int userId = new SQLDatabaseAccess().GetUser((string)cbx.SelectedItem).AsEnumerable().FirstOrDefault().Field<int>(0);
                        a = a.Where(x => (int)x["project_component"] == userId);
                        break;
                }
                foreach (var drow in a)
                {
                    Button nTask = new Button();
                    item.Children.Add(nTask);
                    nTask.HorizontalAlignment = HorizontalAlignment.Center;
                    nTask.Height = 30;
                    nTask.Width = 100;
                    nTask.Background = Brushes.White;
                    nTask.Click += OpenTask_Click;

                    TextBlock taskDesc = new TextBlock { FontSize = 8, TextAlignment = TextAlignment.Left };
                    taskDesc.Text = GenerateTaskName(drow);
                    nTask.Content = taskDesc;
                }
            }
        }

        private string GenerateTaskName(DataRow dRow)
        {
            string taskHeader = GetItemFromId("c_tasktypes", dRow.Field<int>(3)).Substring(0, 1)
                + "-" + dRow.Field<int>(0).ToString() + Environment.NewLine +
                dRow.Field<string>(1);
            //MessageBox.Show(taskHeader, "Task header", MessageBoxButton.OK);
            return taskHeader;
        }

        private void InitializeTaskStatusGrids()
        {
            statusPanelList = new List<StackPanel>();

            DataTable statusTb = projectConfigurationDataSet.Tables["c_taskstatus"];
            var statusList = statusTb.AsEnumerable().ToList();
            var mainGridWidth = this.Width - (MainGrid.Margin.Left + MainGrid.Margin.Right);

            var childGridWidth = mainGridWidth / statusList.Count;
            //MessageBox.Show(mainGridWidth.ToString(), "width", MessageBoxButton.OK);

            for (int i = 0; i < statusList.Count; i++)
            {
                Thickness margins = new Thickness(childGridWidth * i, 20, childGridWidth * (statusList.Count - (i + 1)), 30);
                CreateStatusGrid(statusList[i].Field<string>(1).ToString(),margins);
            }
        }

        private void CreateStatusGrid(string headerTitle, Thickness margins)
        {
            ScrollViewer nSv = new ScrollViewer();
            MainGrid.Children.Add(nSv);
            nSv.Margin = margins;
            nSv.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            nSv.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;

            StackPanel newStack = new StackPanel();
            nSv.Content = newStack;

            TextBlock statusHeader = new TextBlock { Text = headerTitle, FontSize = 12, TextAlignment = TextAlignment.Center };
            newStack.Children.Add(statusHeader);
            statusHeader.Margin = new Thickness(0, 0, 0, newStack.ActualHeight - 20);

            statusPanelList.Add(newStack);
        }

        private void InitializeTaskDataset() { projectConfigurationDataSet = sqDataAccess.GetConfigurationTables(); }

        private void ExitBtn_Click(object sender, RoutedEventArgs e) { this.Close(); }

        private void OpenTask_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            int taskId = int.Parse(b.ToString().ToString().Split(' ')[1].Substring(2, 1));
            Window nTask = new Window
            {
                Title = "Create new task",
                Content = new AddTaskControl(projectConfigurationDataSet, taskId),
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize
            };
            nTask.ShowDialog();
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            Window nTask = new Window
            {
                Title = "Create new task",
                Content = new AddTaskControl(projectConfigurationDataSet),
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize
            };   
            nTask.ShowDialog();
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OptionBtn_Click(object sender, RoutedEventArgs e)
        {
            OptionWindow options = new OptionWindow();
            options.Show();
        }
        private int GetItemId(string tableName, string objectName) { return projectConfigurationDataSet.Tables[tableName].Select().Where(x => x[1].Equals(objectName)).FirstOrDefault().Field<int>(0); }
        private string GetItemFromId(string tableName, int objectId) { return projectConfigurationDataSet.Tables[tableName].Select().Where(x => x[0].Equals(objectId)).FirstOrDefault().Field<string>(1); }

        private void FilterOptionCbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dependantObjectsList.Count > 0) { TopGrid.Children.Remove((UIElement)dependantObjectsList[0]); }
            dependantObjectsList = new List<object>();
            switch (FilterOptionCbx.SelectedIndex)
            {
                   //             < ComboBoxItem > No filter </ ComboBoxItem >
                   //< ComboBoxItem > Task ID </ ComboBoxItem >
                   //   < ComboBoxItem > Task name </ ComboBoxItem >
                   //      < ComboBoxItem > Task type </ ComboBoxItem >
                   //      < ComboBoxItem > Task priority </ ComboBoxItem >
                   //         < ComboBoxItem > Project component </ ComboBoxItem >
                   //            < ComboBoxItem > Assigned to </ ComboBoxItem >
                case 0: break;
                case 1:
                    TextBox idBox = new TextBox { Margin = new Thickness(100, 15, 600, 0), MaxLength = 4, FontSize=10 };
                    dependantObjectsList.Add(idBox);
                    break;
                case 2:
                    idBox = new TextBox { Margin = new Thickness(100, 15, 600, 0), MaxLength = 4, FontSize = 10 };
                    dependantObjectsList.Add(idBox);
                    break;
                case 3:
                    ComboBox statusCbx = new ComboBox { Margin = new Thickness(100, 15, 600, 0), FontSize = 10, ItemsSource = projectConfigurationDataSet.Tables["c_tasktypes"].AsEnumerable().Select(x => x[1]) };
                    dependantObjectsList.Add(statusCbx);
                    break;
                case 4:
                    ComboBox prioCbx = new ComboBox { Margin = new Thickness(100, 15, 600, 0), FontSize = 10, ItemsSource = projectConfigurationDataSet.Tables["c_taskpriorities"].AsEnumerable().Select(x => x[1]) };
                    dependantObjectsList.Add(prioCbx);
                    break;
                case 5:
                    ComboBox compCbx = new ComboBox { Margin = new Thickness(100, 15, 600, 0), FontSize = 10, ItemsSource = projectConfigurationDataSet.Tables["c_projectcomponents"].AsEnumerable().Select(x => x[1]) };
                    dependantObjectsList.Add(compCbx);
                    break;
                case 6:
                    ComboBox userCbx = new ComboBox { Margin = new Thickness(100, 15, 600, 0), FontSize = 10, ItemsSource = projectConfigurationDataSet.Tables["p_project_users"].AsEnumerable().Select(x => x[1]) };
                    dependantObjectsList.Add(userCbx);
                    break;
            }

            if(dependantObjectsList.Count > 0) { TopGrid.Children.Add((UIElement)dependantObjectsList[0]); }
        }

        private void FilterBtn_Click(object sender, RoutedEventArgs e)
        {
            switch (FilterOptionCbx.SelectedIndex)
            {
                //             < ComboBoxItem > No filter </ ComboBoxItem >
                //< ComboBoxItem > Task ID </ ComboBoxItem >
                //   < ComboBoxItem > Task name </ ComboBoxItem >
                //      < ComboBoxItem > Task type </ ComboBoxItem >
                //      < ComboBoxItem > Task priority </ ComboBoxItem >
                //         < ComboBoxItem > Project component </ ComboBoxItem >
                //            < ComboBoxItem > Assigned to </ ComboBoxItem >
                case 0: break;
                case 1:
                    TextBox idBox = new TextBox { Margin = new Thickness(100, 15, 600, 10), MaxLength = 4, FontSize = 10 };
                    dependantObjectsList.Add(idBox);
                    break;
                case 2:
                    idBox = new TextBox { Margin = new Thickness(100, 15, 600, 10), MaxLength = 4, FontSize = 10 };
                    dependantObjectsList.Add(idBox);
                    break;
                case 3:
                    ComboBox statusCbx = new ComboBox { Margin = new Thickness(100, 15, 600, 10), FontSize = 10, ItemsSource = projectConfigurationDataSet.Tables["c_taskstatus"].AsEnumerable().Select(x => x[1]) };
                    dependantObjectsList.Add(statusCbx);
                    break;
                case 4:
                    ComboBox prioCbx = new ComboBox { Margin = new Thickness(100, 15, 600, 10), FontSize = 10, ItemsSource = projectConfigurationDataSet.Tables["c_taskpriorities"].AsEnumerable().Select(x => x[1]) };
                    dependantObjectsList.Add(prioCbx);
                    break;
                case 5:
                    ComboBox compCbx = new ComboBox { Margin = new Thickness(100, 15, 600, 10), FontSize = 10, ItemsSource = projectConfigurationDataSet.Tables["c_projectcomponents"].AsEnumerable().Select(x => x[1]) };
                    dependantObjectsList.Add(compCbx);
                    break;
                case 6:
                    ComboBox userCbx = new ComboBox { Margin = new Thickness(100, 15, 600, 10), FontSize = 10, ItemsSource = projectConfigurationDataSet.Tables["p_project_users"].AsEnumerable().Select(x => x[1]) };
                    dependantObjectsList.Add(userCbx);
                    break;
            }

            LoadTasks();
        }
    }
}
