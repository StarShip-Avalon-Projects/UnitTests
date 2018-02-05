using Furcadia.IO;
using Furcadia.Logging;
using Furcadia.Net;
using Furcadia.Net.Options;
using Furcadia.Net.Proxy;
using Furcadia.Net.Utils.ServerParser;
using NUnit.Framework;
using System.IO;
using static FurcadiaLibTests.Utilities;

namespace FurcadiaLibTests.Net.Proxy
{
    [TestFixture]
    public class FurcadiaJoinDreamTests
    {
        #region Public Fields

        public Paths FurcPaths = new Paths();

        #endregion Public Fields

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

        private ProxySession Proxy;

        #endregion Private Fields

        [OneTimeSetUp]
        public void Initialize()
        {
#pragma warning disable CS0618 // Obsolete, Place holder till Accounts are ready
            var CharacterFile = Path.Combine(FurcPaths.CharacterPath,
#pragma warning restore CS0618 // Obsolete, Place holder till Accounts are ready
                "silvermonkey.ini");

            var Options = new ProxyOptions()
            {
                Standalone = true,
                CharacterIniFile = CharacterFile,
                ResetSettingTime = 10
            };

            Proxy = new ProxySession(Options);
            Proxy.ServerData2 += data => Proxy.SendToClient(data);
            Proxy.ClientData2 += data => Proxy.SendToServer(data);
            Proxy.Error += (e, o) => Logger.Error($"{e} {o}");
            BotHasConnected(Proxy.StandAlone);
        }

        public void BotHasConnected(bool StandAlone = true)
        {
            Proxy.StandAlone = StandAlone;
            Proxy.Connect();
            // HaltFor(ConnectWaitTime);

            Assert.Multiple(() =>
            {
                Assert.That(Proxy.ServerStatus,
                    Is.EqualTo(ConnectionPhase.Connected),
                    $"Proxy.ServerStatus {Proxy.ServerStatus}");
                Assert.That(Proxy.IsServerSocketConnected,
                    Is.EqualTo(true),
                    $"Proxy.IsServerSocketConnected {Proxy.IsServerSocketConnected}");
                if (StandAlone)
                {
                    Assert.That(Proxy.ClientStatus,
                        Is.EqualTo(ConnectionPhase.Disconnected),
                         $"Proxy.ClientStatus {Proxy.ClientStatus}");
                    Assert.That(Proxy.IsClientSocketConnected,
                        Is.EqualTo(false),
                         $"Proxy.IsClientSocketConnected {Proxy.IsClientSocketConnected}");
                    Assert.That(Proxy.FurcadiaClientIsRunning,
                        Is.EqualTo(false),
                        $"Proxy.FurcadiaClientIsRunning {Proxy.FurcadiaClientIsRunning}");
                }
                else
                {
                    Assert.That(Proxy.ClientStatus,
                        Is.EqualTo(ConnectionPhase.Connected),
                        $"Proxy.ClientStatus {Proxy.ClientStatus}");
                    Assert.That(Proxy.IsClientSocketConnected,
                        Is.EqualTo(true),
                        $"Proxy.IsClientSocketConnected {Proxy.IsClientSocketConnected}");
                    Assert.That(Proxy.FurcadiaClientIsRunning,
                        Is.EqualTo(true),
                        $"Proxy.FurcadiaClientIsRunning {Proxy.FurcadiaClientIsRunning}");
                }
            });
        }

        [TestCase("furc://furrabiannights/", "furrabiannights")]
        [TestCase("furc://theshoddyribbon:murdermysteryhotelwip/", "The Shoddy Ribbon", "Murder Mystery Hotel (WIP)")]
        [TestCase("furc://silvermonkey:stargatebase/", "SilverMonkey", "Stargate Base")]
        [TestCase("furc://imaginarium/", "imaginarium")]
        [TestCase("furc://vinca/", "vinca")]
        [Author("Gerolkae")]
        public void DreamBookmarkSettingsTest(string DreamUrl, string DreamOwner, string DreamTitle = null)
        {
            Proxy.ProcessServerInstruction += (data, handled) =>
            {
                if (data is DreamBookmark)
                {
                    Assert.Multiple(() =>
                    {
                        Assert.That(Proxy.Dream.DreamOwner, Is.EqualTo(DreamOwner.ToFurcadiaShortName()), $"Dream Owner: {Proxy.Dream.DreamOwner}");
                        if (string.IsNullOrWhiteSpace(Proxy.Dream.Title))
                            Assert.That(Proxy.Dream.DreamUrl, Is.EqualTo($"furc://{DreamOwner.ToFurcadiaShortName()}/"), $"Dream URL {Proxy.Dream.DreamUrl}");
                        else
                            Assert.That(Proxy.Dream.DreamUrl, Is.EqualTo($"furc://{DreamOwner.ToFurcadiaShortName()}:{DreamTitle.ToFurcadiaShortName()}/"), $"Dream URL {Proxy.Dream.DreamUrl}");
                    });
                }
            };

            Proxy.SendFormattedTextToServer($"`fdl {DreamUrl}");

            Proxy.ProcessServerInstruction -= (data, handled) =>
            {
                if (data is DreamBookmark)
                {
                    Assert.Multiple(() =>
                    {
                        Assert.That(Proxy.Dream.DreamOwner, Is.EqualTo(DreamOwner.ToFurcadiaShortName()), $"Dream Owner: {Proxy.Dream.DreamOwner}");
                        if (string.IsNullOrWhiteSpace(Proxy.Dream.Title))
                            Assert.That(Proxy.Dream.DreamUrl, Is.EqualTo($"furc://{DreamOwner.ToFurcadiaShortName()}/"), $"Dream URL {Proxy.Dream.DreamUrl}");
                        else
                            Assert.That(Proxy.Dream.DreamUrl, Is.EqualTo($"furc://{DreamOwner.ToFurcadiaShortName()}:{DreamTitle.ToFurcadiaShortName()}/"), $"Dream URL {Proxy.Dream.DreamUrl}");
                    });
                }
            };
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            DisconnectTests();
            Proxy.ClientData2 -= data => Proxy.SendToServer(data);
            Proxy.ServerData2 -= data => Proxy.SendToClient(data);
            Proxy.Error -= (e, o) => Logger.Error($"{e} {o}");

            Proxy.Dispose();
        }

        public void DisconnectTests()
        {
            Proxy.DisconnectServerAndClientStreams();
            if (!Proxy.StandAlone)
                HaltFor(CleanupDelayTime);

            Assert.Multiple(() =>
            {
                Assert.That(Proxy.ServerStatus,
                     Is.EqualTo(ConnectionPhase.Disconnected),
                    $"Proxy.ServerStatus {Proxy.ServerStatus}");
                Assert.That(Proxy.IsServerSocketConnected,
                     Is.EqualTo(false),
                    $"Proxy.IsServerSocketConnected {Proxy.IsServerSocketConnected}");
                Assert.That(Proxy.ClientStatus,
                     Is.EqualTo(ConnectionPhase.Disconnected),
                     $"Proxy.ClientStatus {Proxy.ClientStatus}");
                Assert.That(Proxy.IsClientSocketConnected,
                     Is.EqualTo(false),
                     $"Proxy.IsClientSocketConnected {Proxy.IsClientSocketConnected}");
                Assert.That(Proxy.FurcadiaClientIsRunning,
                     Is.EqualTo(false),
                    $"Proxy.FurcadiaClientIsRunning {Proxy.FurcadiaClientIsRunning}");
            });
        }

        [Test]
        public void DreamInfoIsSet()
        {
            Proxy.ProcessServerInstruction += (data, handled) =>
            {
                if (data is DreamBookmark)
                {
                    Assert.Multiple(() =>
                    {
                        Assert.That(Proxy.InDream,
                            Is.EqualTo(true),
                            "Bot has not joined a dream");
                        Assert.That(Proxy.Dream.Rating,
                            !Is.EqualTo(null),
                            $"Dream Rating is '{Proxy.Dream.Rating}'");
                        Assert.That(Proxy.Dream.Name,
                            !Is.EqualTo(null),
                            $"Dream Name is '{Proxy.Dream.Name}'");

                        Assert.That(Proxy.Dream.DreamOwner,
                            !Is.EqualTo(null),
                            $"Dream DreamOwner is '{Proxy.Dream.DreamOwner}'");

                        Assert.That(Proxy.Dream.DreamUrl,
                            !Is.EqualTo(null),
                            $"Dream URL is '{Proxy.Dream.DreamUrl}'");
                        Assert.That(string.IsNullOrWhiteSpace(Proxy.BanishName),
                            $"BanishName is '{Proxy.BanishName}'");
                        Assert.That(Proxy.BanishList,
                            !Is.EqualTo(null),
                            $"BanishList is '{Proxy.BanishList}'");
                        Assert.That(Proxy.BanishList.Count,
                            Is.EqualTo(0),
                            $"BanishList is '{Proxy.BanishList.Count}'");
                    });
                }
            };
            Proxy.SendFormattedTextToServer("`fdl furc://SilverMonkey");

            Proxy.ProcessServerInstruction -= (data, handled) =>
            {
                if (data is DreamBookmark)
                {
                    Assert.Multiple(() =>
                    {
                        Assert.That(Proxy.InDream,
                            Is.EqualTo(true),
                            "Bot has not joined a dream");
                        Assert.That(Proxy.Dream.Rating,
                            !Is.EqualTo(null),
                            $"Dream Rating is '{Proxy.Dream.Rating}'");
                        Assert.That(Proxy.Dream.Name,
                            !Is.EqualTo(null),
                            $"Dream Name is '{Proxy.Dream.Name}'");

                        Assert.That(Proxy.Dream.DreamOwner,
                            !Is.EqualTo(null),
                            $"Dream DreamOwner is '{Proxy.Dream.DreamOwner}'");

                        Assert.That(Proxy.Dream.DreamUrl,
                            !Is.EqualTo(null),
                            $"Dream URL is '{Proxy.Dream.DreamUrl}'");
                        Assert.That(string.IsNullOrWhiteSpace(Proxy.BanishName),
                            $"BanishName is '{Proxy.BanishName}'");
                        Assert.That(Proxy.BanishList,
                            !Is.EqualTo(null),
                            $"BanishList is '{Proxy.BanishList}'");
                        Assert.That(Proxy.BanishList.Count,
                            Is.EqualTo(0),
                            $"BanishList is '{Proxy.BanishList.Count}'");
                    });
                }
            };
        }
    }
}