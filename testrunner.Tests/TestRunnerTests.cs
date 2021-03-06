﻿using System.IO;
using System.Reflection;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestRunner.Infrastructure;

namespace TestRunner.Tests
{

    [TestClass]
    public class TestRunnerTests
    {
        
        static string here = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        #if NET461
        const string frameworkMoniker = "net461";
        #elif NETCOREAPP2_0
        const string frameworkMoniker = "netcoreapp2.0";
        #else
        #error Unrecognised build framework
        #endif

        #if NET461
        const string testRunnerExe = "testrunner.exe";
        #elif NETCOREAPP2_0
        const string testRunnerExe = "testrunner.dll";
        #else
        #error Unrecognised build framework
        #endif

        static string testRunner = Path.GetFullPath(Path.Combine(
            here, "..", "..", "..", "..", "..",
            "testrunner",
            "bin", "Debug",
            frameworkMoniker,
            "publish",
            testRunnerExe));

        static string passTests = Path.GetFullPath(Path.Combine(
            here, "..", "..", "..", "..", "..",
            "testrunner.Tests.Pass",
            "bin", "Debug",
            frameworkMoniker,
            "publish",
            "testrunner.Tests.Pass.dll"));

        static string failTests = Path.GetFullPath(Path.Combine(
            here, "..", "..", "..", "..", "..",
            "testrunner.Tests.Fail",
            "bin", "Debug",
            frameworkMoniker,
            "publish",
            "testrunner.Tests.Fail.dll"));

        static string msTestTests = Path.GetFullPath(Path.Combine(
            here, "..", "..", "..", "..", "..",
            "testrunner.Tests.MSTest",
            "bin", "Debug",
            frameworkMoniker,
            "publish",
            "testrunner.Tests.MSTest.dll"));

        static string differentConfigTests = Path.GetFullPath(Path.Combine(
            here, "..", "..", "..", "..", "..",
            "testrunner.Tests.DifferentConfigValue",
            "bin", "Debug",
            frameworkMoniker,
            "publish",
            "testrunner.Tests.DifferentConfigValue.dll"));

        static string fakeDll = Path.GetFullPath(Path.Combine(
            here, "..", "..", "..", "..", "..",
            "testrunner.Tests.FakeDll",
            "FakeDll.dll"));

        static string referencedAssembly = Path.GetFullPath(Path.Combine(
            here, "..", "..", "..", "..", "..",
            "testrunner.Tests.ReferencedAssembly",
            "bin", "Debug",
            frameworkMoniker,
            "publish",
            "testrunner.Tests.ReferencedAssembly.dll"));


        static string Quote(params string[] arguments)
        {
            return string.Join(" ", arguments.Select(arg => "\"" + arg + "\""));
        }


        [TestMethod]
        public void Pass_Yields_ExitCode_0()
        {
            Assert.AreEqual(
                0,
                ProcessExtensions.ExecuteDotnet(testRunner, Quote(passTests)).ExitCode);
        }


        [TestMethod]
        public void Fail_Yields_ExitCode_1()
        {
            Assert.AreEqual(
                1,
                ProcessExtensions.ExecuteDotnet(testRunner, Quote(failTests)).ExitCode);
        }


        [TestMethod]
        public void TestCleanup_Receives_Failed_UnitTestResult_When_Test_Fails()
        {
            Assert.IsTrue(
                ProcessExtensions.ExecuteDotnet(testRunner, Quote(failTests)).Output
                    .Contains("Failed UnitTestOutcome"));
        }


        [TestMethod]
        public void Non_Existent_File_Yields_ExitCode_1()
        {
            Assert.AreEqual(
                1,
                ProcessExtensions.ExecuteDotnet(testRunner, Quote("no-such.dll")).ExitCode);
        }


        [TestMethod]
        public void MSTest_Suite_Passes()
        {
            var results = ProcessExtensions.ExecuteDotnet(testRunner, Quote(msTestTests));

            Assert.AreEqual(
                0, results.ExitCode,
                "TestRunner.exe returned non-zero exit code");

            Assert.IsTrue(
                results.Output.Contains(MSTest.MSTestTests.TestCleanupMessage),
                "[TestCleanup] method did not run");

            Assert.IsTrue(
                results.Output.Contains(MSTest.MSTestTests.ClassCleanupMessage),
                "[ClassCleanup] method did not run");

            Assert.IsTrue(
                results.Output.Contains(MSTest.MSTestTests.AssemblyCleanupMessage),
                "[AssemblyCleanup] method did not run");

            Assert.IsTrue(
                results.Output.Contains(MSTest.MSTestTests.TraceTestMessage),
                "System.Diagnostics.Trace test message was not printed");

            Assert.IsFalse(
                results.Output.Contains(MSTest.MSTestTests.IgnoredTestMessage),
                "An [Ignore]d test method ran");
        }


        [TestMethod]
        public void Many_Passing_Assemblies_Yields_ExitCode_0()
        {
            Assert.AreEqual(
                0,
                ProcessExtensions.ExecuteDotnet(testRunner, Quote(passTests, passTests)).ExitCode);
        }


        [TestMethod]
        public void One_Failing_Assembly_In_Many_Yields_ExitCode_1()
        {
            Assert.AreEqual(
                1,
                ProcessExtensions.ExecuteDotnet(testRunner, Quote(failTests, passTests)).ExitCode);
        }


        [TestMethod]
        public void Many_Assemblies_With_Different_Config_Files_Pass()
        {
            Assert.AreEqual(
                0,
                ProcessExtensions.ExecuteDotnet(testRunner, Quote(msTestTests, differentConfigTests)).ExitCode);
        }


        [TestMethod]
        public void Non_Test_Assembly_Yields_ExitCode_0()
        {
            Assert.AreEqual(
                0,
                ProcessExtensions.ExecuteDotnet(testRunner, Quote(referencedAssembly)).ExitCode);
        }


        [TestMethod]
        public void Non_DotNet_Dll_Yields_ExitCode_0()
        {
            Assert.AreEqual(
                0,
                ProcessExtensions.ExecuteDotnet(testRunner, Quote(fakeDll)).ExitCode);
        }

    }
}
