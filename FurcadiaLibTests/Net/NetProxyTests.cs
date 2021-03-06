﻿using Furcadia.Logging;
using Furcadia.Net.DreamInfo;
using Furcadia.Net.Options;
using Furcadia.Net.Proxy;
using Furcadia.Net.Utils.ServerParser;
using NUnit.Framework;
using System;

/// <summary>
/// Don't Actually Connect to Furcadia. Just Test <see cref="ProcessServerChannelData"/> and
/// <see cref="ParseServerChannel"/>
/// </summary>
namespace FurcadiaLibTests.Net.Proxy.DisconnectedTests
{
    [TestFixture]
    [NonParallelizable]
    public class NetProxyTests : ProxySession
    {
        #region Private Fields

        private const string Emit = "<font color='dragonspeak'><img src='fsh://system.fsh:91' alt='@emit' /><channel name='@emit' /> Silver|Monkey has arrived...</font>";
        private const string EmitWarning = "<font color='warning'><img src='fsh://system.fsh:91' alt='@emit' /><channel name='@emit' /> (<name shortname='silvermonkey'>Silver|Monkey</name> just emitted.)</font>";
        private const string Emote = "<font color='emote'><name shortname='silvermonkey'>Silver|Monkey</name> Emoe</font>";
        private const string GeroShout = "<font color='shout'>{S} <name shortname='gerolkae'>Gerolkae</name> shouts: ping</font>";
        private const string PingTest = @"<name shortname='gerolkae'>Gerolkae</name>: ping";
        private const string PingTest2 = @"<name shortname='gerolkae'>Gerolkae</name>: Ping";
        private const string SpokenEmit = "<font color='emit'><img src='fsh://system.fsh:91' alt='@emit' /><channel name='@emit' /> Blah</font>";
        private const string WhisperTest = "<font color='whisper'>[ <name shortname='gerolkae' src='whisper-from'>Gerolkae</name> whispers, \"hi\" to you. ]</font>";
        private const string WhisperTest2 = "<font color='whisper'>[ <name shortname='gerolkae' src='whisper-from'>Gerolkae</name> whispers, \"Hi\" to you. ]</font>";
        private const string YouShouYo = "<font color='shout'>You shout, \"Yo Its Me\"</font>";
        private const string YouWhisper = "<font color='whisper'>[You whisper \"Logged on\" to<name shortname='gerolkae' forced src='whisper-to'>Gerolkae</name>. ]</font>";
        private const string YouWhisper2 = "<font color='whisper'>[ You whisper \"Logged on2\" to <name shortname='gerolkae' forced src='whisper-to'>Gerolkae</name>. ]</font>";

        //    private ProxySession Proxy;
        public const string ErrorGeroOffline = "<font color='error'>Sorry, there's no furre around right now with an exact name gerolkae! To find similar names, try typing the name without the '%' at the beginning of the name. -- Beekin the Help Dragon</font>";

        #endregion Private Fields

        #region Public Methods

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
        public void NetProxy_Test_Channel(string testc, string ExpectedValue)

        {
            var Proxy = ProxySessionInitialize();

            Proxy.ProcessServerChannelData += (sender, Args) =>
            {
                if (sender is ChannelObject ServeObject)
                {
                    Assert.That(Args.Channel,
                        Is.EqualTo(ExpectedValue),
                        $"Args.Channel '{Args.Channel}' ExpectedValue: {ExpectedValue}"
                        );
                }
            };

            Proxy.ParseServerChannel(testc, false);
            Proxy.ProcessServerChannelData -= (sender, Args) =>
            {
                if (sender is ChannelObject ServeObject)
                {
                    Assert.That(Args.Channel,
                        Is.EqualTo(ExpectedValue),
                        $"Args.Channel '{Args.Channel}' ExpectedValue: {ExpectedValue}"
                        );
                }
            };

            Proxy.Dispose();
        }

        [TestCase(WhisperTest, "hi")]
        [TestCase(WhisperTest2, "Hi")]
        [TestCase(YouWhisper, "Logged on")]
        [TestCase(YouShouYo, "Yo Its Me")]
        [TestCase(GeroShout, "ping")]
        [TestCase(EmitWarning, "(<name shortname='silvermonkey'>Silver|Monkey</name> just emitted.)")]
        [TestCase(Emit, "Silver|Monkey has arrived...")]
        [TestCase(SpokenEmit, "Blah")]
        [TestCase(Emote, "Emoe")]
        [TestCase(ErrorGeroOffline, "Sorry, there's no furre around right now with an exact name gerolkae! To find similar names, try typing the name without the '%' at the beginning of the name. -- Beekin the Help Dragon")]
        public void Test_FurreMessageIs(string testc, string ExpectedValue)
        {
            var Proxy = ProxySessionInitialize();
            bool IsTested = false;
            Proxy.ProcessServerChannelData += (sender, Args) =>
            {
                if (!IsTested && sender is ChannelObject ServeObject)
                {
                    Assert.That(ServeObject.Player.Message.Trim(),
                        Is.EqualTo(ExpectedValue.Trim()));
                    IsTested = true;
                }
            };

            Proxy.ParseServerChannel(testc, false);

            Proxy.ProcessServerChannelData -= (sender, Args) =>
            {
                if (!IsTested && sender is ChannelObject ServeObject)
                {
                    Assert.That(ServeObject.Player.Message.Trim(),
                        Is.EqualTo(ExpectedValue.Trim()));
                    IsTested = true;
                }
            };

            Proxy.Dispose();
        }

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
            t.ProcessServerChannelData += (sender, Args) =>
            {
                var channel = Args.Channel;
                var ServeObject = (ChannelObject)sender;
                Assert.That(ExpectedValue.ToFurcadiaShortName() == ServeObject.Player.ShortName);
            };

            t.ParseServerChannel(testc, false);
            t.ProcessServerChannelData -= (sender, Args) =>
           {
               var channel = Args.Channel;
               var ServeObject = (ChannelObject)sender;
               Assert.That(ExpectedValue.ToFurcadiaShortName() == ServeObject.Player.ShortName);
           };
            t.Dispose();
        }

        #endregion Public Methods

        #region Private Methods

        [SetUp]
        public void Initialize()
        {
        }

        private ProxySession ProxySessionInitialize()
        {
            var options = new ProxyOptions()
            {
                CharacterIniFile = ""
            };
            var proxy = new ProxySession(options);
            proxy.Error += (e, o) => Logger.Error($"{e} {o}");

            Furres.Add(new Furre(1, "John"));
            Furres.Add(new Furre(2, "Bill Nye"));
            Furres.Add(new Furre(3, "John More"));
            Furres.Add(new Furre(4, "Silver|Monkey"));
            Furres.Add(new Furre(5, "Gerolkae"));
            proxy.ConnectedFurreId = 4;
            proxy.ConnectedFurreName = "Silver|Monkey";
            // proxy.ConnectedFurre = new Furre(4, "Silver Monkey");

            return proxy;
        }

        #endregion Private Methods
    }
}