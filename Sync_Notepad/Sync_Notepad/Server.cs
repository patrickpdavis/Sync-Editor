﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool
//     Changes to this file will be lost if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using DiffMatchPatch;

[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
//[DataContract]
public class Server : IServer
{
    #region Data members
    //Users registered on the server.
    private ClientList users;
    //Text Messages that have been sent.
    List<TextMessage> textMessages;
    //List of openDocuments
    private List<Document> listOfDocuments;
    //Servers randomly generated password.
    private string serverPassword;
    #endregion

    #region Constructor
    public Server()
    {
        //Initializes Datamembers
        users = new ClientList();
        //UnconfirmedUsers = new UserCollection();
        textMessages = new List<TextMessage>();
        // create list of documents
        listOfDocuments = new List<Document>();

        //Creates random server password.
        serverPassword = randomString(10);
        Console.WriteLine(serverPassword);

        //Reads in users.
        string[] UserFiles;
        if (Directory.Exists(Client.pathToDir + Client.USER_LOCATION) == true)
        {
            UserFiles = Directory.GetFiles(Client.pathToDir + Client.USER_LOCATION);
            Client tempUser;
            foreach (string item in UserFiles)
            {
                tempUser = new Client(item);
                this.users.Add(tempUser);
            }
        }
        else
        {
            Directory.CreateDirectory(Client.pathToDir + Client.USER_LOCATION);
        }

        //Creates directory for files if it doesnt exist.
        if (!Directory.Exists(Document.pathToDir + Document.FILE_LOCATION))
        {
            Directory.CreateDirectory(Document.pathToDir + Document.FILE_LOCATION);
        }

        //Starts thread to keep track of inline users
        Thread thread = new Thread(new ThreadStart(setOnline));
        thread.Start();
    }
    #endregion

    //Login just checks if a user exist.
    public FailureType login(Client connectingUser)
    {
        //Finds user.
        int index;
        index = users.findIndexByName(connectingUser.Name);

        //Fails if user doesn't exist is already online or if passwords to match.
        if (index == -1)
        {
            return FailureType.FailedUserDoesNotExist;
        }
        else if (users.ElementAt(index).Online == true)
        {
            return FailureType.FailedUserAlreadyOnline;
        }
        else if (Client.checkPassword(users.ElementAt(index), connectingUser.Password) == true)
        {
            Console.WriteLine(connectingUser.Name + " did a login check.");
            return FailureType.Good;
        }
        else
        {
            return FailureType.FailedUnknownReason;
        }

    }

    //Adds user and sets them to unconfirmed.
    public FailureType requestAccount(Client requestingUser)
    {
        //Checks for valid input
        if (requestingUser == null || requestingUser.Name == "" || requestingUser.Password == "")
        {
            return FailureType.FailedInvalidInput;
        }

        // user is not confirmed just yet
        requestingUser.Confirmed = false;

        //returns false if the username already exist
        Console.WriteLine("Adding user: " + requestingUser.Name);
        return users.AddNewUser(requestingUser);
    }

    //Gets a list of users online.
    public List<string> getUsers()
    {
        List<string> temp = new List<string>();

        foreach (Client item in users)
        {
            if (item.Online == true)
            {
                temp.Add(item.Name);
            }
        }


        return temp;
    }

    //Gets a list of unconfirmed useres.
    public List<string> getUnconfirmedUsers()
    {
        List<string> tempUserList = new List<string>();

        foreach (Client user in users)
        {
            if (user.Confirmed == false)
            {
                tempUserList.Add(user.Name);
            }
        }

        return tempUserList;
    }

    //Changes user's password.
    public virtual FailureType changePassword(Client connectingUser, string targetName, string password)
    {
        return users.UserChangePassword(connectingUser, targetName, password);
    }

    //Confirms a user.
    public FailureType confirmUsers(string userToConfirm, Client connectingUser)
    {
        //Checks if user is admin
        if(users.UserIsAdmin(connectingUser) == FailureType.FailedUserNotAdmin)
        {
            return FailureType.FailedUserNotAdmin;
        }
        //Confirms user
        foreach (Client currUser in users)
        {
            if (currUser.Name == userToConfirm)
            {
                currUser.Confirmed = true;
                currUser.writeToFile();
            }
        }
        Console.WriteLine("Confirmed: " + userToConfirm);
        return FailureType.Good;
    }

    //Sets a user to an admin
    public virtual FailureType setAdmin(Client user, string targetName, bool adminValue)
    {
        //refers to the same function in the User Collection class
        return users.setAdmin(user, targetName, adminValue);
    }

    //Allows a direct connection to server without having an admin on server.
    public virtual FailureType setAdminDirect(string user, string serverPassword)
    {
        //this has to be done manually since there is no admin invoking this call
        if (this.serverPassword == serverPassword)
        {
            int index = users.findIndexByName(user);

            if (index == -1)
            {
                return FailureType.FailedUserDoesNotExist;
            }

            users.ElementAt(index).Admin = true;
            users.ElementAt(index).Confirmed = true;
            users.ElementAt(index).writeToFile();
            Console.WriteLine(user + " is now an admin");
            return FailureType.Good;
        }
        else
        {
            return FailureType.FailedBadPassword;
        }
    }

    //Checks if user is an admin.
    public bool amIAdmin(string name)
    {
        foreach (Client currUser in users)
        {
            if (currUser.Name == name && currUser.Admin == true)
            {
                return true;
            }
        }

        return false;
    }

    //Allows the user to send text message.
    public FailureType sendText(TextMessage sentMessage)
    {
        sentMessage.Date = DateTime.UtcNow;
        textMessages.Add(sentMessage);
        return FailureType.Good;
    }

    //Gets text for user.
    public Queue<TextMessage> getTexts(Client myInfo)
    {
        Queue<TextMessage> tempText = new Queue<TextMessage>();
        int index = users.findIndexByName(myInfo.Name);
        Client temp = users.ElementAt(index);

        if (textMessages.Count != 0)
        {
            foreach (TextMessage item in textMessages)
            {
                if (temp.lastText < item.Date)
                {
                    tempText.Enqueue(item);
                }
            }
            users.ElementAt(index).lastText = textMessages.Last().Date;
            users.ElementAt(index).lastRequest = new DateTime(DateTime.UtcNow.Ticks);
        }
        return tempText;
    }

    //Opens file for user.
    public Document openFile(string nameOfFile, Client user)
    {
        //Finds file.
        int index;
        index = users.findIndexByName(user.Name);
        Document File;

        if (Client.checkPassword(users.ElementAt(index), user.Password))
        {

            File = listOfDocuments.Find(x => x.FileName == nameOfFile);
            if (File == null)
            {
                File = new Document(nameOfFile);
                listOfDocuments.Add(File);
            }
            return File;
        }
        else
        {
            return null;
        }

    }

    //Creates a new file for user.
    public string newFile(string nameOfFile, Client user)
    {
        int index;
        index = users.findIndexByName(user.Name);
        Document File;

        if (Client.checkPassword(users.ElementAt(index), user.Password))
        {
            File = new Document(nameOfFile);
            listOfDocuments.Add(File);
            return File.FileContents;
        }
        else
        {
            return null;
        }
    }

    //Reqyest Lock for a file.
    public FailureType requestLock(string nameOfFile, Client user)
    {

        if (!listOfDocuments.Exists(x => x.FileName == nameOfFile))
        {
            return FailureType.FailedFileNotFound;
        }
        else
        {
            if ((listOfDocuments.Find(x => x.FileName == nameOfFile).LockedUser == null))
            {

                int index = users.findIndexByName(user.Name);
                if (index == -1)
                {
                    return FailureType.FailedUserDoesNotExist;
                }
                else if (Client.checkPassword(users.ElementAt(index), user.Password) == false)
                {
                    return FailureType.FailedBadPassword;
                }

                listOfDocuments.Find(x => x.FileName == nameOfFile).LockedUser = users.ElementAt(index);
                sendText(new TextMessage(users.ElementAt(index), "<I now have control of " + nameOfFile + ">\n"));

                return FailureType.Good;
            }
            else
            {
                return FailureType.FailedLockNotRelease;
            }
        }
    }

    //Releases lock for a file.
    public FailureType releaseLock(string nameOfFile, Client user)
    {
        if (!listOfDocuments.Exists(x => x.FileName == nameOfFile))
        {
            return FailureType.FailedFileNotFound;
        }
        else
        {
            int index = users.findIndexByName(user.Name);
            if (index == -1)
            {
                return FailureType.FailedUserDoesNotExist;
            }

            if ((listOfDocuments.Find(x => x.FileName == nameOfFile).LockedUser == users.ElementAt(index)))
            {
                if (Client.checkPassword(users.ElementAt(index), user.Password) == false)
                {
                    return FailureType.FailedBadPassword;
                }

                listOfDocuments.Find(x => x.FileName == nameOfFile).LockedUser = null;
                sendText(new TextMessage(users.ElementAt(index), "<I no longer have control of " + nameOfFile + ">\n"));
                return FailureType.Good;
            }
            else
            {
                return FailureType.FailedLockNotRelease;
            }
        }
    }

    //Gets document changes.
    public Dictionary<int, List<Patch>> getDocChanges(string nameOfFile, Client user, int lastPatch)
    {
        if (listOfDocuments.Find(x => x.FileName == nameOfFile) != null)
        {
            int index;
            index = users.findIndexByName(user.Name);

            if (Client.checkPassword(users.ElementAt(index), user.Password))
            {
                return listOfDocuments.Find(x => x.FileName == nameOfFile).getChanges(lastPatch);
            }

        }

        return new Dictionary<int, List<Patch>>();


    }

    //Sends Changes to server.
    public FailureType sendDocChanges(string nameOfFile, List<Patch> sentPatches, Client user)
    {
        if (listOfDocuments.Find(x => x.FileName == nameOfFile) == null)
        {
            return FailureType.FailedFileNotFound;
        }

        int index;
        index = users.findIndexByName(user.Name);

        if (Client.checkPassword(users.ElementAt(index), user.Password))
        {
            if (listOfDocuments.Find(x => x.FileName == nameOfFile).LockedUser == users.ElementAt(index))
            {
                return listOfDocuments.Find(x => x.FileName == nameOfFile).changeDocument(sentPatches);
            }
            else
            {
                return FailureType.FailedInvalidUser;
            }
        }
        else
        {
            return FailureType.FailedBadPassword;
        }

    }

    //Gets a list of files.
    public List<string> getFiles(Client user)
    {
        List<string> Files = new List<string>();
        if (Directory.Exists(Document.pathToDir + Document.FILE_LOCATION) == true)
        {
            foreach (string item in Directory.GetFiles(Document.pathToDir + Document.FILE_LOCATION))
            {
                Files.Add(item.Remove(0, Document.pathToDir.Length + Document.FILE_LOCATION.Length));
            }
            return Files;
        }
        else
        {
            Directory.CreateDirectory(Client.pathToDir + Client.USER_LOCATION);
            return Files;
        }

    }

    //Get a Files.
    public FailureType getFile(Client myUser, string targetFile)
    {
        throw new NotImplementedException();
    }

    //Saves a file.
    public FailureType saveFile(string nameOfFile, Client user)
    {
        if (!listOfDocuments.Exists(x => x.FileName == nameOfFile))
        {
            return FailureType.FailedFileNotFound;
        }
        else
        {
            int index = users.findIndexByName(user.Name);
            if (index == -1)
            {
                return FailureType.FailedUserDoesNotExist;
            }
            else if (Client.checkPassword(users.ElementAt(index), user.Password) == false)
            {
                return FailureType.FailedBadPassword;
            }

            listOfDocuments.Find(x => x.FileName == nameOfFile).saveDocument();
        }

        return FailureType.Good;
    }

    //Gets the last patch from a files dictionary.
    public int getLastPatchID(string nameOfFile)
    {
        if (listOfDocuments.Find(x => x.FileName == nameOfFile) != null)
        {
            return listOfDocuments.Find(x => x.FileName == nameOfFile).getLastPatchID();
        }
        else
        {
            return 0;
        }
    }
    
    //Generates a random password.
    public string randomString(int size)
    {
        string ranString = "";

        byte[] testarray = new Byte[size];

        RNGCryptoServiceProvider randomVals = new RNGCryptoServiceProvider();

        randomVals.GetBytes(testarray);

        foreach (byte x in testarray)
        {
            ranString += (char)(x % 92 + 32);
        }

        return ranString;
    }

    //Sets a user online.
    public void setOnline()
    {
        TextMessage welcomeMessage = new TextMessage(new Client("Server", ""), "Welcome to the Server!\n");
        welcomeMessage.Date = DateTime.UtcNow;
        this.sendText(welcomeMessage);
        while (true)
        {
            Thread.Sleep(2500);
            foreach (Client item in users)
            {
                item.Online = false;
                if (DateTime.UtcNow.CompareTo(item.lastRequest.AddMilliseconds(1500)) < 0)
                {
                    item.Online = true;
                }

                foreach (Document doc in listOfDocuments)
                {
                    if (doc.LockedUser == item && item.Online == false)
                    {
                        doc.LockedUser = null;
                    }
                }
            }
        }
    }


    #region Entry Point for the main program. This section of code starts the main endpoint and starts the wcf service.
    public static string ADDRESS = "net.tcp://localhost:35442/main";
    static void Main(string[] args)
    {
        //Creating the binding use windows security
        NetTcpBinding netBinding = new NetTcpBinding();
        netBinding.Security.Mode = SecurityMode.None;

        //The Server Needs a Uri to start
        Console.WriteLine("Starting Server...");
        Uri address = new Uri(ADDRESS);

        //Starts the Service
        ServiceHost mainHost = new ServiceHost(typeof(Server));
        mainHost.AddServiceEndpoint(typeof(IServer), netBinding, address);

        //Opens the service
        mainHost.Open();
        Console.WriteLine("Server Running... ", mainHost.ToString());
        Console.WriteLine("Press 'Q' to quit");

        //Waits for command to quit
        while (Console.ReadKey(true).Key != ConsoleKey.Q) { }

        //Closes the service before closing the program
        mainHost.Close();

    }
    #endregion
}


