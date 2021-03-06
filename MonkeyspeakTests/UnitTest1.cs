﻿using NUnit.Framework;
using Monkeyspeak;
using Monkeyspeak.Libraries;
using Monkeyspeak.Utils;
using Monkeyspeak.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonkeyspeakTests
{
    //public class MyLibrary : BaseLibrary
    //{
    //    public override void Initialize(params object[] args)
    //    {
    //        // add trigger handlers here
    //        Add(TriggerCategory.Cause, 0, EntryPointForScript,
    //            "(0:0) entering script here,");
    //        Add(TriggerCategory.Effect, 0, FirstHandlerThatPrints,
    //            "(5:0) hello!");

    //        Add(TriggerCategory.Effect, 1, SecondHandlerTakesAString,
    //            "(5:1) write {...}.");
    //    }

    //    private bool EntryPointForScript(TriggerReader reader)
    //    {
    //        return true; // return false stops execution of any triggers below the one that called this method
    //    }

    //    public bool FirstHandlerThatPrints(TriggerReader reader)
    //    {
    //        Console.WriteLine("Hello!");
    //        return true; // return false stops execution of any triggers below the one that called this method
    //    }

    //    public bool SecondHandlerTakesAString(TriggerReader reader)
    //    {
    //        Console.WriteLine(reader.ReadString());
    //        return true; // return false stops execution of any triggers below the one that called this method
    //    }

    //    public override void Unload(Page page)
    //    {
    //        // this is called by page.Dispose() which is not called automatically
    //        // remove any unmanaged and disposable resources here or just due a ending action
    //    }
    //}

    [TestFixture]
    public class UnitTest1
    {
        private string testScript = @"
(0:0) when the script is started,
    (5:102) print {Welcome to %MONKEY} to the console.

(0:0) when the script is started,
    (5:102) print {None = %none} to the console.

(0:0) when the script is started,
    (1:666) and false,
        (5:102) print {better not be seen} to the console.

(0:0) when the script is started,
    (1:104) and variable %hello equals {this will be false move on to next condition}
		(5:100) set %hello to {Hello World}.
        (5:101) set %helloNum to 5.69.
        (5:102) print {hello = %hello helloNum = %helloNum} to the console.

(0:0) when the script is started,
        (5:100) set %hello to {Hello World}.
        (5:101) set %num to 5.1212E+003.
        (5:102) print {num = %num} to the console.
        (5:102) print {%hello} to the console.

(0:0) when the script is started,
    *Uncommented version
    (1:104) and variable %hello equals {this will be false move on to next condition}
        (5:102) print {the pen is blue!} to the console
    (1:104) and variable %hello equals {Hello World}
        (5:102) print {the pen is red!} to the console
        (5:102) print {hello = %hello helloNum = %helloNum} to the console
        (5:100) set %hello to {@%helloNum}
        (5:101) set %helloNum to 5.6969.
        (5:102) print {hello = %hello helloNum = %helloNum} to the console
        (5:115) call job 1 with %helloNum and {test arg}.
        (5:115) call job 2.

(0:0) when the script is started,
        (5:250) create a table as %myTable.
        (5:252) with table %myTable put {Hello World} in it at key {myKey}.
        (5:102) print {%myTable[myKey]} to the console.
        (6:250) for each entry in table %myTable put it into %entry,
            (5:102) print {%entry} to the console.

(0:0) when the script is started,
		(5:10000) create a debug breakpoint here,
        (5:100) set %testVariable to {Modified!}. -- try to modify constant variable
		(5:102) print {%testVariable} to the console.

(0:10000) when a debug breakpoint is hit,
		(5:102) print {Hit a breakpoint!} to the console.
        (5:105) raise an error. * dirty exit

(0:100) when job 1 is called put arguments into table %table,
    (5:102) print {job 1 executed} to the console
        (6:250) for each entry in table %table put it into %entry,
            (5:102) print {%entry} to the console.

(0:100) when job 2 is called,
    (5:102) print {job will not execute because infinite loop possibility} to the console
   (5:115) call job 1.
";

        public static string tableScript = @"
(0:0) when the script is started,
    (5:250) create a table as %myTable.
    (5:100) set %hello to {hi}
    (5:101) set %i to 0
    (5:252) with table %myTable put {%hello} in it at key {myKey1}.
    (5:252) with table %myTable put {%hello} in it at key {myKey2}.
    (5:252) with table %myTable put {%hello} in it at key {myKey3}.
    (5:252) with table %myTable put {%hello} in it at key {myKey4}.
    (5:252) with table %myTable put {%hello} in it at key {myKey5}.
    (5:252) with table %myTable put {%hello} in it at key {myKey6}.
    (5:252) with table %myTable put {%hello} in it at key {myKey7}.
    (5:252) with table %myTable put {123} in it at key {123}.
    (6:250) for each entry in table %myTable put it into %entry,
        (5:102) print {%entry} to the console.
        (5:102) print {%i} to the console.
    (6:454) after the loop is done,
        (5:150) take variable %myTable[123] and add 1 to it.
        (5:102) print {%myTable[123]} to the console.

(0:0) when the script is started,
    (5:101) set %answer to 0
    (5:101) set %life to 42
    (6:450) while variable %answer is not %life,
        (5:150) take variable %answer and add 1 to it.
        (1:102) and variable %answer equals 21,
            (5:450) exit the current loop.
    (6:454) after the loop is done,
        (5:102) print {We may never know the answer...} to the console.

(0:0) when the script is started,
        (5:102) print {%i} to the console.
        (5:102) print {%i} to the console.
        (5:102) print {%i} to the console.
        (5:102) print {%i} to the console.
        (5:102) print {%i} to the console.
        (5:102) print {%i} to the console.
        (5:102) print {%i} to the console.

(0:0) when the script is started,
    (5:250) create a table as %mytable
    (5:251) with table %mytable put 123 in it at key {123}.
    (6:250) for each entry in table %mytable put it into %entry,
        (5:102) print {%entry} to the console.
        (5:102) print {%entry} to the console.
        (5:102) print {%entry} to the console.
        (5:102) print {%entry} to the console.
    (6:450) while variable %myTable[123] is not 1234,
        (5:150) take variable %myTable[123] and add 1 to it.
    (6:454) after the loop is done,
        (5:102) print {%mytable[123]} to the console.
";

        public static string tableScriptMini = @"
(0:0)(5:250) %myTable(5:100) %hello {hi}(5:101) %i 0 (5:252) %myTable {%hello} {myKey1}.
(5:252) %myTable {%hello} {myKey2}(5:252) %myTable {%hello} {myKey3}(5:252) %myTable {%hello}{myKey4}(5:252) %myTable {%hello} {myKey5}.
(5:252) %myTable {%hello} {myKey6}(5:252) %myTable {%hello} {myKey7}(6:250) %myTable %entry
(5:102) {%entry}(5:150) %i 1(5:102) {%i}(6:454),(5:102) {I'm done!}(1:250) %myTable(5:101) %myTable[myKey1] 123
(5:102) {%myTable[myKey1]}

(0:0)(5:101) %answer 0(5:101)%life 42
(6:450) %answer %life(5:150) %answer 1(1:102) %answer 21(5:450)(6:454)(5:102) {%answer}(5:102) {We may never know the answer...}
";

        [SetUp]
        public void Initialize()
        {
            Logger.SingleThreaded = true;
        }

        [TearDown]
        public void Cleanup()
        {
        }

        [Test]
        public void AssemblyInfo()
        {
            var asm = Assembly.GetAssembly(typeof(MonkeyspeakEngine));
            Logger.Info(asm.FullName);
            Logger.Info(asm.ImageRuntimeVersion);
            Logger.Info(asm.GetName().Version);
            Logger.Info(int.MaxValue.ToString());
        }

        [Test]
        public void Tables()
        {
            for (int i = 0; i < 1; i++)
            {
                var engine = new MonkeyspeakEngine();
                engine.Options.Debug = false;

                Logger.Info(tableScript);
                Page page = engine.LoadFromString(tableScript); // replace with tableScriptMini to see results of that script

                page.Error += DebugAllErrors;
                page.AddTriggerHandler(TriggerCategory.Condition, 666, AlwaysFalseCond);
                page.LoadAllLibraries();
                //   page.RemoveLibrary<Monkeyspeak.Libraries.Debug>();
                page.SetVariable("%testVariable", "Hello WOrld", true);

                page.AddTriggerHandler(TriggerCategory.Cause, 0, HandleScriptStartCause);

                // Trigger count created by subscribing to TriggerAdded event and putting triggers into a list.
                Console.WriteLine("Trigger Count: " + page.Size);
                Logger.Assert(page.Size > 0, "Page size was 0 = FAIL!");
                page.Execute();
                foreach (var variable in page.Scope)
                {
                    Logger.Info($"{variable.ToString()} {variable.GetType().Name}");
                }
            }
        }

        [Test]
        public void LexerPrint()
        {
            //using (var stream = new FileStream("testBIG.ms", FileMode.OpenOrCreate))
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(tableScript)))
            using (Lexer lexer = new Lexer(new MonkeyspeakEngine(), new SStreamReader(stream)))
            {
                foreach (var token in lexer.Read())
                {
                    if (token.Type != TokenType.COMMENT)
                        Logger.Info($"{token} = {new string(lexer.Read(token.ValueStartPosition, token.Length))}");
                }
            }
        }

        [Test]
        public void LexerAndParserPrint()
        {
            var engine = new MonkeyspeakEngine();
            //engine.Options.Debug = true;
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(tableScript));
            using (Lexer lexer = new Lexer(engine, new SStreamReader(stream)))
            {
                Parser parser = new Parser(engine);
                //parser.VisitToken = VisitTokens;
                foreach (var triggerList in parser.Parse(lexer))
                {
                    Logger.Info($"New Block starting with {triggerList.()}");
                    // each triggerList instance is a new (0:###) block.
                    foreach (var trigger in triggerList)
                    {
                        // check trigger's out here.
                        StringBuilder sb = new StringBuilder();
                        sb.Append(trigger.ToString(true, true));
                        Logger.Info(sb.ToString());
                    }
                }
                parser.VisitToken = null;
            }
        }

        [Test]
        public void DurabilityParseString()
        {
            var engine = new MonkeyspeakEngine();

            // Set the trigger limit to int.MaxValue to prevent TriggerLimit reached exceptions
            engine.Options.TriggerLimit = int.MaxValue;

            var sb = new StringBuilder();
            for (int i = 0; i < 1; i++)
            {
                sb.AppendLine();
                sb.AppendLine("(0:0) when the script is started,");
                sb.AppendLine("(5:100) set %hello to {Hello World}.");
                sb.AppendLine("(1:104) and variable %hello equals {this will be false move on to next condition}");
                sb.AppendLine("(5:102) print {hello = %hello helloNum = % helloNum} to the console.");
                sb.AppendLine("(1:104) and variable %hello equals {Hello World}");
                sb.AppendLine("(5:101) set %helloNum to 5.");
                sb.AppendLine("(5:102) print {hello = %hello helloNum = %helloNum} to the console.");
                sb.AppendLine();
            }
            Logger.Info(sb.ToString());
            Page page = engine.LoadFromString(sb.ToString());
            page.LoadAllLibraries();
            page.RemoveLibrary<Monkeyspeak.Libraries.Debug>();

            var sb2 = new StringBuilder();
            for (int i = 0; i < 5; i++)
            {
                sb.AppendLine();
                sb.AppendLine("(0:0) when the script is started,");
                sb.AppendLine("(5:100) set %hello to {Hello World}.");
                sb.AppendLine("(1:104) and variable %hello equals {this will be false move on to next condition}");
                sb.AppendLine("(5:102) print {hello = %hello helloNum = % helloNum} to the console.");
                sb.AppendLine("(1:104) and variable %hello equals {Hello World}");
                sb.AppendLine("(5:101) set %helloNum to 5.");
                sb.AppendLine("(5:102) print {hello = %hello helloNum = % helloNum} to the console.");
                sb.AppendLine();
            }

            engine.LoadFromString(page, sb2.ToString());

            page.Error += DebugAllErrors;

            page.AddTriggerHandler(TriggerCategory.Cause, 0, HandleScriptStartCause);

            Logger.Info($"Triggers: {page.Size}");
            page.Execute();
        }

        [Test]
        public void DurabilityParseStringAsync()
        {
            var engine = new MonkeyspeakEngine();

            // Set the trigger limit to int.MaxValue to prevent TriggerLimit reached exceptions
            engine.Options.TriggerLimit = int.MaxValue;

            var sb = new StringBuilder();
            for (int i = 0; i < 10000; i++)
            {
                sb.AppendLine();
                sb.Append("(5:100) set %hello to {Hello World}.");
                sb.AppendLine();
            }

            Page page = engine.LoadFromString(sb.ToString());

            page.LoadAllLibraries();
            page.RemoveLibrary<Monkeyspeak.Libraries.Debug>();

            var sb2 = new StringBuilder();
            for (int i = 0; i < 50000; i++)
            {
                sb2.AppendLine();
                sb2.Append("(5:100) set %hello to {Hello World}.");
                sb2.AppendLine();
            }
            var tasks = new Task[5];
            for (int i = 0; i <= tasks.Length - 1; i++)
            {
                tasks[i] = engine.LoadFromStringAsync(page, sb2.ToString());
                tasks[i].ContinueWith(task => Logger.Info($"Triggers: {page.Size}"));
            }

            page.Error += DebugAllErrors;

            page.AddTriggerHandler(TriggerCategory.Cause, 0, HandleScriptStartCause);

            Task.WaitAll(tasks);
            Logger.Info($"Triggers: {page.Size}");
            //page.Execute(0);
        }

        [Test]
        public void SetGetVariableTest()
        {
            MonkeyspeakEngine engine = new MonkeyspeakEngine();
            engine.Options.VariableCountLimit = 100000;
            Page page = engine.LoadFromString(testScript);

            page.Error += DebugAllErrors;

            for (int i = 0; i <= 10000; i++)
                page.SetVariable(i.ToString(), true.ToString(), false);
            foreach (var variable in page.Scope)
            {
                Console.WriteLine(variable.ToString());
            }
        }

        [Test]
        public void IOLibraryTest()
        {
            var ioTestString = @"
(0:0) when the script starts,
	(5:100) set variable %file to {test.txt}.
	(5:102) print {%file} to the console.

(0:0) when the script starts,
	(1:200) and the file {%file} exists,
		(5:202) delete file {%file}.
		(5:203) create file {%file}.

(0:0) when the script starts,
	(1:200) and the file {%file} exists,
	(1:203) and the file {%file} can be written to,
		(5:200) append {Hello World from Monkeyspeak %VERSION!} to file {%file}.

(0:0) when the script starts,
	(5:150) take variable %test and add 2 to it.
	(5:102) print {%test} to the console.
";
            var engine = new MonkeyspeakEngine();
            var page = engine.LoadFromString(ioTestString);

            page.Error += DebugAllErrors;

            page.LoadAllLibraries();
            page.RemoveLibrary<Monkeyspeak.Libraries.Debug>();

            page.AddTriggerHandler(TriggerCategory.Cause, 0, HandleScriptStartCause);

            page.Execute(0);
            foreach (var v in page.Scope)
            {
                Logger.Debug(v);
            }
        }

        [Test]
        public void GetTriggerDescriptionsTest()
        {
            MonkeyspeakEngine engine = new MonkeyspeakEngine();
            engine.Options.Debug = true;
            Page page = engine.LoadFromString(testScript);

            page.Error += DebugAllErrors;

            page.LoadAllLibraries();
            page.RemoveLibrary<Monkeyspeak.Libraries.Debug>();

            foreach (string desc in page.GetTriggerDescriptions())
            {
                Console.WriteLine(desc);
            }
        }

        [Test]
        public void TimerLibraryTest()
        {
            var timerLibTestScript = @"
(0:0) when the script starts,
    (5:303) pause script execution for 2 seconds.
    (5:304) get the current time in seconds and put it into variable %currentTime.
    (5:102) print {Current Time %currentTime secs} to the console.

(0:0) when the script starts,
    (5:101) set variable %timer to 1.
    (5:300) create timer %timer to go off every 2 second(s) with a start delay of 1 second(s).
    (5:300) create timer 2 to go off every 5 second(s). *don't need delay part here
    (5:300) create timer 13 to go off every 900 second(s) with a start delay of # second(s).

(0:300) when timer 2 goes off,
(0:300) when timer %timer goes off,
    (5:302) get current timer and put the id into variable %timerId.
    (5:304) get the current uptime and put it into variable %currentTime.
    (5:102) print {Timer %timerId at %currentTime secs} to the console.

(0:300) when timer 2 goes off,
(0:300) when timer %timer goes off,
    (5:302) get current timer and put the id into variable %timerId.
    (5:304) get the current uptime and put it into variable %currentTime.
    (5:102) print {timerId = %timerId at %currentTime secs} to the console.
    (5:150) take variable %i2 and add 1 to it.
    (5:102) print {elapsed count = %i2} to the console.
";
            var engine = new MonkeyspeakEngine();
            engine.Options.Debug = true;
            var page = engine.LoadFromString(timerLibTestScript);

            page.Error += DebugAllErrors;

            page.LoadAllLibraries();
            page.RemoveLibrary<Monkeyspeak.Libraries.Debug>();

            page.AddTriggerHandler(TriggerCategory.Cause, 0, HandleScriptStartCause);

            page.Execute(0);
            System.Threading.Thread.Sleep(10000);
            page.Dispose();
        }

        [Test]
        public void NonExistantTimerTest()
        {
            var timerLibTestScript = @"

(0:0) when the script starts,
    (5:300) create timer 13 to go off every 900 second(s) with a start delay of 0 second(s).
    (5:102) print {Timer 13 created} to the console.

(0:300) when timer 2 goes off,
    (5:302) get current timer and put the id into variable %timerId.
    (5:304) get the current uptime and put it into variable %currentTime.
    (5:102) print {Timer %timerId at %currentTime secs} to the console.
";
            var engine = new MonkeyspeakEngine();
            engine.Options.Debug = true;
            var page = engine.LoadFromString(timerLibTestScript);

            //page.Error += DebugAllErrors;

            page.LoadAllLibraries();
            // page.RemoveLibrary<Monkeyspeak.Libraries.Debug>();

            page.AddTriggerHandler(TriggerCategory.Cause, 0, HandleScriptStartCause);

            page.Execute(0);
            System.Threading.Thread.Sleep(100);
            page.Dispose();
        }

        public static bool HandleScriptStartCause(Monkeyspeak.TriggerReader reader)
        {
            return true;
        }

        public static bool AlwaysFalseCond(TriggerReader reader)
        {
            return false;
        }

        private void DebugAllErrors(TriggerHandler handler, Monkeyspeak.Trigger trigger, Exception ex)
        {
            Logger.Error($"{handler.Method.Name} in {trigger.ToString(true)}\n{ex}");
#if DEBUG
            throw ex;
#endif
        }

        public static Token VisitTokens(ref Token token)
        {
            return new Token(TokenType.NONE, token.ValueStartPosition, token.Length, token.Position); // lolololol!
        }
    }
}