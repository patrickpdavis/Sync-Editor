using DiffMatchPatch;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Tests
{
    [TestClass()]
    public class DocumentTests
    {
        [TestMethod()]
        public void DocumentTest()
        {
            //creates object and makes sure it works
            string name = "TestName0.txt", contents = "Here is the contents of the file.";
            Document testDoc = new Document(name, contents);

            Assert.AreEqual(testDoc.FileName, name);
            Assert.AreEqual(testDoc.FileContents, contents);
        }

        [TestMethod()]
        public void DocumentTest1()
        {
            //creates and writes document
            //then reads contents into new document object
            Directory.CreateDirectory(Document.pathToDir + Document.FILE_LOCATION);

            string name = "TestName.txt", contents = "Here is the contents of the file.";
            Document testDoc = new Document(name);
            testDoc.FileContents = contents;
            testDoc.saveDocument();

            Document testDoc2 = new Document(name);

            Assert.AreEqual(testDoc.FileName, name);
            Assert.AreEqual(testDoc.FileContents, contents);


            Assert.AreEqual(testDoc2.FileName, name);
            Assert.AreEqual(testDoc2.FileContents, contents);
        }

        [TestMethod()]
        public void changeDocumentTest()
        {
            diff_match_patch testDIff = new diff_match_patch();

            string name = "TestName2.txt", contents = "Here is the contents of the file.";
            Document testDoc = new Document(name, contents);

            string text2 = "Here are the contents of the file.";
            List<Patch> testPatch = testDIff.patch_make(testDoc.FileContents, text2);

            testDoc.changeDocument(testPatch);
            testDoc.saveDocument();

            Assert.AreEqual(text2, testDoc.FileContents);
        }

        [TestMethod()]
        public void getChangesTest()
        {
            diff_match_patch testDiff = new diff_match_patch();

            string name = "TestName3.txt", contents = "Here is the contents of the file.";
            Document testDoc = new Document(name, contents);

            string text2 = "Here are the contents of the file.";
            List<Patch> testPatch = testDiff.patch_make(testDoc.FileContents, text2);

            testDoc.changeDocument(testPatch);

            Assert.AreEqual(testDiff.patch_toText(testPatch), testDiff.patch_toText(testDoc.getChanges(0).Values.Last()));
        }

        [TestMethod()]
        public void saveDocumentTest()
        {
            //already tested in changeDoc test
            changeDocumentTest();
        }

        [TestMethod()]
        public void getLastPatchIDTest()
        {
            diff_match_patch testDiff = new diff_match_patch();

            string name = "TestName.txt4", contents = "Here is the contents of the file.";
            Document testDoc = new Document(name, contents);

            string text2 = "Here are the contents of the file.";
            List<Patch> testPatch = testDiff.patch_make(testDoc.FileContents, text2);

            testDoc.changeDocument(testPatch);

            Assert.AreEqual(1,testDoc.getChanges(0).Keys.Last());
        }
    }
}