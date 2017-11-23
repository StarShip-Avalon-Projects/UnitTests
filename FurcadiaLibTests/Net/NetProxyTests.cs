using Furcadia.Net.Options;
using Furcadia.Net.Proxy;
using NUnit.Framework;
using System.ComponentModel;
using Furcadia.Net.Utils.ServerParser;
using NUnit.Framework.Internal.Execution;
using Furcadia.Net.Dream;

using Furcadia.Net.Proxy;
using Furcadia.Net.Utils.ServerParser;

using System;
using System.Diagnostics;

namespace Furcadia.Net.Tests
{
    [TestFixture]
    public class NetProxyTests : ProxySession
    {
        private ProxySession Proxy;

        private ProxySession ProxySessionInitialize()
        {
            var options = new ProxySessionOptions()
            {
                CharacterIniFile = ""
            };
            var proxy = new ProxySession(ref options);
            proxy.Dream.Furres.Add(new Furre(1, "John"));
            proxy.Dream.Furres.Add(new Furre(2, "Bill Nye"));
            proxy.Dream.Furres.Add(new Furre(3, "John More"));
            proxy.Dream.Furres.Add(new Furre(4, "Silver Monkey"));
            proxy.Dream.Furres.Add(new Furre(5, "Gerolkae"));

            proxy.ConnectedFurre = new Furre(4, "Silver Monkey");
            return proxy;
        }

        private const string PingTest = @"<name shortname='gerolkae'>Gerolkae</name>: ping";
        private const string WhisperTest = "<font color='whisper'>[ <name shortname='gerolkae' src='whisper-from'>Gerolkae</name> whispers, \"hi\" to you. ]</font>";
        private const string PingTest2 = @"<name shortname='gerolkae'>Gerolkae</name>: Ping";
        private const string WhisperTest2 = "<font color='whisper'>[ <name shortname='gerolkae' src='whisper-from'>Gerolkae</name> whispers, \"Hi\" to you. ]</font>";
        private const string YouWhisper = "<font color='whisper'>[You whisper \"Logged on\" to<name shortname='gerolkae' forced src='whisper-to'>Gerolkae</name>. ]</font>";

        [TestCase(WhisperTest, 5, "Gerolkae")]
        [TestCase(PingTest, 5, "Gerolkae")]
        [TestCase(YouWhisper, 4, "Silver Monkey")]
        public void Test_FurreNameIs(string testc, int ExpectedFurreID, string ExpectedValue)
        {
            var t = ProxySessionInitialize();
            t.Error += OnErrorException;
            t.ProcessServerChannelData += delegate (object sender, ParseChannelArgs Args)
            {
                var channel = Args.Channel;
                var ServeObject = (ChannelObject)sender;
                Assert.AreEqual(new Furre(ExpectedFurreID, ExpectedValue), ServeObject.Player);
            };
            t.ParseServerChannel(testc, false);
            t.Dispose();
        }

        [TestCase(WhisperTest, "hi")]
        [TestCase(PingTest, "ping")]
        [TestCase(WhisperTest2, "Hi")]
        [TestCase(PingTest2, "Ping")]
        [TestCase(YouWhisper, "Logged on")]
        public void Test_FurreMessageIs(string testc, string ExpectedValue)
        {
            var t = ProxySessionInitialize();
            t.Error += OnErrorException;
            t.ProcessServerChannelData += delegate (object sender, ParseChannelArgs Args)
            {
                var ServeObject = (ChannelObject)sender;
                Assert.AreEqual(ExpectedValue, ServeObject.Player.Message);
            };
            t.ParseServerChannel(testc, false);
            t.Dispose();
        }

        [TestCase(YouWhisper, "whisper")]
        [TestCase(WhisperTest, "whisper")]
        [TestCase(PingTest, "say")]
        public void ProxySession_Test_Channel(string testc, string ExpectedValue)

        {
            var t = ProxySessionInitialize();
            t.Error += OnErrorException;
            t.ProcessServerChannelData += delegate (object sender, ParseChannelArgs Args)
            {
                var ServeObject = (ChannelObject)sender;
                Assert.IsTrue(ExpectedValue == ServeObject.Channel);
            };
            t.ParseServerChannel(testc, false);
            t.Dispose();
        }

        private void OnErrorException(Exception e, object o, string text)
        {
            Console.WriteLine($"{e.ToString()} {text}");
            Assert.Fail();
        }
    }
}