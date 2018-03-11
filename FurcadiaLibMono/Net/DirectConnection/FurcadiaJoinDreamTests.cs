using Furcadia.IO;
using Furcadia.Logging;
using Furcadia.Net;
using Furcadia.Net.DirectConnection;
using Furcadia.Net.Options;
using Furcadia.Net.Utils.ServerParser;
using NUnit.Framework;
using System;
using System.IO;
using static FurcadiaLibMono.Utilities;

namespace FurcadiaLibMono.Net.DirectConnection
{
    [TestFixture]
    public class FurcadiaJoinDreamTests
    {
        [OneTimeSetUp]
        public void OnewTimeSetup()
        {
            Logger.InfoEnabled = true;
            Logger.SuppressSpam = false;
            Logger.ErrorEnabled = true;
            Logger.WarningEnabled = true;
            Logger.SingleThreaded = true;
            Logger.LogOutput = new MultiLogOutput(new FileLogOutput(AppDomain.CurrentDomain.BaseDirectory, Level.Debug), new FileLogOutput(AppDomain.CurrentDomain.BaseDirectory, Level.Error));
        }

        #region Private Fields

        private const string CookieBank = "<font color='emit'><img src='fsh://system.fsh:90' alt='@cookie' /><channel name='@cookie' /> Cookie <a href='http://www.furcadia.com/cookies/Cookie%20Economy.html'>bank</a> has currently collected: 0</font>";
        private const string Emit = "<font color='dragonspeak'><img src='fsh://system.fsh:91' alt='@emit' /><channel name='@emit' /> Silver|Monkey has arrived...</font>";
        private const string EmitBlah = "<font color='emit'><img src='fsh://system.fsh:91' alt='@emit' /><channel name='@emit' /> Blah</font>";
        private const string EmitTest = "<font color='emit'><img src='fsh://system.fsh:91' alt='@emit' /><channel name='@emit' /> test</font>";
        private const string EmitWarning = "<font color='warning'><img src='fsh://system.fsh:91' alt='@emit' /><channel name='@emit' /> (<name shortname='silvermonkey'>Silver|Monkey</name> just emitted.)</font>";
        private const string Emote = "<font color='emote'><name shortname='silvermonkey'>Silver|Monkey</name> Emoe</font>";
        private const string GeroSayPing = "<name shortname='gerolkae'>Gerolkae</name>: ping";
        private const string GeroShout = "<font color='shout'>{S} <name shortname='gerolkae'>Gerolkae</name> shouts: ping</font>";
        private const string GeroWhisperCunchatize = "<font color='whisper'>[ <name shortname='gerolkae' src='whisper-from'>Gerolkae</name> whispers, \"crunchatize\" to you. ]</font>";
        private const string GeroWhisperHi = "<font color='whisper'>[ <name shortname='gerolkae' src='whisper-from'>Gerolkae</name> whispers, \"Hi\" to you. ]</font>";
        private const string GeroWhisperRollOut = "<font color='whisper'>[ <name shortname='gerolkae' src='whisper-from'>Gerolkae</name> whispers, \"roll out\" to you. ]</font>";
        private const string PingTest = @"<name shortname='gerolkae'>Gerolkae</name>: ping";
        private const string PingTest2 = @"<name shortname='gerolkae'>Gerolkae</name>: Ping";
        private const string WhisperTest = "<font color='whisper'>[ <name shortname='gerolkae' src='whisper-from'>Gerolkae</name> whispers, \"hi\" to you. ]</font>";
        private const string YouShouYo = "<font color='shout'>You shout, \"Yo Its Me\"</font>";

        //  private const string YouWhisper = "<font color='whisper'>[You whisper \"Logged on\" to<name shortname='gerolkae' forced src='whisper-to'>Gerolkae</name>. ]</font>";
        private const string YouWhisper2 = "<font color='whisper'>[ You whisper \"Logged on\" to <name shortname='gerolkae' forced src='whisper-to'>Gerolkae</name>. ]</font>";

        private NetConnection Client;

        #endregion Private Fields

        [SetUp]
        public void Initialize()
        {
            var Character = new IniParser();
            var Options = Character.LoadOptionsFromIni(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dbugger.ini"));

            Client = new NetConnection(Options);

            Client.Error += (e, o) => Logger.Error($"{e} {o}");
            ClientHasConnected();
        }

        public void ClientHasConnected()
        {
            Client.Connect();
            HaltFor(ClientConnectWaitTime);

            Assert.Multiple((() =>
            {
                Assert.That(Client.ServerStatus,
                    Is.EqualTo(ConnectionPhase.Connected),
                    $"Client.ServerStatus {Client.ServerStatus}");
                Assert.That(Client.IsServerSocketConnected,
                    Is.EqualTo(true),
                    $"Client.IsServerSocketConnected {Client.IsServerSocketConnected}");
            }));
        }

        [TestCase("furc://furrabiannights/", "furrabiannights")]
        [TestCase("furc://theshoddyribbon:murdermysteryhotelwip/", "The Shoddy Ribbon", "Murder Mystery Hotel (WIP)")]
        [TestCase("furc://silvermonkey:stargatebase/", "SilverMonkey", "Stargate Base")]
        [TestCase("furc://imaginarium/", "imaginarium")]
        [TestCase("furc://vinca/", "vinca")]
        [Author("Gerolkae")]
        public void DreamBookmarkSettingsTest(string DreamUrl, string DreamOwner, string DreamTitle = null)
        {
            Client.ProcessServerInstruction += (data, handled) =>
            {
                if (data is DreamBookmark)
                {
                    Assert.Multiple(() =>
                    {
                        Assert.That(Client.Dream.DreamOwner, Is.EqualTo(DreamOwner.ToFurcadiaShortName()), $"Dream Owner: {Client.Dream.DreamOwner}");
                        if (string.IsNullOrWhiteSpace(Client.Dream.Title))
                            Assert.That(Client.Dream.DreamUrl, Is.EqualTo($"furc://{DreamOwner.ToFurcadiaShortName()}/"), $"Dream URL {Client.Dream.DreamUrl}");
                        else
                            Assert.That(Client.Dream.DreamUrl, Is.EqualTo($"furc://{DreamOwner.ToFurcadiaShortName()}:{DreamTitle.ToFurcadiaShortName()}/"), $"Dream URL {Client.Dream.DreamUrl}");
                    });
                }
            };

            Client.SendFormattedTextToServer($"`fdl {DreamUrl}");

            Client.ProcessServerInstruction -= (data, handled) =>
            {
                if (data is DreamBookmark)
                {
                    Assert.Multiple(() =>
                    {
                        Assert.That(Client.Dream.DreamOwner, Is.EqualTo(DreamOwner.ToFurcadiaShortName()), $"Dream Owner: {Client.Dream.DreamOwner}");
                        if (string.IsNullOrWhiteSpace(Client.Dream.Title))
                            Assert.That(Client.Dream.DreamUrl, Is.EqualTo($"furc://{DreamOwner.ToFurcadiaShortName()}/"), $"Dream URL {Client.Dream.DreamUrl}");
                        else
                            Assert.That(Client.Dream.DreamUrl, Is.EqualTo($"furc://{DreamOwner.ToFurcadiaShortName()}:{DreamTitle.ToFurcadiaShortName()}/"), $"Dream URL {Client.Dream.DreamUrl}");
                    });
                }
            };
        }

        [TearDown]
        public void Cleanup()
        {
            DisconnectTests();
            Client.Error -= (e, o) => Logger.Error($"{e} {o}");

            Client.Dispose();
        }

        public void DisconnectTests()
        {
            Client.Disconnect();
            //HaltFor(CleanupDelayTime);

            Assert.Multiple((() =>
            {
                Assert.That(this.Client.ServerStatus,
                     Is.EqualTo(ConnectionPhase.Disconnected),
                    $"Client.ServerStatus {this.Client.ServerStatus}");
                Assert.That(this.Client.IsServerSocketConnected,
                     Is.EqualTo(false),
                    $"Client.IsServerSocketConnected {this.Client.IsServerSocketConnected}");
            }));
        }

        [Test]
        public void DreamInfoIsSet()
        {
            Client.ProcessServerInstruction += (data, handled) =>
            {
                if (data is DreamBookmark)
                {
                    Assert.Multiple(() =>
                    {
                        Assert.That(Client.InDream,
                            Is.EqualTo(true),
                            "Bot has not joined a dream");
                        Assert.That(Client.Dream.Rating,
                            !Is.EqualTo(null),
                            $"Dream Rating is '{Client.Dream.Rating}'");
                        Assert.That(Client.Dream.Name,
                            !Is.EqualTo(null),
                            $"Dream Name is '{Client.Dream.Name}'");

                        Assert.That(Client.Dream.DreamOwner,
                            !Is.EqualTo(null),
                            $"Dream DreamOwner is '{Client.Dream.DreamOwner}'");

                        Assert.That(Client.Dream.DreamUrl,
                            !Is.EqualTo(null),
                            $"Dream URL is '{Client.Dream.DreamUrl}'");
                        Assert.That(string.IsNullOrWhiteSpace(Client.BanishName),
                            $"BanishName is '{Client.BanishName}'");
                        Assert.That(Client.BanishList,
                            !Is.EqualTo(null),
                            $"BanishList is '{Client.BanishList}'");
                        Assert.That(Client.BanishList.Count,
                            Is.EqualTo(0),
                            $"BanishList is '{Client.BanishList.Count}'");
                    });
                }
            };
            Client.SendFormattedTextToServer("`fdl furc://SilverMonkey");

            Client.ProcessServerInstruction -= (data, handled) =>
            {
                if (data is DreamBookmark)
                {
                    Assert.Multiple(() =>
                    {
                        Assert.That(Client.InDream,
                            Is.EqualTo(true),
                            "Bot has not joined a dream");
                        Assert.That(Client.Dream.Rating,
                            !Is.EqualTo(null),
                            $"Dream Rating is '{Client.Dream.Rating}'");
                        Assert.That(Client.Dream.Name,
                            !Is.EqualTo(null),
                            $"Dream Name is '{Client.Dream.Name}'");

                        Assert.That(Client.Dream.DreamOwner,
                            !Is.EqualTo(null),
                            $"Dream DreamOwner is '{Client.Dream.DreamOwner}'");

                        Assert.That(Client.Dream.DreamUrl,
                            !Is.EqualTo(null),
                            $"Dream URL is '{Client.Dream.DreamUrl}'");
                        Assert.That(string.IsNullOrWhiteSpace(Client.BanishName),
                            $"BanishName is '{Client.BanishName}'");
                        Assert.That(Client.BanishList,
                            !Is.EqualTo(null),
                            $"BanishList is '{Client.BanishList}'");
                        Assert.That(Client.BanishList.Count,
                            Is.EqualTo(0),
                            $"BanishList is '{Client.BanishList.Count}'");
                    });
                }
            };
        }
    }
}