using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfClientGUI
{
    /// <summary>
    /// Interaction logic for ConfirmUsers.xaml
    /// </summary>
    public partial class ConfirmUsers : Window
    {
        private MainWindow mainWindow;

        public ConfirmUsers(List<string> unconfirmedUsers, MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;

            foreach(string currUser in unconfirmedUsers)
            {
                listOfUsers.Items.Add(currUser);
            }
        }

        private void submitButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.confirmingUserAction(listOfUsers.SelectedItem.ToString());
            this.Close();
        }
    }
}
