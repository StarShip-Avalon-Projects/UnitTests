using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyspeakTest
{
    [TestFixture]
    public class UnitTest1
    {
        #region Private Fields

        private string testScript = @"
*This is a comment
(0:0) when the script is started,
        (5:100) set %hello to {Hello World}.
        (5:10000) create a debug breakpoint here,
        (5:102) print {%hello} to the console.
        (5:102) print {%testVariable} to the console.

(0:0) when the script is started,
        (5:102) print {This is a test script.} to the console.
";

        #endregion Private Fields

        #region Public Methods

        [Test, ExpectedException(typeof(Monkeyspeak.MonkeyspeakException))]
        public void DebugTest()
        {
            Monkeyspeak.MonkeyspeakEngine engine = GetMonkeySpeakEngine();
            Monkeyspeak.Page page = engine.LoadFromString(testScript);

            page.Error += DebugAllErrors;

            List<Monkeyspeak.Trigger> triggers = new List<Monkeyspeak.Trigger>();

            page.LoadSysLibrary();
            page.LoadDebugLibrary();

            Monkeyspeak.IVariable var = page.SetVariable("%testVariable", "Hello WOrld", true);

            page.SetTriggerHandler(Monkeyspeak.TriggerCategory.Cause, 0, HandleAllCauses);

            // Trigger count created by subscribing to TriggerAdded event
            // and putting triggers into a list.
            Console.WriteLine("Trigger Count: " + page.Size);

            page.ExecuteAsync(0);
        }

        [Test]
        public void DemoTest()
        {
            Monkeyspeak.MonkeyspeakEngine engine = GetMonkeySpeakEngine();
            Monkeyspeak.Page page = engine.LoadFromString(testScript);

            page.Error += DebugAllErrors;

            List<Monkeyspeak.Trigger> triggers = new List<Monkeyspeak.Trigger>();

            page.LoadSysLibrary();
            Monkeyspeak.IVariable var = page.SetVariable("%testVariable", "Hello WOrld", true);

            page.SetTriggerHandler(Monkeyspeak.TriggerCategory.Cause, 0, HandleAllCauses);

            // Trigger count created by subscribing to TriggerAdded event
            // and putting triggers into a list.
            Console.WriteLine("Trigger Count: " + page.Size);

            page.ExecuteAsync(0);
        }

        [Test]
        public void DurabilityParseTest()
        {
            Monkeyspeak.MonkeyspeakEngine engine = GetMonkeySpeakEngine();

            // Set the trigger limit to int.MaxValue to prevent TriggerLimit
            // reached exceptions
            engine.Options.TriggerLimit = int.MaxValue;

            var bigScript = testScript;
            for (int i = 0; i <= 6000; i++)
            {
                bigScript += testScript + "\n";
            }
            Monkeyspeak.Page page = engine.LoadFromString(bigScript);

            page.Error += DebugAllErrors;

            page.LoadSysLibrary();

            page.SetTriggerHandler(Monkeyspeak.TriggerCategory.Cause, 0, HandleAllCauses);

            Console.WriteLine("Page Trigger Count: " + page.Size);
            //page.Execute(Monkeyspeak.TriggerCategory.Cause, 0);
        }

        [Test, ExpectedException(typeof(Monkeyspeak.MonkeyspeakException))]
        public async Task ErrorTriggerTest()
        {
            var errorTestScript = @"
(0:0) when the script starts,
        (5:102) print {This is a test of the new error system} to the console.
        (5:105) raise an error.
        (5:102) print {This will NOT be displayed because an error was raised} to the console.
";
            Monkeyspeak.MonkeyspeakEngine engine = GetMonkeySpeakEngine();
            Monkeyspeak.Page page = engine.LoadFromString(errorTestScript);

            page.Error += DebugAllErrors;

            page.LoadSysLibrary();

            //Throws MonkeySpeak.Exception
            await Task.Run(() => page.ExecuteAsync(0));
        }

        [Test()]
        public void TestParallelExecute()
        {
            var ioTestString = @"
(0:0) when the script starts,
    (5:100) set variable %file to {test.txt}.
    (1:200) and the file {%file} exist,
        (5:202) delete file {%file}.
        (5:203) create file {%file}.

(0:0) when the script starts,
        (5:102) print {%file} to the console.
        (5:150) take variable %increment and add 1 to it.
        (5:102) print {Execution increment %increment} to the console.

(0:0) when the script starts,
    (1:200) and the file {%file} exists,
    (1:203) and the file {%file} can be written to,
        (5:200) append {Hello World from Monkeyspeak %VERSION!} to file {%file}.

(0:0) when the script starts,
    (5:150) take variable %test and add 2 to it.
    (5:102) print {%test} to the console.
";

            var engine = new Monkeyspeak.MonkeyspeakEngine
            {
                Options = { TriggerLimit = int.MaxValue }
            };

            var page = engine.LoadFromString(ioTestString);

            page.LoadSysLibrary();
            page.LoadIOLibrary();
            page.LoadStringLibrary();
            page.LoadMathLibrary();
            page.LoadTimerLibrary();

            var tasks = new Task[100];
            for (int i = 0; i <= tasks.Length - 1; i++)
                tasks[i] = Task.Run(async () => await page.ExecuteAsync(0));

            Console.WriteLine("Page Trigger Count: " + page.Size);
            Task.WaitAll(tasks);
            // Result is execution is parallel! Awesome!
        }

        [Test]
        public void GetTriggerDescriptionsTest()
        {
            Monkeyspeak.MonkeyspeakEngine engine = GetMonkeySpeakEngine();
            Monkeyspeak.Page page = engine.LoadFromString("");

            page.Error += DebugAllErrors;

            page.LoadSysLibrary();
            page.LoadIOLibrary();
            page.LoadMathLibrary();
            page.LoadTimerLibrary();

            foreach (string desc in page.GetTriggerDescriptions())
            {
                Console.WriteLine(desc);
            }
        }

        public bool HandleAllCauses(Monkeyspeak.TriggerReader reader)
        {
            return true;
        }

        public bool AlwayFalse(Monkeyspeak.TriggerReader reader)
        {
            return false;
        }

        [Test]
        public void IOLibraryTest()
        {
            var ioTestString = @"
(0:0) when the script starts,
    (5:100) set variable %file to {test.txt}.
    (5:102) print {%file} to the console.

(0:0) when the script starts,
    (1:200) and the file {%file} exist,
        (5:202) delete file {%file}.
        (5:203) create file {%file}.

(0:0) when the script starts,
    (1:200) and the file {%file} exists,
    (1:203) and the file {%file} can be written to,
        (5:200) append {Hello World from Monkeyspeak!} to file {%file}.

(0:0) when the script starts,
    (5:150) take variable %test and add 2 to it.
    (5:102) print {%test} to the console.
";
            Monkeyspeak.MonkeyspeakEngine engine = GetMonkeySpeakEngine();
            Monkeyspeak.Page page = engine.LoadFromString(ioTestString);

            page.Error += DebugAllErrors;

            page.LoadSysLibrary();
            page.LoadIOLibrary();

            page.SetTriggerHandler(Monkeyspeak.TriggerCategory.Cause, 0, HandleAllCauses);

            page.Execute(0);
        }

        [Test]
        public void SetGetVariableTest()
        {
            Monkeyspeak.MonkeyspeakEngine engine = GetMonkeySpeakEngine();
            Monkeyspeak.Page page = engine.LoadFromString(testScript);

            page.Error += DebugAllErrors;

            for (int i = 0; i <= 10; i++)
            {
                page.SetVariable("Name", true.ToString(), false);
            }

            foreach (var variable in page.Scope)
            {
                System.Diagnostics.Debug.WriteLine(variable);
            }
        }

        [Test()]
        public async Task TextCondkitionBlockageAsync()
        {
            var timerLibTestScript = @"
(0:0) when the script starts,
    (5:102) print {Before False} to the console.
(1:6666) return false,
    (5:102) print {After False} to the console.

(0:300) when timer %timer goes off,
    (5:102) print {Timer %timer went off.} to the console.
";
            Monkeyspeak.MonkeyspeakEngine engine = GetMonkeySpeakEngine();
            Monkeyspeak.Page page = engine.LoadFromString(timerLibTestScript);

            page.Error += DebugAllErrors;

            page.LoadSysLibrary();
            page.LoadTimerLibrary();
            page.SetTriggerHandler(Monkeyspeak.TriggerCategory.Condition, 6666, AlwayFalse);
            page.SetTriggerHandler(Monkeyspeak.TriggerCategory.Cause, 0, HandleAllCauses);
            int[] ids = { 0 };
            await Task.Run(() => page.ExecuteAsync(ids));
        }

        [Test]
        public void TimerLibraryTest()
        {
            var timerLibTestScript = @"
(0:0) when the script starts,
    (5:101) set variable %timer to 1.
    (5:300) create timer %timer to go off every 2 second(s).

(0:300) when timer %timer goes off,
    (5:102) print {Timer %timer went off.} to the console.
";
            Monkeyspeak.MonkeyspeakEngine engine = GetMonkeySpeakEngine();
            Monkeyspeak.Page page = engine.LoadFromString(timerLibTestScript);

            page.Error += DebugAllErrors;

            page.LoadSysLibrary();
            page.LoadTimerLibrary();

            page.SetTriggerHandler(Monkeyspeak.TriggerCategory.Cause, 0, HandleAllCauses);

            page.Execute(0);
            System.Threading.Thread.Sleep(4000);
        }

        private Monkeyspeak.MonkeyspeakEngine GetMonkeySpeakEngine()
        {
            return new Monkeyspeak.MonkeyspeakEngine();
        }

        #endregion Public Methods

        #region Private Methods

        private void DebugAllErrors(Monkeyspeak.TriggerHandler handler, Monkeyspeak.Trigger trigger, Exception ex)
        {
            Console.WriteLine("Error with " + trigger.ToString());
#if DEBUG
            System.Diagnostics.Debug.WriteLine(ex.ToString());
#endif
        }

        #endregion Private Methods
    }
}