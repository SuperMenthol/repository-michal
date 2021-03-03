using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Kanban
{
    public partial class AddTaskControl : UserControl
    {
        private DataTable userDt;
        private DataSet configDs;
        public AddTaskControl(DataSet ds, int taskId = 0)
        {
            InitializeComponent();
            configDs = ds;
            ConfigureFields();
            if (taskId > 0) { LoadTask(taskId); }
        }

        public void ConfigureFields()
        {
            Tp_CmbBox.ItemsSource = configDs.Tables["c_tasktypes"].AsEnumerable().Select(x => x[1] as string).ToList();
            Prio_CmbBox.ItemsSource = configDs.Tables["c_taskpriorities"].AsEnumerable().Select(x => x[1] as string).ToList();
            Comp_CmbBox.ItemsSource = configDs.Tables["c_projectcomponents"].AsEnumerable().Select(x => x[1] as string).ToList();
            Stat_CmbBox.ItemsSource = configDs.Tables["c_taskstatus"].AsEnumerable().Select(x => x[1] as string).ToList();
            Cnn1_CmbBox.ItemsSource = configDs.Tables["p_project_tasks"].AsEnumerable().Select(x => x[1] as string).ToList();
            if (configDs.Tables["p_project_users"].Rows.Count > 0)
            {
                userDt = new SQLDatabaseAccess().GetProjectUserLogins(configDs.Tables["p_project_users"].AsEnumerable().Select(x => x[1] as string).ToList());
                Assign1_CmbBox.ItemsSource = userDt.AsEnumerable().Select(x => x[1] as string).ToList();
            }
        }

        private void LoadTask(int taskId)
        {
            Id_TxtBox.Text = taskId.ToString();
            DataRow dRow = GetTask(taskId);

            Nm_TxtBox.Text = dRow.Field<string>(1);
            Tp_CmbBox.SelectedItem = GetItemFromId("c_tasktypes", dRow.Field<int>(3));
            Prio_CmbBox.SelectedItem = GetItemFromId("c_taskpriorities", dRow.Field<int>(5));
            Comp_CmbBox.SelectedItem = GetItemFromId("c_projectcomponents", dRow.Field<int>(4));
            Desc_TxtBox.Text = dRow.Field<string>(2);
            Stat_CmbBox.SelectedItem = GetItemFromId("c_taskstatus", dRow.Field<int>(6));
        }

        private void DiscardBtn_Click(object sender, RoutedEventArgs e) { var par = this.Parent as Window; par.Close(); }
        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            var sqDbAccess = new SQLDatabaseAccess();
            var pmw = Application.Current.Windows.OfType<ProjectMainWindow>().ToList().FirstOrDefault();

            List<KeyValuePair<string, object>> inputList = new List<KeyValuePair<string, object>>();

            inputList.Add(new KeyValuePair<string, object>("@task_title", Nm_TxtBox.Text));
            inputList.Add(new KeyValuePair<string, object>("@task_description", Desc_TxtBox.Text));
            inputList.Add(new KeyValuePair<string, object>("@task_type", GetItemId("c_tasktypes", Tp_CmbBox.SelectedItem.ToString())));
            inputList.Add(new KeyValuePair<string, object>("@task_status", GetItemId("c_taskstatus", Stat_CmbBox.SelectedItem.ToString())));
            inputList.Add(new KeyValuePair<string, object>("@task_priority", GetItemId("c_taskpriorities", Prio_CmbBox.SelectedItem.ToString())));
            inputList.Add(new KeyValuePair<string, object>("@task_component", GetItemId("c_projectcomponents", Comp_CmbBox.Text)));
            if (Assign1_CmbBox.SelectedIndex > -1)
            {
                int usId = sqDbAccess.GetUser(Assign1_CmbBox.SelectedItem.ToString()).AsEnumerable().FirstOrDefault().Field<int>(0);
                inputList.Add(new KeyValuePair<string, object>("@task_assignee1", usId));
            }
            if (Cnn1_CmbBox.SelectedIndex > -1)
            {
                int connectedTask = pmw.SqDataAccess.GetTaskId(Cnn1_CmbBox.SelectedItem.ToString());
                inputList.Add(new KeyValuePair<string, object>("@task_connection1", connectedTask));
            }

            if (string.IsNullOrEmpty(Id_TxtBox.Text)) { pmw.SqDataAccess.AddTaskToTable(inputList); }
            else
            {
                inputList.Add(new KeyValuePair<string, object>("@task_id", Id_TxtBox.Text));
                pmw.SqDataAccess.UpdateTask(inputList);
            }

            Application.Current.Windows.OfType<ProjectMainWindow>().ToList()[0].LoadTasks();
        }

        private int GetItemId(string tableName, string objectName) { return configDs.Tables[tableName].Select().Where(x => x[1].Equals(objectName)).FirstOrDefault().Field<int>(0); }
        private string GetItemFromId(string tableName, int objectId) { return configDs.Tables[tableName].Select().Where(x => x[0].Equals(objectId)).FirstOrDefault().Field<string>(1); }
        private DataRow GetTask(int taskId) { return configDs.Tables["p_project_tasks"].Select().Where(x => x[0].Equals(taskId)).FirstOrDefault(); }

        private bool TaskTitleValidation(string inputString, SQLDataAccess sqlComponent)
        {
            if (Nm_TxtBox.Text == string.Empty) { return false; }
            else if (sqlComponent.GetTaskId(inputString) != 0) { return false; }
            else return true;
        }
    }
}
