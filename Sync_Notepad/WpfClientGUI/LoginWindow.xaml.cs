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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ServiceModel;
using System.IO;

namespace WpfClientGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        #region Data Members
        //Location for save files.
        public string serverFile = "\\serverAddress.txt";
        public string userFile = "\\username.txt";
        //Data Members
        //Parent mainwindow.
        public MainWindow mainWindow;
        //Used for confirming an admin.
        bool isDirect;
        #endregion

        #region Constructors
        public LoginWindow(MainWindow group)
        {
            this.mainWindow = group;
            InitializeComponent();

            group.IsEnabled = false;
            this.isDirect = false;

            if (File.Exists(Client.pathToDir + serverFile))
            {
                textBoxServer.Text = File.ReadAllText(Client.pathToDir + serverFile);
            }
            else
            {
                File.WriteAllText(Client.pathToDir + serverFile, textBoxServer.Text);
            }
            
            if(File.Exists(Client.pathToDir + userFile))
            {
                textBoxUsername.Text = File.ReadAllText(Client.pathToDir + userFile);
            }
            else
            {
                File.WriteAllText(Client.pathToDir + userFile, textBoxUsername.Text);
            }
        }
        #endregion

        #region Methods
        //Connects to server sets stuff.
        private void buttonLogin_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText(Client.pathToDir + serverFile, textBoxServer.Text);
            File.WriteAllText(Client.pathToDir + userFile, textBoxUsername.Text);
            //Creates a channel factory
            if (isDirect == false)
            {
                mainWindow.myInfo = new Client(textBoxUsername.Text, textBoxPassword.Password);
                mainWindow.connect(this, textBoxServer.Text, mainWindow.myInfo);
            }
            else
            {
                if (mainWindow.myInfo == null)
                {
                    MessageBox.Show("Please attempt a regular login first", "Error");
                    return;
                }
            }


            //Creates a remote class the interface.
            try
            {
                //Sets some stuff
                mainWindow.network = mainWindow.clientChannel.CreateChannel();
                FailureType loginCheck;


                //Generic response handling.
                if (isDirect == true)
                {
                    loginCheck = mainWindow.network.setAdminDirect(textBoxUsername.Text, textBoxPassword.Password);
                    if (loginCheck == FailureType.Good)
                    {
                        MessageBox.Show("You are now an Admin on this server! Please login normally.", "Success");
                    }
                    else if (loginCheck == FailureType.FailedBadPassword)
                    {
                        MessageBox.Show(
                           "The account doesn't exist or and incorrect password was entered." +
                            " Please return to the other login page and request and account or enter a correct server password.", "Error");
                    }
                }
                else
                {

                    loginCheck = mainWindow.network.login(mainWindow.myInfo);
                    if (loginCheck == FailureType.Good)
                    {
                        //This will launch the filegroup gui later
                        //MessageBox.Show("Success");
                        this.Hide();
                        mainWindow.IsEnabled = true;
                    }
                    else if (loginCheck == FailureType.FailedBadPassword)
                    {
                        MessageBox.Show("Invalid Password Entered.");
                    }
                    else if(loginCheck == FailureType.FailedUserAlreadyOnline)
                    {
                        MessageBox.Show("User is already online", "Error");
                    }
                    else
                    {
                        MessageBoxResult result;


                        result = MessageBox.Show(
                        "The account doesn't exist or and incorrect password was entered. Would you like to request an account?", "Error",
                        MessageBoxButton.YesNo);


                        if (result == MessageBoxResult.Yes)
                        {
                            FailureType requestCheck = mainWindow.network.requestAccount(mainWindow.myInfo);
                            if (requestCheck == FailureType.Good)
                            {
                                MessageBox.Show("Account Requested. Please try logging in again after your account has been confirmed or directly login with the server password.");
                            }
                            else if (requestCheck == FailureType.FailedUserAlreadyExist)
                            {
                                MessageBox.Show("Account Already Exist");
                            }
                            else
                            {
                                MessageBox.Show("Account request failed for unknown reason. Please try again.");
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show(
                    "Failed to Connect", "Error",
                    MessageBoxButton.OK);
                return;
            }

        }

        //Login for admin.
        private void Direct_Login_Click(object sender, RoutedEventArgs e)
        {
            if (isDirect == false)
            {
                isDirect = true;
                labelPassword.Content = "Server Password";
                login.Header = "Regular Login";
            }
            else
            {
                isDirect = false;
                labelPassword.Content = "Password";
                login.Header = "Direct Login";
            }
        }

        //Exit button.
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //Window Closing event.
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mainWindow.Close();
            Environment.Exit(0);

        }
        #endregion
    }
}
