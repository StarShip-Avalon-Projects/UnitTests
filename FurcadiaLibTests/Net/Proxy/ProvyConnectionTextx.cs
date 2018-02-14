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
    [NonParallelizable]
    public class ProcyConnectionTests_Alt_SilverMonkey
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

        public ProcyConnectionTests_Alt_SilverMonkey()
        {
            SettingsFile = Path.Combine(FurcPaths.SettingsPath, @"settings.ini");
            BackupSettingsFile = Path.Combine(FurcPaths.SettingsPath, @"BackupSettings.ini");
            File.Copy(SettingsFile, BackupSettingsFile, true);
        }

        #endregion Public Constructors

        #region Public Properties

        public string BackupSettingsFile { get; private set; }
        public ProxyOptions Options { get; private set; }
        public string SettingsFile { get; private set; }

        #endregion Public Properties

        #region Public Methods

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
            Proxy.ProcessServerChannelData += (sender, Args) =>
            {
                if (sender is ChannelObject ServeObject)
                    Assert.That(ServeObject.Player.Message,
                        Is.EqualTo(ExpectedValue));
            };
            Proxy.ParseServerChannel(testc, false);
            Proxy.ProcessServerChannelData -= (sender, Args) =>
            {
                if (sender is ChannelObject ServeObject)
                    Assert.That(ServeObject.Player.Message,
                        Is.EqualTo(ExpectedValue));
            };
            Logger.Debug($"ServerStatus: {Proxy.ServerStatus}");
            Logger.Debug($"ClientStatus: {Proxy.ClientStatus}");
        }

        [TearDown]
        public void Cleanup()
        {
            DisconnectTests();
            Proxy.ClientData2 -= data => Proxy.SendToServer(data);
            Proxy.ServerData2 -= data => Proxy.SendToClient(data);
            Proxy.Error -= (e, o) => Logger.Error($"{e} {o}");

            Proxy.Dispose();
            Options = null;
        }

        public void DisconnectTests()
        {
            Proxy.ServerStatusChanged += (sender, e) =>
            {
                if (e.ConnectPhase == ConnectionPhase.Disconnected)
                {
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
            };
            Proxy.DisconnectServerAndClientStreams();
            Proxy.ServerStatusChanged -= (sender, e) =>
            {
                if (e.ConnectPhase == ConnectionPhase.Disconnected)
                {
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
            };
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
            //if (!Proxy.StandAlone)
            //    HaltFor(DreamEntranceDelay);

            Proxy.ProcessServerChannelData += (sender, Args) =>
            {
                if (sender is ChannelObject ServeObject)
                {
                    Assert.That(Args.Channel,
                        Is.EqualTo(ExpectedValue));
                }
            };

            Proxy.ParseServerChannel(ChannelCode, false);
            Proxy.ProcessServerChannelData -= (sender, Args) =>
            {
                if (sender is ChannelObject ServeObject)
                {
                    Assert.That(Args.Channel,
                        Is.EqualTo(ExpectedValue));
                }
            };
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
            Proxy.ProcessServerChannelData += (sender, Args) =>
            {
                if (sender is ChannelObject ServeObject)
                {
                    Assert.That(ServeObject.Player.ShortName,
                        Is.EqualTo(ExpectedValue.ToFurcadiaShortName()));
                }
            };

            Proxy.ParseServerChannel(testc, false);
            Proxy.ProcessServerChannelData -= (sender, Args) =>
            {
                if (sender is ChannelObject ServeObject)
                {
                    Assert.That(ServeObject.Player.ShortName,
                        Is.EqualTo(ExpectedValue.ToFurcadiaShortName()));
                }
            };
            Logger.Debug($"ServerStatus: {Proxy.ServerStatus}");
            Logger.Debug($"ClientStatus: {Proxy.ClientStatus}");
        }

        [SetUp]
        public void Initialize()
        {
#pragma warning disable CS0618 // Obsolete, Place holder till Accounts are ready
            var CharacterFile = Path.Combine(FurcPaths.CharacterPath,
#pragma warning restore CS0618 // Obsolete, Place holder till Accounts are ready
                "silvermonkey.ini");

            Options = new ProxyOptions()
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

        [TestCase(GeroShout, "ping")]
        public void ProxySession_InstructionObjectPlayerIs(string testc, string ExpectedValue)
        {
            Proxy.SendFormattedTextToServer("- Shout");
            Proxy.ProcessServerChannelData += (sender, Args) =>
            {
                if (sender is ChannelObject InstructionObject)
                {
                    Assert.That(InstructionObject.Player.Message,
                        Is.EqualTo(ExpectedValue));
                }
            };

            Proxy.ParseServerChannel(testc, false);
            Proxy.ProcessServerChannelData -= (sender, Args) =>
            {
                if (sender is ChannelObject InstructionObject)
                {
                    Assert.That(InstructionObject.Player.Message,
                        Is.EqualTo(ExpectedValue));
                }
            };
        }

        #endregion Public Methods
    }
}