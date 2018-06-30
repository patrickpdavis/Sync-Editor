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
using DiffMatchPatch;

[ServiceContract]
public interface IServer
{
    [OperationContract]
    FailureType login(Client connectingUser);
    [OperationContract]
    FailureType requestAccount(Client requestingUser);
    [OperationContract]
    List<string> getUsers();
    [OperationContract]
    List<string> getUnconfirmedUsers();
    [OperationContract]
    FailureType changePassword(Client connectingUser, string targetName, string password);
    //Moved filegroup option
    [OperationContract]
    FailureType confirmUsers(string userToConfirm, Client connectingUser);
    [OperationContract]
    FailureType setAdmin(Client user, string targetName, bool adminValue);
    [OperationContract]
    FailureType setAdminDirect(string user, string serverPassword);
    [OperationContract]
    bool amIAdmin(string name);
    [OperationContract]
    FailureType sendText(TextMessage sentMessage);
    [OperationContract]
    Queue<TextMessage> getTexts(Client myInfo);
    [OperationContract]
    Document openFile(string nameOfFile, Client user);
    [OperationContract]
    string newFile(string nameOfFile, Client user);
    [OperationContract]
    FailureType requestLock(string nameOfFile, Client user);
    [OperationContract]
    FailureType releaseLock(string nameOfFile, Client user);
    [OperationContract]
    Dictionary<int, List<Patch>> getDocChanges(string nameOfFile, Client user, int lastPatch);
    [OperationContract]
    FailureType sendDocChanges(string nameOfFile, List<Patch> sentPatches, Client user);
    [OperationContract]
    List<string> getFiles(Client user);
    [OperationContract]
    FailureType getFile(Client myUser, string targetFile);
    [OperationContract]
    FailureType saveFile(string nameOfFile, Client user);
    [OperationContract]
    int getLastPatchID(string nameOfFile);

}
