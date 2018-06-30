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
using System.Runtime.Serialization;


/// <summary>
/// This is a class for transferring messages over the network.
/// Most everything in this class is self explanatory.
/// </summary>

[DataContract]
public class TextMessage
{
    #region Data Members.
    [DataMember]
    private string message;
    [DataMember]
    private string user;
    [DataMember]
    private DateTime date;
    #endregion

    #region Properties
    public string Message
    {
        get
        {
            return message;
        }

        set
        {
            message = value;
        }
    }

    public string User
    {
        get
        {
            return user;
        }

        set
        {
            user = value;
        }
    }

    public DateTime Date
    {
        get
        {
            return date;
        }

        set
        {
            date = value;
        }
    }
    #endregion

    #region Constructors
    public TextMessage(Client MyInfo, string message)
    {
        this.User = MyInfo.Name;
        this.Message = message;
    }
    #endregion

}

