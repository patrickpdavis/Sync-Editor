using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DiffMatchPatch;

namespace WpfClientGUI
{
    //This code is the main form the user interacts with.
    public partial class MainWindow : Window
    {
        #region Data Members
        //Satic Data Members
        public static string BINDING = "net.tcp://";
        public static string TARGET = "/main";

        //Non-Static Data Members
        //Address of the server pulled from login window.
        public string address;
        //String used for getting objects from other forms.
        public string returnString;
        //A client object containing the users information.
        public Client myInfo;
        //The object that allows connection over the network.
        public IServer network;
        //The channel used to connect to the network.
        public ChannelFactory<IServer> clientChannel;
        //Open file that the user is editing.
        public Document openDocument;
        //This boolean value is used to control the thread that gets or sends data to the server.
        public bool hasLock;
        string currentTextboxText;
        //Diff_Match_Patch for matching and patching.
        public diff_match_patch diffMatch;
        #endregion

        #region Constructor
        public MainWindow()
        {
            //Window for login
            LoginWindow loginWindow = new LoginWindow(this);
            hasLock = false;
            currentTextboxText = "";

            //Starts Windows
            InitializeComponent();
            this.Show();
            loginWindow.ShowDialog();

            //Post Start Area
            //Stops the text from wrapping on its own.
            DocumentText.Document.PageWidth = 10000;

            //Disables to lock options.
            lockRequest.IsEnabled = false;
            releaseLock.IsEnabled = false;

            //Makes document text read only until user creates new file or opens file.
            DocumentText.IsReadOnly = true;


            //diff Match stuff.
            diffMatch = new diff_match_patch();

            //Welcome Message.
            richTextBoxText.AppendText("Welcome to Live Pair Coding Editor!\n");

            //Thread for Messages
            Thread textThread = new Thread(new ThreadStart(getMessages));
            textThread.Start();

            //Thread for document changes
            Thread userThread = new Thread(new ThreadStart(getUsers));
            userThread.Start();

            //Gets the document Changes
            Thread DocChangesThread = new Thread(new ThreadStart(changes));
            DocChangesThread.Start();



        }
        #endregion

        #region Normal Methods
        //Allows the login window to connect this form to the server.
        public FailureType connect(LoginWindow loginWindow, string address, Client user)
        {
            try
            {
                //Saves the address and binding mode.
                this.address = address;
                NetTcpBinding netBinding = new NetTcpBinding();
                netBinding.Security.Mode = SecurityMode.None;

                //Creates channelfactor and network object.
                clientChannel = new ChannelFactory<IServer>(netBinding, BINDING + address + TARGET);
                network = clientChannel.CreateChannel();

                return FailureType.Good;
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Error: Something went wrong on connect.", "Error");
                return FailureType.FailedUnknownReason;
            }
        }

        //Sends message from IM window.
        private void buttonSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Network call.
                network.sendText(new TextMessage(myInfo, textBoxMessage.Text + "\n"));
                textBoxMessage.Text = "";
            }
            catch (Exception err)
            {
                // on exception being thrown call onDisconnect method
                // and pass exception to be displayed to user
                onDisconnect(err);
            }

        }

        //Tells the server to create a new document.
        public void newFile_Click(object sender, RoutedEventArgs e)
        {
            if(hasLock == true)
            {
                releaseLock_Click(null, null);
            }
            GrabFileString grabFileDialogBox = new GrabFileString();
            grabFileDialogBox.ShowDialog();

            if (grabFileDialogBox.nameOfFile == null)
            {
                return;
            }

            string nameOfFile = grabFileDialogBox.nameOfFile;
            try
            {
                network.newFile(nameOfFile, myInfo);
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Error: Something went wrong when creating a new file.", "Error");
            }

            //Opens the document and allows the user to get a lock.
            openDocument = new Document(nameOfFile, "");
            lockRequest.IsEnabled = true;
            releaseLock.IsEnabled = true;

        }

        //Calls the openFile method below
        public void openFile_Click(object sender, RoutedEventArgs e)
        {
            FileWindow files = new FileWindow(network.getFiles(myInfo), this);
            files.ShowDialog();
        }

        //Calls the server and ask for a document.
        public void openFile(string nameOfFile)
        {
            
            if (hasLock == false)
            {
                try
                {
                    if (nameOfFile != "")
                    {
                        //Network call.
                        openDocument = network.openFile(nameOfFile, myInfo);
                        //Clears the text box.
                        DocumentText.Document.Blocks.Clear();
                        //Appends the grabbed document.
                        DocumentText.AppendText(openDocument.FileContents);
                        //Sets current textbox string for the thread to send and recieve changes.
                        currentTextboxText = openDocument.FileContents;
                        //Unlocks the lock request and drop buttons.
                        lockRequest.IsEnabled = true;
                        releaseLock.IsEnabled = true;
                    }
                }
                catch (Exception err)
                {
                    onDisconnect(err);
                }
            }
            else
            {
                releaseLock_Click(null, null);
                openFile(nameOfFile);
                //System.Windows.MessageBox.Show("Please release the lock before opening a new file.", "Error");
            }
        }

        //Call for exit button.
        private void exitButton_click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //On text changed the current textbox string is updated.
        //Originally doing this in a thread caused a crash when the user backspaced.
        //This is a temporary work around and may cause performance issues on larger documents.
        private void DocumentText_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                //Sets string to contents of document text.
                currentTextboxText = new TextRange(DocumentText.Document.ContentStart, DocumentText.Document.ContentEnd).Text;

            }
            catch (Exception err)
            {
                // on exception being thrown call onDisconnect method
                // and pass exception to be displayed to user
                System.Windows.MessageBox.Show("Error: Something went wrong on connect.", "Error");
                onDisconnect(err);
            }
        }

        //Shows a user the error that occured and the quietly kills the program.
        private void onDisconnect(Exception e)
        {
            this.Dispatcher.Invoke(new Action(() => { this.Hide(); }));
            System.Windows.MessageBox.Show(e.Message, "Error");
            Environment.Exit(1);
        }

        //Closes the program when exit is hit.
        private void Window_Closed(object sender, EventArgs e)
        {
            // this needs 
            if (clientChannel != null)
            {
                clientChannel.Close();
            }
            this.Close();
            Environment.Exit(1);
        }

        //Request lock from server for a document.
        private void lockRequest_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                if (network.requestLock(openDocument.FileName, myInfo) == FailureType.Good)
                {
                    DocumentText.IsReadOnly = false;
                    hasLock = true;
                }
                else
                {
                    System.Windows.MessageBox.Show("Lock In Use");
                }
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Error: Something went wrong when trying to grab the lock.", "Error");
                return;
            }

        }

        //Tells the server to release a document lock.
        private void releaseLock_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (network.releaseLock(openDocument.FileName, myInfo) == FailureType.Good)
                {
                    DocumentText.IsReadOnly = true;
                    hasLock = false;
                }
                else
                {
                    System.Windows.MessageBox.Show("Something Went Wrong :(");
                }
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Error: Something went wrong when trying to grab the lock.", "Error");
                return;
            }

        }

        //Tells the server to write the document to file.
        private void saveFile_click(object sender, RoutedEventArgs e)
        {
            try
            {
                network.saveFile(openDocument.FileName, myInfo);
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Error: Something went wrong when saving the document.", "Error");
            }
        }

        //Intercepts the tab key and replaces it with 4 spaces.
        //This is a workaround for a deficiency in the Diff Match Patch api and the richtextbox.
        private void DocumentText_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                e.Handled = true;
                SendKeys.SendWait("    ");
            }
        }

        //Opens a window to confirm a user.
        private void confirmUsers_Click(object sender, RoutedEventArgs e)
        {
            // goes over to server and checks if I'm saved as an admin
            if (network.amIAdmin(myInfo.Name))
            {
                // good, I'm an admin, now I can confirm other users
                List<string> unconfirmedUsers = network.getUnconfirmedUsers();

                ConfirmUsers confirmUserWindow = new ConfirmUsers(unconfirmedUsers, this);
                confirmUserWindow.ShowDialog();
            }
            else
            {
                // Whoops! Looks like you're not an admin, not soup for you!
                System.Windows.MessageBox.Show("You're Not an Admin Bro!");
            }

        }

        //Sets a call to the server to confirm a user.
        public void confirmingUserAction(string userToConfirm)
        {
            network.confirmUsers(userToConfirm, myInfo);
        }
        #endregion

        #region Threaded Methods
        //Threaded method for getting text messages
        private void getMessages()
        {
            try
            {
                while (true)
                {
                    System.Threading.Thread.Sleep(500);
                    try
                    {
                        foreach (TextMessage item in network.getTexts(myInfo))
                        {
                            this.Dispatcher.Invoke(new Action(() => { richTextBoxText.AppendText(item.User + ": " + item.Message); richTextBoxText.ScrollToEnd(); }));
                        }
                    }
                    catch (Exception)
                    {
                        //MessageBoxResult result = System.Windows.MessageBox.Show("Error: Connection to the server was lost. Would you like to reconnect?", "Error", MessageBoxButton.YesNo);
                        //if (result == MessageBoxResult.No)
                        //{
                        //    this.Close();
                        //    Environment.Exit(1);
                        //}
                    }
                }
            }
            catch (Exception err)
            {
                // on exception being thrown call onDisconnect method
                // and pass exception to be displayed to user
                onDisconnect(err);
            }
        }

        private void getUsers()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(2500);
                try
                {
                    this.Dispatcher.Invoke(new Action(() => { listBoxUsers.ItemsSource = new BindingList<string>(network.getUsers()); }));
                }
                catch (Exception)
                {
                    //MessageBoxResult result = System.Windows.MessageBox.Show("Error: Connection to the server was lost. Would you like to reconnect?", "Error", MessageBoxButton.YesNo);
                    //if (result == MessageBoxResult.No)
                    //{
                    //    this.Close();
                    //    Environment.Exit(1);
                    //}
                }
            }
        }

        public void changes()
        {
            while (true)
            {
                if (openDocument != null)
                {
                    List<Patch> patches;
                    Dictionary<int, List<Patch>> returnedPatches;
                    Object[] temp;

                    while (true)
                    {
                        try
                        {
                            if (hasLock == true)
                            {
                                Thread.Sleep(1000);
                                //Copies the document Textbox
                                // TextRange range = new TextRange(DocumentText.Document.ContentStart, DocumentText.Document.ContentEnd);
                                string myText2 = currentTextboxText;
                                patches = diffMatch.patch_make(diffMatch.diff_main(openDocument.FileContents, myText2));

                                temp = diffMatch.patch_apply(patches, openDocument.FileContents);

                                openDocument.FileContents = temp[0].ToString();

                                network.sendDocChanges(openDocument.FileName, patches, myInfo);
                                openDocument.SaveID = network.getLastPatchID(openDocument.FileName) + 1;
                            }
                            else
                            {
                                Thread.Sleep(1000);
                                returnedPatches = network.getDocChanges(openDocument.FileName, myInfo, openDocument.SaveID);

                                if (returnedPatches.Count > 0)
                                {
                                    foreach (List<Patch> item in returnedPatches.Values)
                                    {
                                        temp = diffMatch.patch_apply(item, openDocument.FileContents);

                                        openDocument.FileContents = temp[0].ToString();
                                        this.Dispatcher.Invoke(new Action(() => { DocumentText.Document.Blocks.Clear(); DocumentText.AppendText(openDocument.FileContents); }));
                                    }
                                    openDocument.SaveID = returnedPatches.Last().Key + 1;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            //MessageBoxResult result = System.Windows.MessageBox.Show("Error: Connection to the server was lost. Would you like to reconnect?", "Error", MessageBoxButton.YesNo);
                            //if (result == MessageBoxResult.No)
                            //{
                            //    this.Close();
                            //    Environment.Exit(1);
                            //}
                        }
                    }
                }
            }
        }
        #endregion
    }
}
