using System.IO;
using Coditate.Common.IO;
using Coditate.TestSupport;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Coditate.Build
{
    /// <summary>
    /// Tests for the XMapConfiguratorTask.
    /// </summary>
    /// <remarks>
    /// Most of the complex functionality is implemented and tested in the underlying XMap class, so these 
    /// tests are basic tests to ensure the whole thing is tied together correctly.
    /// </remarks>
    [TestFixture]
    public class XMapConfiguratorTaskTest
    {
        private static readonly FileInfo DefaultConfigFile =
            new FileInfo(@".\XMapConfiguratorTaskTest-Execute.defaultconfig");

        private static readonly FileInfo ConfigFileNoXMapping =
            new FileInfo(@".\XMapConfiguratorTaskTest-Execute_NoXMapping.config");

        private static readonly FileInfo ConfigFileWithXMapping =
            new FileInfo(@".\XMapConfiguratorTaskTest-Execute_WithXMapping.config");

        private static readonly FileInfo XMapPropertyFile = new FileInfo(@".\XMapConfiguratorTaskTest-Properties.xprop");

        private static readonly FileInfo XMapFile = new FileInfo(@".\XMapConfiguratorTaskTest-Execute.xmap");
        private FileInfo workingDefaultConfigFile, workingConfigFile;

        private XMapConfiguratorTask task;
        private IBuildEngine2 engine;
        private string tempPath;

        [SetUp]
        public void SetUp()
        {
            tempPath = Path.Combine(Path.GetTempPath(), typeof (XMapConfiguratorTaskTest).Name);
            IOUtils.Delete(new DirectoryInfo(tempPath), true);
            Directory.CreateDirectory(tempPath);
            workingDefaultConfigFile = DefaultConfigFile.CopyTo(Path.Combine(tempPath, DefaultConfigFile.Name));
            workingConfigFile = new FileInfo(Path.ChangeExtension(workingDefaultConfigFile.FullName, ".config"));

            task = new XMapConfiguratorTask();

            engine = MockRepository.GenerateStub<IBuildEngine2>();
            engine.Expect(e => e.ProjectFileOfTaskNode).Return(@".\XMapConfiguratorTaskTest.proj");

            task = new XMapConfiguratorTask();
            task.BuildEngine = engine;
            task.XMapping = Path.GetFileNameWithoutExtension(XMapFile.Name);
            task.DefaultConfigFiles = new ITaskItem[]
                {
                    new TaskItem(workingDefaultConfigFile.FullName)
                };
        }

        [TearDown]
        public void TearDown()
        {
            IOUtils.Delete(new DirectoryInfo(tempPath), true);
        }

        /// <summary>
        /// Verifies basic execution and output with an Xmapping file.
        /// </summary>
        [Test]
        public void Execute_WithXMapping()
        {
            // put the xmapping where it can be picked up by the task
            XMapFile.CopyTo(Path.Combine(tempPath, XMapFile.Name));

            task.Execute();

            using (
                TextReader first = ConfigFileWithXMapping.OpenText(),
                           second = new StreamReader(workingConfigFile.FullName))
            {
                AssertEx.AreEqual(first, second);
            }
        }


        /// <summary>
        /// Verfies execution and output with no actual xmapping file defined.
        /// </summary>
        [Test]
        public void Execute_NoXMapping()
        {
            task.Execute();

            using (
                TextReader first = ConfigFileNoXMapping.OpenText(),
                           second = new StreamReader(workingConfigFile.FullName))
            {
                AssertEx.AreEqual(first, second);
            }
        }
    }
}