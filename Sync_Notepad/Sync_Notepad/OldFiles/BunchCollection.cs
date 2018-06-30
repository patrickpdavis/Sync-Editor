// This file is not set to compile


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Sync_Notepad
{
    class BunchCollection : List<BunchoFiles>
    {
        // this is the pathname to the xml file that contains the bunch list and users allowed access
        static String pathToFile = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\bunch_list.xml";

        public BunchCollection()
        {

        }

        public remoteReturn writeBunchToFile()
        {
            try
            {
                var bunchCollectionWrite = 
                    new XElement("BunchCollection",
                           from currBunch in this
                           select new XElement("Filegroup",
                                      new XElement("name", currBunch.name),
                                      new XElement("ID", currBunch.ID),
                                        from user in currBunch.Users
                                        select new XElement("user", user.Name)
                                      ));

                bunchCollectionWrite.Save(pathToFile);

                return remoteReturn.Good;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return remoteReturn.FailedUnknownReason;
            }
        }

        public remoteReturn searchUserInBunchFile(string nameOfBunch, Users user)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(pathToFile);


            XmlElement root = doc.DocumentElement;
            XmlNode node = root.SelectSingleNode($"//Filegroups/{nameOfBunch}");
            XmlNodeList nodes = node.ChildNodes;

            foreach (XmlNode n in nodes)
            {
                if (n.InnerText.Equals(user.Name))
                {
                    // person is apart of bunch, return good
                    return remoteReturn.Good;
                }
            }

            // user isn't in bunch
            return remoteReturn.FailedUserNotInBunch;
        }

        public remoteReturn deleteUserFromFile(Users user)
        {

            XmlDocument doc = new XmlDocument();
            doc.Load(pathToFile);


            XmlElement root = doc.DocumentElement;
            XmlNode node = root.SelectSingleNode($"//Filegroups/{nameOfBunch}");
            XmlNodeList nodes = node.ChildNodes;

            foreach (XmlNode n in nodes)
            {
                if (n.InnerText.Equals(user.Name))
                {
                    n.ParentNode.RemoveChild(n);
                    return remoteReturn.Good;
                }
            }

            // user isn't in bunch
            return remoteReturn.FailedUserNotInBunch;

            
        }

        

    }
}
