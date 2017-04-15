using NUnit.Framework;
using System;
using System.Reflection;

namespace Monkeyspeak.Tests
{
    [TestFixture()]
    public class MonkeyspeakEngineTests
    {
        #region Private Fields

        private const string testScript = @"
*This is a comment
(0:0) when the script is started,
        (5:100) set %hello to {Hello World}.
        (5:102) print {%hello} to the console.
";

        #endregion Private Fields

        #region Public Methods

        public bool HandleAllCauses(Monkeyspeak.TriggerReader reader)
        {
            return true;
        }

        [TestCase("test.msx", testScript)]
        public void PageCompileToFile_DoesExist(string FileName, string TestScript)
        {
            Monkeyspeak.MonkeyspeakEngine engine = new Monkeyspeak.MonkeyspeakEngine();
            engine.Options.TriggerLimit = int.MaxValue;

            var bigScript = TestScript;
            for (int i = 0; i <= 6000; i++)
            {
                bigScript += TestScript + "\n";
            }
            Monkeyspeak.Page oldPage = engine.LoadFromString(bigScript);

            Console.WriteLine("Page Trigger Count: " + oldPage.Size);

            oldPage.CompileToFile(FileName);
            FileAssert.Exists(FileName);
        }

        [TestCase("", testScript)]
        [TestCase("test.msx", ""),]
        public void PageCompileTpoFile_ThrowsInValidArgument(string FileName, string Script)
        {
            //Assert.Throws(typeof(ArgumentException),
            //   new TestDelegate(MethodThatThrows));

            //Assert.Throws<ArgumentException>(
            //  new TestDelegate(MethodThatThrows));

            ArgumentException ex = Assert.Throws<ArgumentException>(
      delegate { PageCompileToFile_DoesExist(FileName, Script); });
            Assert.That(ex.Message, Is.EqualTo("filePath cannot bw null or empty"));

            //Assert.Throws(typeof(ArgumentException),
            //  delegate { throw new ArgumentException(); });

            //Assert.Throws<ArgumentException>(
            //  delegate { throw new ArgumentException(); });
        }

        [TestCase("")]
        [TestCase("test.msx")]
        public void TestLoadCompiledFile(string FileName)
        {
            Monkeyspeak.MonkeyspeakEngine engine = new Monkeyspeak.MonkeyspeakEngine();
            engine.Options.TriggerLimit = int.MaxValue;

            Monkeyspeak.Page page = engine.LoadCompiledFile(FileName);

            page.LoadSysLibrary();
            page.LoadIOLibrary();
            page.LoadMathLibrary();
            page.LoadTimerLibrary();

            page.Execute(0);
            Console.WriteLine("Page Trigger Count: " + page.Size);
        }

        [Test]
        public void TestReflectionLoader()
        {
            MonkeyspeakEngine engine = new MonkeyspeakEngine();
            Page page = engine.LoadFromString(@"
(0:1000)
(5:1000) {Hello Reflection Test}
(5:1001) {Hello Reflection Test 2}
");
            page.LoadLibraryFromAssembly(Assembly.GetExecutingAssembly().Location);

            page.Execute(1000);
        }

        #endregion Public Methods

        #region Private Methods

        private void DebugAllErrors(Monkeyspeak.Trigger trigger, Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Error with " + trigger.ToString());
#if DEBUG
            throw ex;
#endif
        }

        #endregion Private Methods

        #region Private Classes

        private class TestReflection
        {
            #region Public Methods

            [Monkeyspeak.TriggerHandler(TriggerCategory.Cause, 1000, "(0:1000) TestTriggerHandlerMethod")]
            public bool TestTriggerHandlerCauseMethod(TriggerReader reader)
            {
                return true;
            }

            [Monkeyspeak.TriggerHandler(TriggerCategory.Effect, 1001, "(5:1001) test print 2 {...}")]
            [Monkeyspeak.TriggerHandler(TriggerCategory.Effect, 1000, "(5:1000) test print {...}")]
            public bool TestTriggerHandlerEffectMethod(TriggerReader reader)
            {
                if (reader.PeekString()) Console.WriteLine(reader.ReadString());
                else Console.WriteLine("Error!!!... no value.");
                return true;
            }

            #endregion Public Methods
        }

        #endregion Private Classes

        #region Public Methods

        [Test()]
        public void CompileToStreamTest()
        {
            throw new NotImplementedException();
        }

        [Test()]
        public void LoadCompiledStreamTest()
        {
            throw new NotImplementedException();
        }

        [Test()]
        public void LoadFromStreamTest()
        {
            throw new NotImplementedException();
        }

        [Test()]
        public void LoadFromStreamTest1()
        {
            throw new NotImplementedException();
        }

        [Test()]
        public void LoadFromStringTest()
        {
            throw new NotImplementedException();
        }

        [Test()]
        public void LoadFromStringTest1()
        {
            throw new NotImplementedException();
        }

        [Test()]
        public void MonkeyspeakEngineTest()
        {
            throw new NotImplementedException();
        }

        [Test()]
        public void MonkeyspeakEngineTest1()
        {
            throw new NotImplementedException();
        }

        #endregion Public Methods
    }
}