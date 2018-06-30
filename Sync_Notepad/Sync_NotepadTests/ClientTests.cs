using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass()]
    public class ClientTests
    {
        [TestMethod()]
        public void Client_ConstructorTest()
        {
            Client testUser = new Client("testUser", "5555");
            Assert.IsNotNull(testUser.Password);
            Assert.IsNotNull(testUser.Name);
            Assert.IsNotNull(testUser.Online);
            Assert.IsNotNull(testUser.lastRequest);
            Assert.IsNull(testUser.Salt);
        }

        [TestMethod()]
        public void Client_ConstructorTwoTest()
        {
            Client testUser = new Client("testUser");
            Assert.IsNull(testUser.Password);
            Assert.IsNull(testUser.Name);
            Assert.IsNotNull(testUser.Online);
            Assert.IsNotNull(testUser.lastRequest);
            Assert.IsNull(testUser.Salt);
        }

        [TestMethod()]
        public void Client_writeToFileTest()
        {

            Client testUser = new Client("testUser","5555");

            testUser.writeToFile();

            #region ReadClientObjectFromFile

                string pathToDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string USER_LOCATION = "\\users\\";
                string XML_EXT = ".xml";
                string writePath = pathToDir + USER_LOCATION + testUser.Name + XML_EXT;

            Client tempObj;

            //Reads from the xml file for the client.
            using (FileStream reader = new FileStream(writePath, FileMode.Open, FileAccess.Read))
            {
                DataContractSerializer ser = new DataContractSerializer(typeof(Client));
                tempObj = (Client)ser.ReadObject(reader);
            }

            Assert.AreEqual(testUser.Name, tempObj.Name);
            Assert.AreEqual(testUser.Online, tempObj.Online);
            Assert.AreEqual(testUser.Password, tempObj.Password);
                
            #endregion


        }

        [TestMethod]
        public void Client_checkPasswordTest()
        {
            Client tempUser = new Client("testUser", "5555");
            Client.hashPassword(tempUser);

            Assert.IsTrue(Client.checkPassword(tempUser, "5555"));
        }


    }
}