using Furcadia.Net.DreamInfo;
using Furcadia.Net.Options;
using Furcadia.Net.Proxy;
using Furcadia.Net.Utils.ServerParser;
using NUnit.Framework;
using System;

namespace Furcadia.Net.Tests
{
    [TestFixture]
    public class NetProxyTests : ProxySession
    {
        private ProxySession Proxy;

        private ProxySession ProxySessionInitialize()
        {
            var options = new ProxyOptions()
            {
                CharacterIniFile = ""
            };
            var proxy = new ProxySession(options);
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
        private const string YouWhisper2 = "<font color='whisper'>[ You whisper \"Logged on2\" to <name shortname='gerolkae' forced src='whisper-to'>Gerolkae</name>. ]</font>";
        private const string YouShouYo = "<font color='shout'>You shout, \"Yo Its Me\"</font>";
        private const string GeroShout = "<font color='shout'>{S} <name shortname='gerolkae'>Gerolkae</name> shouts: ping</font>";
        private const string Emote = "<font color='emote'><name shortname='silvermonkey'>Silver|Monkey</name> Emoe</font>";

        private const string Emit = "<font color='dragonspeak'><img src='fsh://system.fsh:91' alt='@emit' /><channel name='@emit' /> Silver|Monkey has arrived...</font>";
        private const string SpokenEmit = "<font color='emit'><img src='fsh://system.fsh:91' alt='@emit' /><channel name='@emit' /> Blah</font>";
        private const string EmitWarning = "<font color='warning'><img src='fsh://system.fsh:91' alt='@emit' /><channel name='@emit' /> (<name shortname='silvermonkey'>Silver|Monkey</name> just emitted.)</font>";

        [TestCase(WhisperTest, 5, "Gerolkae")]
        [TestCase(PingTest, 5, "Gerolkae")]
        [TestCase(YouWhisper, 4, "Silver Monkey")]
        [TestCase(YouWhisper2, 4, "Silver Monkey")]
        [TestCase(GeroShout, 5, "Gerolkae")]
        [TestCase(YouShouYo, 4, "Silver Monkey")]
        [TestCase(EmitWarning, 4, "Silver Monkey")]
        [TestCase(Emit, -1, "Furcadia Game Server")]
        [TestCase(SpokenEmit, -1, "Furcadia Game Server")]
        [TestCase(Emote, 4, "Silver monkey")]
        public void Test_FurreNameIs(string testc, int ExpectedFurreID, string ExpectedValue)
        {
            var t = ProxySessionInitialize();
            t.Error += OnErrorException;
            t.ProcessServerChannelData += delegate (object sender, ParseChannelArgs Args)
            {
                var channel = Args.Channel;
                var ServeObject = (ChannelObject)sender;
                Assert.That(ExpectedValue.ToFurcadiaShortName() == ServeObject.Player.ShortName);
            };

            t.ParseServerChannel(testc, false);
            t.ProcessServerChannelData -= delegate (object sender, ParseChannelArgs Args)
            {
                var channel = Args.Channel;
                var ServeObject = (ChannelObject)sender;
                Assert.That(ExpectedValue.ToFurcadiaShortName() == ServeObject.Player.ShortName);
            };
            t.Dispose();
        }

        [TestCase(WhisperTest, "hi")]
        [TestCase(PingTest, "ping")]
        [TestCase(WhisperTest2, "Hi")]
        [TestCase(PingTest2, "Ping")]
        [TestCase(YouWhisper, "Logged on")]
        [TestCase(YouWhisper, "Logged on")]
        [TestCase(YouShouYo, "Yo Its Me")]
        [TestCase(GeroShout, "ping")]
        [TestCase(EmitWarning, "(<name shortname='silvermonkey'>Silver|Monkey</name> just emitted.)")]
        [TestCase(Emit, "Silver|Monkey has arrived...")]
        [TestCase(SpokenEmit, "Blah")]
        [TestCase(Emote, "Emoe")]
        public void Test_FurreMessageIs(string testc, string ExpectedValue)
        {
            var t = ProxySessionInitialize();
            t.Error += OnErrorException;
            t.ProcessServerChannelData += delegate (object sender, ParseChannelArgs Args)
            {
                var ServeObject = (ChannelObject)sender;
                Assert.IsTrue(ExpectedValue == ServeObject.Player.Message);
            };
            t.ParseServerChannel(testc, false);
            t.ProcessServerChannelData -= delegate (object sender, ParseChannelArgs Args)
            {
                var ServeObject = (ChannelObject)sender;
                Assert.IsTrue(ExpectedValue == ServeObject.Player.Message);
            };

            t.Dispose();
        }

        [TestCase(YouWhisper, "whisper")]
        [TestCase(YouWhisper2, "whisper")]
        [TestCase(WhisperTest, "whisper")]
        [TestCase(PingTest, "say")]
        [TestCase(YouShouYo, "shout")]
        [TestCase(GeroShout, "shout")]
        [TestCase(EmitWarning, "@emit")]
        [TestCase(Emit, "@emit")]
        [TestCase(SpokenEmit, "@emit")]
        [TestCase(Emote, "emote")]
        public void ProxySession_Test_Channel(string testc, string ExpectedValue)

        {
            var t = ProxySessionInitialize();
            t.Error += OnErrorException;
            t.ProcessServerChannelData += delegate (object sender, ParseChannelArgs Args)
            {
                Assert.IsTrue(ExpectedValue == Args.Channel);
            };
            t.ParseServerChannel(testc, false);
            t.ProcessServerChannelData -= delegate (object sender, ParseChannelArgs Args)
            {
                Assert.IsTrue(ExpectedValue == Args.Channel);
            };
            t.Dispose();
        }

        private void OnErrorException(Exception e, object o)
        {
            Console.WriteLine($"{e} {o}");
        }
    }
}