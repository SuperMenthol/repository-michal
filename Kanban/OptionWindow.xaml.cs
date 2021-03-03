using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Kanban
{
    public partial class OptionWindow : Window
    {
        private DataSet configDs;
        private int currentlyEditedSide;
        private List<Object> objectsToUpdate;
        private ComboBox activeCmbBox = new ComboBox();
        private ComboBox dependantCmbBox = new ComboBox();

        List<KeyValuePair<string, string>> paramList = new List<KeyValuePair<string, string>>()
                    {
                        new KeyValuePair<string, string>("Status","c_taskstatus"),
                        new KeyValuePair<string, string>("Priority","c_taskpriorities"),
                        new KeyValuePair<string, string>("Component","c_projectcomponents"),
                        new KeyValuePair<string, string>("User role","c_projectroles"),
                        new KeyValuePair<string, string>("Task type","c_tasktypes"),
                    };
        KeyValuePair<string, string> kvp = new KeyValuePair<string, string>();
        KeyValuePair<string, string> KVP
        {
            get { return kvp; }
            set
            {
                if (kvp.Key != value.Key)
                {
                    kvp = value;
                    ChangeValueBoxValues();
                }
            }
        }

        int editedId;
        int EditedId
        {
            get { return editedId; }
            set
            {
                if (editedId != value)
                {
                    editedId = value;
                    LoadDependantObjectsValues(editedId);
                }
            }
        }

        public OptionWindow()
        {
            InitializeComponent();
            InitializeUserData();
            if (Properties.Settings.Default.loggedRole == 1 || Properties.Settings.Default.loggedRole == 4) { MidGrid.Visibility = Visibility.Visible; }
            configDs = Application.Current.Windows.OfType<ProjectMainWindow>().ToList().FirstOrDefault().ProjectConfigurationDataSet;
        }

        void InitializeUserData()
        {
            var userInfo = new SQLDatabaseAccess().GetCredentials(Properties.Settings.Default.loggedUserId).Rows[0];
            LoginBox.Text = userInfo.Field<string>(0);
            PassBox.Text = userInfo.Field<string>(1);
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyChanges(currentlyEditedSide, activeCmbBox.SelectedIndex);
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void LeftSideActionBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != e.RemovedItems) 
            {
                LeftMainGrid.Children.Clear();
                LeftMainGrid.Background = Brushes.WhiteSmoke;
                LeftSide_InitializeControls(LeftSideActionBox.SelectedIndex);
            }
        }

        private void ActionCbx_SelectionChaned(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != e.RemovedItems)
            {
                MidMainGrid.Children.Clear();
                MidMainGrid.Background = Brushes.WhiteSmoke;
                MidSide_InitializeControls(MidActionBox.SelectedIndex);
            }
        }

        private void MidSide_InitializeControls(int index)
        {
            currentlyEditedSide = 1;
            activeCmbBox = MidActionBox;
            objectsToUpdate = new List<object>();

            switch (index)
            {
                case 0: //choose action
                    break;
                case 1: //add global attribute
                    TextBlock attrHeader = new TextBlock { Text = "Choose attribute:", FontSize = 9, Margin = new Thickness(0, 0, 0, 160) };
                    ComboBox attrBox = new ComboBox { FontSize = 9, Margin = new Thickness(0, 15, 0, 135), ItemsSource = paramList.AsEnumerable().Select(x => x.Key), SelectedIndex = 0 };
                    attrBox.SelectionChanged += AttrBox_SelectionChanged;

                    KVP = paramList[attrBox.SelectedIndex];
                    TextBlock nameHeader = new TextBlock { Text = "Name:", FontSize = 9, Margin = new Thickness(0, 40, 0, 115) };
                    TextBox nameBox = new TextBox { FontSize = 10, Margin = new Thickness(0, 55, 0, 100) };
                    TextBlock descHeader = new TextBlock { Text = "Description:", FontSize = 9, Margin = new Thickness(0, 75, 0, 80) };
                    TextBox descBox = new TextBox { FontSize = 10, Margin = new Thickness(0, 90, 0, 60) };
                    objectsToUpdate.Add(nameBox);
                    objectsToUpdate.Add(descBox);

                    MidMainGrid.Children.Add(attrHeader);
                    MidMainGrid.Children.Add(attrBox);
                    MidMainGrid.Children.Add(nameHeader);
                    MidMainGrid.Children.Add(nameBox);
                    MidMainGrid.Children.Add(descHeader);
                    MidMainGrid.Children.Add(descBox);
                    break;
                case 2: //create user
                    TextBlock loginHeader = new TextBlock { Text = "User login:", FontSize = 9, Margin = new Thickness(0, 0, 0, 160) };
                    TextBox loginBox = new TextBox { FontSize = 10, Margin = new Thickness(0, 15, 0, 135) };
                    TextBlock passHeader = new TextBlock { Text = "User password:", FontSize = 9, Margin = new Thickness(0, 40, 0, 115) };
                    TextBox passBox = new TextBox { FontSize = 10, Margin = new Thickness(0, 55, 0, 100) };
                    TextBlock roleHeader = new TextBlock { Text = "User role:", FontSize = 9, Margin = new Thickness(0, 75, 0, 85) };
                    ComboBox roleCbx = new ComboBox { FontSize = 9, Margin = new Thickness(0, 90, 0, 65) };
                    roleCbx.ItemsSource = new SQLDatabaseAccess().GetUserRoles().AsEnumerable().Select(x => x[1]);

                    MidMainGrid.Children.Add(loginHeader);
                    MidMainGrid.Children.Add(loginBox);
                    MidMainGrid.Children.Add(passHeader);
                    MidMainGrid.Children.Add(passBox);
                    MidMainGrid.Children.Add(roleHeader);
                    MidMainGrid.Children.Add(roleCbx);

                    objectsToUpdate.Add(loginBox);
                    objectsToUpdate.Add(passBox);
                    objectsToUpdate.Add(roleCbx);
                    break;
                case 3: //manage user
                    TextBlock userHeader = new TextBlock { Text = "User login:", FontSize = 9, Margin = new Thickness(0, 0, 0, 160) };
                    ComboBox userBox = new ComboBox { FontSize = 9, Margin = new Thickness(0, 15, 0, 135) };
                    userBox.ItemsSource = new SQLDatabaseAccess().GetAllUserLogins().AsEnumerable().Select(x => x[1]);
                    userBox.SelectionChanged += UserBox_SelectionChanged;

                    roleHeader = new TextBlock { Text = "User role:", FontSize = 9, Margin = new Thickness(0, 40, 0, 115) };
                    ComboBox RoleCbx = new ComboBox { FontSize = 9, Margin = new Thickness(0, 55, 0, 100) };
                    RoleCbx.ItemsSource = new SQLDatabaseAccess().GetUserRoles().AsEnumerable().Select(x => x[1]);
                    dependantCmbBox = RoleCbx;

                    MidMainGrid.Children.Add(userHeader);
                    MidMainGrid.Children.Add(userBox);
                    MidMainGrid.Children.Add(roleHeader);
                    MidMainGrid.Children.Add(RoleCbx);

                    objectsToUpdate.Add(userBox);
                    objectsToUpdate.Add(RoleCbx);
                    break;
                case 4: //delete project
                    TextBlock projHeader = new TextBlock { Text = "Choose project:", FontSize = 9, Margin = new Thickness(0, 0, 0, 160) };
                    ComboBox projCbx = new ComboBox { FontSize = 9, Margin = new Thickness(0, 15, 0, 135) };
                    projCbx.ItemsSource = new SQLDatabaseAccess().GetProjectsList().AsEnumerable().Select(x => x[1]);

                    objectsToUpdate.Add(projCbx);

                    MidMainGrid.Children.Add(projHeader);
                    MidMainGrid.Children.Add(projCbx);
                    break;
                case 5: //global reset
                    break;
                default: break;
            }
        }

        private void UserBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dependantCmbBox.SelectedIndex = new SQLDatabaseAccess().GetUser(e.AddedItems[0].ToString()).AsEnumerable().FirstOrDefault().Field<int>(2)-1;
        }

        private void LeftSide_InitializeControls(int index)
        {
            currentlyEditedSide = 0;
            activeCmbBox = LeftSideActionBox;
            objectsToUpdate = new List<Object>();

            switch (index)
            {
                case 0: //choose action
                    break;
                case 1: //add user to project
                    var logins = new SQLDatabaseAccess().GetAllUserLogins().AsEnumerable();
                    var roles = configDs.Tables["c_projectroles"].AsEnumerable();
                    TextBlock loginHeader = new TextBlock { Text = "Choose user:", FontSize = 9, Margin = new Thickness(0, 0, 0, 160) };
                    ComboBox userList = new ComboBox { FontSize = 10, ItemsSource = logins.Select(x => x[1]), Margin = new Thickness(0, 15, 0, 135) };
                    TextBlock roleHeader = new TextBlock { Text = "Choose role:", FontSize = 9, Margin = new Thickness(0, 35, 0, 115) };
                    ComboBox roleList = new ComboBox { FontSize = 10, ItemsSource = roles.Select(x => x[1]), Margin = new Thickness(0, 50, 0, 100) };
                    dependantCmbBox = userList;
                    objectsToUpdate.Add(roleList);

                    LeftMainGrid.Children.Add(userList);
                    LeftMainGrid.Children.Add(roleList);
                    LeftMainGrid.Children.Add(loginHeader);
                    LeftMainGrid.Children.Add(roleHeader);
                    break;
                case 2: //manage project user
                    logins = new SQLDatabaseAccess().GetAllUserLogins().AsEnumerable().Where(x => configDs.Tables["p_project_users"].AsEnumerable().Select(y=>y[0]).Contains(x[0]));
                    roles = configDs.Tables["c_projectroles"].AsEnumerable();
                    TextBlock userHeader = new TextBlock { Text = "Choose user:", FontSize = 9, Margin = new Thickness(0, 0, 0, 160) };
                    userList = new ComboBox { FontSize = 10, ItemsSource = logins.Select(x => x[1]), Margin = new Thickness(0, 15, 0, 135) };
                    roleHeader = new TextBlock { Text = "Choose role:", FontSize = 9, Margin = new Thickness(0, 35, 0, 115) };
                    roleList = new ComboBox { FontSize = 10, ItemsSource = roles.Select(x => x[1]), Margin = new Thickness(0, 50, 0, 100) };
                    CheckBox deleteUserBox = new CheckBox { FontSize = 10, Content = "Remove from project", Margin = new Thickness(0, 75, 0, 80) };

                    LeftMainGrid.Children.Add(userList);
                    LeftMainGrid.Children.Add(roleList);
                    LeftMainGrid.Children.Add(userHeader);
                    LeftMainGrid.Children.Add(roleHeader);
                    LeftMainGrid.Children.Add(deleteUserBox);

                    objectsToUpdate.Add(deleteUserBox);
                    objectsToUpdate.Add(userList);
                    objectsToUpdate.Add(roleList);
                    break;
                case 3: //edit attributes
                    TextBlock attrHeader = new TextBlock { Text = "Choose attribute:", FontSize = 9, Margin = new Thickness(0, 0, 0, 160) };
                    ComboBox attrBox = new ComboBox { FontSize = 10, Margin = new Thickness(0, 15, 0, 135), ItemsSource = paramList.AsEnumerable().Select(x => x.Key), SelectedIndex = 0 };
                    attrBox.SelectionChanged += AttrBox_SelectionChanged;

                    KVP = paramList[attrBox.SelectedIndex]; //called to fill in below controls
                    TextBlock valueHeader = new TextBlock { Text = "Choose value:", FontSize = 9, Margin = new Thickness(0, 35, 0, 115) };
                    ComboBox valueBox = new ComboBox { FontSize = 10, Margin = new Thickness(0, 50, 0, 100), ItemsSource = configDs.Tables[kvp.Value].AsEnumerable().Select(x=>x[1]), SelectedIndex = 0 };
                    dependantCmbBox = valueBox;
                    valueBox.SelectionChanged += ValueBox_SelectionChanged;

                    TextBlock nameHeader = new TextBlock { Text = "Name:", FontSize = 9, Margin = new Thickness(0, 70, 0, 80) };
                    TextBox nameBox = new TextBox { FontSize = 10, Margin = Margin = new Thickness(0, 85, 0, 65) };
                    TextBlock descHeader = new TextBlock { Text = "Description:", FontSize = 9, Margin = new Thickness(0, 105, 0, 45) };
                    TextBox descBox = new TextBox { FontSize = 10, Margin = new Thickness(0, 120, 0, 30) };
                    objectsToUpdate.Add(nameBox);
                    objectsToUpdate.Add(descBox);

                    EditedId = valueBox.SelectedIndex;
                    Button crtBtn = new Button { FontSize = 10, Content = "Add", Margin = new Thickness(0, 140, 60, 0) };
                    Button deleteBtn = new Button { FontSize = 10, Content = "Delete", Margin = new Thickness(60, 140, 0, 0) };
                    crtBtn.Click += CrtBtn_Click;
                    deleteBtn.Click += DeleteBtn_Click;

                    LeftMainGrid.Children.Add(attrHeader);
                    LeftMainGrid.Children.Add(attrBox);
                    LeftMainGrid.Children.Add(valueHeader);
                    LeftMainGrid.Children.Add(valueBox);
                    LeftMainGrid.Children.Add(nameHeader);
                    LeftMainGrid.Children.Add(nameBox);
                    LeftMainGrid.Children.Add(descHeader);
                    LeftMainGrid.Children.Add(descBox);
                    LeftMainGrid.Children.Add(crtBtn);
                    LeftMainGrid.Children.Add(deleteBtn);
                    break;
                default:
                    break;
            }
        }

        private object DataRowField(EnumerableRowCollection<DataRow> dt, int field) { return dt.AsEnumerable().FirstOrDefault().Field<object>(field); }

        private void ApplyChanges(int side, int action)
        {
            switch (side)
            {
                case 0:
                    switch (action)
                    {
                        case 1: //add user
                            ComboBox role = (ComboBox)objectsToUpdate[0];
                            SQLDatabaseAccess sqDb = new SQLDatabaseAccess();
                            List<KeyValuePair<string, object>> paramList = new List<KeyValuePair<string, object>>();
                            paramList.Add(new KeyValuePair<string, object>("@uid", (int)DataRowField(sqDb.GetUser((string)dependantCmbBox.SelectedItem).AsEnumerable(),0)));
                            paramList.Add(new KeyValuePair<string, object>("@sysrole", (int)DataRowField(sqDb.GetUser((string)dependantCmbBox.SelectedItem).AsEnumerable(), 2)));
                            paramList.Add(new KeyValuePair<string, object>("@urole", (int)DataRowField(configDs.Tables["c_projectroles"].AsEnumerable().Where(x=>x[1] == role.SelectedItem),0)));

                            Application.Current.Windows.OfType<ProjectMainWindow>().ToList()[0].SqDataAccess.AddUserToProject(paramList);
                            break;
                        case 2: //edit user role/remove user from project
                            CheckBox deleteCbx = (CheckBox)objectsToUpdate[0];
                            ComboBox user = (ComboBox)objectsToUpdate[1];
                            role = (ComboBox)objectsToUpdate[2];

                            if (deleteCbx.IsChecked == true)
                            {
                                if (MessageBox.Show("Are you sure you want to delete this user from the project? This operation cannot be reversed","Deletion of user", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                                {
                                    Application.Current.Windows.OfType<ProjectMainWindow>().ToList()[0].SqDataAccess.RemoveUserFromProject((int)DataRowField(new SQLDatabaseAccess().GetUser(user.SelectedItem.ToString()).AsEnumerable(), 0));
                                }
                            }
                            else
                            {
                                List<int> dataList = new List<int>();
                                dataList.Add(new SQLDatabaseAccess().GetUser(user.SelectedItem.ToString()).AsEnumerable().FirstOrDefault().Field<int>(0));
                                dataList.Add(configDs.Tables["c_projectroles"].AsEnumerable().Where(x => x[1] == role.SelectedItem).FirstOrDefault().Field<int>(0));

                                Application.Current.Windows.OfType<ProjectMainWindow>().ToList()[0].SqDataAccess.UpdateProjectUser(dataList);
                            }
                            break;
                        case 3: //update parameter
                            Application.Current.Windows.OfType<ProjectMainWindow>().ToList()[0].SqDataAccess.UpdateParameter(kvp.Value, EditedId, GenerateParamList());
                            break;
                    }
                    break;
                case 1:
                    switch (action)
                    {
                        case 0: //choose action
                            break;
                        case 1: //add global attribute
                            TextBox nameBox = (TextBox)objectsToUpdate[0];
                            TextBox descBox = (TextBox)objectsToUpdate[1];
                            List<KeyValuePair<string, string>> paramList = new List<KeyValuePair<string, string>>();
                            paramList.Add(new KeyValuePair<string, string>("@attrname", nameBox.Text));
                            paramList.Add(new KeyValuePair<string, string>("@attrdesc", descBox.Text));

                            new SQLDatabaseAccess().InsertGlobalParameter(kvp.Value, paramList);
                            break;
                        case 2: //create user
                            TextBox loginBox = (TextBox)objectsToUpdate[0];
                            TextBox passBox = (TextBox)objectsToUpdate[1];
                            ComboBox roleBox = (ComboBox)objectsToUpdate[2];

                            new SQLDatabaseAccess().CreateNewUser(loginBox.Text, passBox.Text, roleBox.SelectedIndex + 1);
                            break;
                        case 3: //manage user
                            ComboBox userCbx = (ComboBox)objectsToUpdate[0];
                            ComboBox roleCbx = (ComboBox)objectsToUpdate[1];

                            new SQLDatabaseAccess().UpdateUserRole(userCbx.SelectedItem.ToString(), roleCbx.SelectedIndex + 1);
                            break;
                        case 4: //delete project
                            ComboBox projCbx = (ComboBox)objectsToUpdate[0];

                            new SQLDatabaseAccess().DropProject(projCbx.Text);
                            break;
                        case 5: //global reset
                            break;
                        default: break;
                    }
                    break;
                case 2:
                    var sqDbAccess = new SQLDatabaseAccess();
                    if ((string)DataRowField(sqDbAccess.GetUser(LoginBox.Text).AsEnumerable(),1) != (string)DataRowField(sqDbAccess.GetUser(Properties.Settings.Default.loggedUserId).AsEnumerable(),1)) //checking login to avoid duplicate names
                    {
                        if (sqDbAccess.LogonValidation(LoginBox.Text, PassBox.Text).Rows.Count == 0)
                        {
                            if (MessageBox.Show("Are you sure you want to change data?", "Changing user data", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            {
                                sqDbAccess.UpdateUser(LoginBox.Text, PassBox.Text);
                            }
                        }
                    }
                    
                    break;
                default:
                    MessageBox.Show("This control is not implemented", "Error on ApplyChanges method", MessageBoxButton.OK);
                    break;
            }
        }

        private List<KeyValuePair<string,object>> GenerateParamList()
        {
            TextBox nameBlock = (TextBox)objectsToUpdate[0];
            TextBox descBlock = (TextBox)objectsToUpdate[1];

            var paramList = new List<KeyValuePair<string, object>>();
            paramList.Add(new KeyValuePair<string, object>("@nm", nameBlock.Text));
            paramList.Add(new KeyValuePair<string, object>("@desc", descBlock.Text));

            return paramList;
        }

        private void AttrBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { KVP = paramList.Where(x => x.Key == (string)e.AddedItems[0]).FirstOrDefault(); }
        private void ValueBox_SelectionChanged(object sender, SelectionChangedEventArgs e) 
        { 
            if (dependantCmbBox.SelectedIndex > -1)
            {
                EditedId = configDs.Tables[kvp.Value].AsEnumerable().Where(x => x[1] == dependantCmbBox.SelectedItem).FirstOrDefault().Field<int>(0);
            }
        }
        private void ChangeValueBoxValues() { if (dependantCmbBox != null) { dependantCmbBox.ItemsSource = configDs.Tables[kvp.Value].AsEnumerable().Select(x => x[1]); } }
        private void LoadDependantObjectsValues(int eId) 
        {
            TextBox nameTxt = (TextBox)objectsToUpdate[0];
            TextBox descTxt = (TextBox)objectsToUpdate[1];

            if (eId > -1)
            {
                DataRow dRow = configDs.Tables[kvp.Value].AsEnumerable().Where(x => x[1] == dependantCmbBox.SelectedItem).FirstOrDefault();

                nameTxt.Text = dRow.Field<string>(1);
                descTxt.Text = dRow.Field<string>(2);
            }
            else
            {
                nameTxt.Text = string.Empty;
                descTxt.Text = string.Empty;
            }
        }

        private void CrtBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Windows.OfType<ProjectMainWindow>().ToList()[0].SqDataAccess.InsertParameter(kvp.Value, GenerateParamList());
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete this parameter? This operation cannot be reversed", "Deletion of parameter", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Application.Current.Windows.OfType<ProjectMainWindow>().ToList()[0].SqDataAccess.RemoveParameter(kvp.Value, EditedId);
            }
        }

        private void ShowPwBox_Checked(object sender, RoutedEventArgs e)
        {
            if (ShowPwBox.IsChecked == true)
            {
            }
            else
            {
            }
        }

        private void LoginBox_GotFocus(object sender, RoutedEventArgs e)
        {
            currentlyEditedSide = 2;
        }
    }
}
