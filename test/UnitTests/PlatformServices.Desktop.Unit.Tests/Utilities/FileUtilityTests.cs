// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MSTestAdapter.PlatformServices.Desktop.UnitTests.Utilities
{
    extern alias FrameworkV1;

    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestPlatform.MSTestAdapter.PlatformServices.Utilities;
    using Moq;
    using Assert = FrameworkV1::Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
    using CollectionAssert = FrameworkV1::Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert;
    using TestClass = FrameworkV1::Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
    using TestInitialize = FrameworkV1::Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
    using TestMethod = FrameworkV1::Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;

    [TestClass]
    public class FileUtilityTests
    {
        private Mock<FileUtility> fileUtility;

        [TestInitialize]
        public void TestInit()
        {
            this.fileUtility = new Mock<FileUtility>();
            this.fileUtility.CallBase = true;
        }

        [TestMethod]
        public void ReplaceInvalidFileNameCharactersShouldReturnFileNameIfItHasNoInvalidChars()
        {
            var fileName = "galaxy";
            Assert.AreEqual(fileName, this.fileUtility.Object.ReplaceInvalidFileNameCharacters(fileName));
        }

        [TestMethod]
        public void ReplaceInvalidFileNameCharactersShouldReplaceInvalidChars()
        {
            var fileName = "galaxy<>far:far?away";
            Assert.AreEqual("galaxy__far_far_away", this.fileUtility.Object.ReplaceInvalidFileNameCharacters(fileName));
        }

        #region AddFilesFromDirectory tests

        [TestMethod]
        public void AddFilesInADirectoryShouldReturnAllTopLevelFilesInADirectory()
        {
            var topLevelFiles = new string[] { "tick.txt", "tock.tick.txt" };

            this.fileUtility.Setup(fu => fu.GetFilesInADirectory(It.IsAny<string>())).Returns(topLevelFiles);
            this.fileUtility.Setup(fu => fu.GetDirectoriesInADirectory(It.IsAny<string>())).Returns(new string[] { });

            var files = this.fileUtility.Object.AddFilesFromDirectory("C:\\randomclock", false);

            CollectionAssert.AreEqual(topLevelFiles, files);
        }

        [TestMethod]
        public void AddFilesInADirectoryShouldReturnAllFilesUnderSubFolders()
        {
            var allFiles = new string[]
                               {
                                   "MainClock\\tickmain.txt", "MainClock\\tock.tick.txt",
                                   "MainClock\\Folder1\\tick.txt", "MainClock\\Folder1\\tock.tick.txt",
                                   "MainClock\\Folder2\\newtick.log", "MainClock\\Folder2\\newtock.log",
                                   "MainClock\\Folder2\\backup\\newtock.tick.txt",
                               };

            this.SetupMockFileAPIs(allFiles);

            var files = this.fileUtility.Object.AddFilesFromDirectory("MainClock", false);

            CollectionAssert.AreEqual(allFiles, files);
        }

        [TestMethod]
        public void AddFilesInADirectoryShouldReturnAllFilesUnderSubFoldersEvenIfAFolderIsEmpty()
        {
            var allFiles = new string[]
                               {
                                   "MainClock\\tickmain.txt", "MainClock\\tock.tick.txt",
                                   "MainClock\\Folder1\\tick.txt", "MainClock\\Folder1\\tock.tick.txt",
                                   "MainClock\\Folder2\\newtick.log", "MainClock\\Folder2\\newtock.log",
                                   "MainClock\\Folder2\\backup\\",
                               };

            this.SetupMockFileAPIs(allFiles);

            var files = this.fileUtility.Object.AddFilesFromDirectory("MainClock", false);

            var expectedFiles = new string[allFiles.Length - 1];
            Array.Copy(allFiles, 0, expectedFiles, 0, 6);

            CollectionAssert.AreEqual(expectedFiles, files);
        }

        private void SetupMockFileAPIs(string[] files)
        {
            this.fileUtility.Setup(fu => fu.GetFilesInADirectory(It.IsAny<string>())).Returns((string dp) =>
            {
                return
                    files.Where(f => f.Contains(dp) && f.LastIndexOf("\\") == (f.IndexOf(dp) + dp.Length) && !f.EndsWith("\\"))
                        .ToArray();
            });
            this.fileUtility.Setup(fu => fu.GetDirectoriesInADirectory(It.IsAny<string>())).Returns((string dp) =>
            {
                return
                    files.Where(f => f.Contains(dp) && f.LastIndexOf("\\") > (f.IndexOf(dp) + dp.Length))
                        .Select(f =>
                        {
                            var val = f.Substring(
                                f.IndexOf(dp) + dp.Length + 1,
                                f.Length - (f.IndexOf(dp) + dp.Length + 1));
                            return f.Substring(0, dp.Length + 1 + val.IndexOf("\\"));
                        })
                        .Distinct()
                        .ToArray();
            });
        }

        #endregion
    }
}
