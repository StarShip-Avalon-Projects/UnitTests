using Furcadia.IO;
using Furcadia.Logging;
using Furcadia.Net;
using Furcadia.Net.Options;
using Furcadia.Net.Proxy;
using Furcadia.Net.Utils.ServerParser;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;
using static FurcadiaLibTests.Utilities;

namespace FurcadiaLibTests.Net.Proxy
{
    [TestFixture]
    public class ProcyConnectionTests
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

        #region Public Constructors

        public ProcyConnectionTests()
        {
            SettingsFile = Path.Combine(FurcPaths.SettingsPath, @"settings.ini");
            BackupSettingsFile = Path.Combine(FurcPaths.SettingsPath, @"BackupSettings.ini");
            File.Copy(SettingsFile, BackupSettingsFile, true);
        }

        #endregion Public Constructors

        #region Public Properties

        public string BackupSettingsFile { get; private set; }
        public ProxyOptions options { get; private set; }
        public string SettingsFile { get; private set; }

        #endregion Public Properties

        #region Public Methods

        public void BotHasConnected_StandAlone(bool StandAlone = false)
        {
            Proxy.StandAlone = StandAlone;
            Proxy.Connect();

            HaltFor(ConnectWaitTime);

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

        [TestCase(WhisperTest, "hi")]
        [TestCase(PingTest, "ping")]
        [TestCase(GeroWhisperHi, "Hi")]
        [TestCase(PingTest2, "Ping")]
        //    [TestCase(YouWhisper, "Logged on")]
        [TestCase(YouShouYo, "Yo Its Me")]
        [TestCase(GeroShout, "ping")]
        [TestCase(EmitWarning, "(<name shortname='silvermonkey'>Silver|Monkey</name> just emitted.)")]
        [TestCase(Emit, "Silver|Monkey has arrived...")]
        [TestCase(EmitBlah, "Blah")]
        [TestCase(Emote, "Emoe")]
        [TestCase(EmitTest, "test")]
        public void ChannelTextIs(string testc, string ExpectedValue)
        {
            BotHasConnected_StandAlone();

            Proxy.ProcessServerChannelData += delegate (object sender, ParseChannelArgs Args)
             {
                 var ServeObject = (ChannelObject)sender;
                 Assert.That(ServeObject.Player.Message,
                     Is.EqualTo(ExpectedValue));
             };

            Console.WriteLine($"ServerStatus: {Proxy.ServerStatus}");
            Console.WriteLine($"ClientStatus: {Proxy.ClientStatus}");
            Proxy.ParseServerChannel(testc, false);
            DisconnectTests();
        }

        [TearDown]
        public void Cleanup()
        {
            Proxy.ClientData2 -= data => Proxy.SendToServer(data);
            Proxy.ServerData2 -= data => Proxy.SendToClient(data);
            Proxy.Error -= (e, o) => Logger.Error($"{e} {o}");

            Proxy.Dispose();
            options = null;
        }

        public void DisconnectTests(bool StandAlone = false)
        {
            Proxy.DisconnectServerAndClientStreams();
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

        [TestCase(true)]
        [TestCase(false)]
        public void DreamInfoIsSet_StandAlone(bool StandAlone)
        {
            BotHasConnected_StandAlone(StandAlone);
            HaltFor(DreamEntranceDelay);

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
                if (Proxy.Dream.IsPermanent)
                {
                    Assert.That(Proxy.Dream.DreamOwner,
                        !Is.EqualTo(null),
                        $"Dream DreamOwner is '{Proxy.Dream.DreamOwner}'");
                }
                else
                {
                    Assert.That(Proxy.Dream.DreamOwner,
                        !Is.EqualTo(null),
                        $"Dream DreamOwner is '{Proxy.Dream.DreamOwner}'");
                    //private dreams most likley to be personal or ddream packs
                    // Dream Owner shoule be set
                }

                Assert.That(Proxy.Dream.URL,
                    !Is.EqualTo(null),
                    $"Dream URL is '{Proxy.Dream.URL}'");
                //Assert.That(Proxy.Dream.Lines,
                //    Is.GreaterThan(0),
                //    $"DragonSpeak Lines {Proxy.Dream.Lines}");
                Assert.That(string.IsNullOrWhiteSpace(Proxy.BanishName),
                    $"BanishName is '{Proxy.BanishName}'");
                Assert.That(Proxy.BanishList,
                    !Is.EqualTo(null),
                    $"BanishList is '{Proxy.BanishList}'");
                Assert.That(Proxy.BanishList.Count,
                    Is.EqualTo(0),
                    $"BanishList is '{Proxy.BanishList.Count}'");
            });
            DisconnectTests(StandAlone);
        }

        //   [TestCase(YouWhisper, "whisper")]
        [TestCase(YouWhisper2, "whisper")]
        [TestCase(WhisperTest, "whisper")]
        [TestCase(GeroWhisperHi, "whisper")]
        [TestCase(PingTest, "say")]
        [TestCase(YouShouYo, "shout")]
        [TestCase(GeroShout, "shout")]
        [TestCase(EmitWarning, "@emit")]
        [TestCase(Emit, "@emit")]
        [TestCase(EmitBlah, "@emit")]
        [TestCase(Emote, "emote")]
        public void ExpectedChannelNameIs(string ChannelCode, string ExpectedValue)
        {
            BotHasConnected_StandAlone();
            HaltFor(DreamEntranceDelay);

            Proxy.ProcessServerChannelData += delegate (object sender, ParseChannelArgs Args)
            {
                var ServeObject = (ChannelObject)sender;
                Assert.That(Args.Channel,
                    Is.EqualTo(ExpectedValue));
            };

            Proxy.ParseServerChannel(ChannelCode, false);
            DisconnectTests();
        }

        [TestCase(WhisperTest, "Gerolkae")]
        [TestCase(PingTest, "Gerolkae")]
        [TestCase(YouWhisper2, "Silver Monkey")]
        [TestCase(GeroShout, "Gerolkae")]
        [TestCase(YouShouYo, "Silver Monkey")]
        [TestCase(EmitWarning, "Silver Monkey")]
        [TestCase(Emit, "Furcadia Game Server")]
        [TestCase(EmitBlah, "Furcadia Game Server")]
        [TestCase(Emote, "Silver monkey")]
        [TestCase(GeroWhisperHi, "Gerolkae")]
        public void ExpectedCharachter(string testc, string ExpectedValue)
        {
            BotHasConnected_StandAlone();

            Proxy.ProcessServerChannelData += (sender, Args) =>
           {
               var ServeObject = (ChannelObject)sender;
               Assert.That(ServeObject.Player.ShortName, Is.EqualTo(ExpectedValue.ToFurcadiaShortName()));
           };

            Console.WriteLine($"ServerStatus: {Proxy.ServerStatus}");
            Console.WriteLine($"ClientStatus: {Proxy.ClientStatus}");
            Proxy.ParseServerChannel(testc, false);
            Proxy.ProcessServerChannelData -= (sender, Args) =>
            {
                var ServeObject = (ChannelObject)sender;
                Assert.That(ServeObject.Player.ShortName,
                    Is.EqualTo(ExpectedValue.ToFurcadiaShortName()));
            };
            DisconnectTests();
        }

        [SetUp]
        public void Initialize()
        {
#pragma warning disable CS0618 // Obsolete, Place holder till Accounts are ready
            var CharacterFile = Path.Combine(FurcPaths.CharacterPath,
#pragma warning restore CS0618 // Obsolete, Place holder till Accounts are ready
                "silvermonkey.ini");

            options = new ProxyOptions()
            {
                Standalone = true,
                CharacterIniFile = CharacterFile,
                ResetSettingTime = 10
            };

            Proxy = new ProxySession(options);
            Proxy.ServerData2 += data => Proxy.SendToClient(data);
            Proxy.ClientData2 += data => Proxy.SendToServer(data);
            Proxy.Error += (e, o) => Logger.Error($"{e} {o}");
        }

        [TestCase(GeroShout, "ping")]
        public void ProxySession_InstructionObjectPlayerIs(string testc, string ExpectedValue)
        {
            BotHasConnected_StandAlone();

            //  Proxy.Error += OnErrorException;
            Proxy.ProcessServerChannelData += delegate (object sender, ParseChannelArgs Args)
            {
                ChannelObject InstructionObject = (ChannelObject)sender;
                Assert.That(InstructionObject.Player.Message,
                    Is.EqualTo(ExpectedValue));
            };
            Task.Run(() => Proxy.SendFormattedTextToServer("- Shout")).Wait();

            Proxy.ParseServerChannel(testc, false);
            DisconnectTests();
        }

        #endregion Public Methods
    }
}