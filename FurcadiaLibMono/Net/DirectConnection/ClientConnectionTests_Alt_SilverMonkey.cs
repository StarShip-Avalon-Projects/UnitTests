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
    public class ClientConnectionTests_Alt_SilverMonkey
    {
        #region Public Fields
        object parseLock = new object();
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

        //private const string PingTest = @"<name shortname='gerolkae'>Gerolkae</name>: ping";
        //private const string PingTest2 = @"<name shortname='gerolkae'>Gerolkae</name>: Ping";
        private const string WhisperTest = "<font color='whisper'>[ <name shortname='gerolkae' src='whisper-from'>Gerolkae</name> whispers, \"hi\" to you. ]</font>";

        private const string YouShouYo = "<font color='shout'>You shout, \"Yo Its Me\"</font>";

        //  private const string YouWhisper = "<font color='whisper'>[You whisper \"Logged on\" to<name shortname='gerolkae' forced src='whisper-to'>Gerolkae</name>. ]</font>";
        private const string YouWhisper2 = "<font color='whisper'>[ You whisper \"Logged on\" to <name shortname='gerolkae' forced src='whisper-to'>Gerolkae</name>. ]</font>";

        private NetConnection Client;

        #endregion Private Fields

        #region Public Properties

        public ClientOptions Options { get; private set; }

        #endregion Public Properties

        #region Public Methods

        public void ClientHasConnected()
        {
            Client.Connect();
            HaltFor(ClientConnectWaitTime);

            Assert.Multiple(() =>
            {
                Assert.That(Client.ServerStatus,
                    Is.EqualTo(ConnectionPhase.Connected),
                    $"Client.ServerStatus {Client.ServerStatus}");
                Assert.That(Client.IsServerSocketConnected,
                    Is.EqualTo(true),
                    $"Client.IsServerSocketConnected {Client.IsServerSocketConnected}");
            });
        }

        [TestCase(WhisperTest, "hi")]
        // [TestCase(PingTest, "ping")]
        [TestCase(GeroWhisperHi, "Hi")]
        //  [TestCase(PingTest2, "Ping")]
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
            lock (parseLock)
            {
                bool isTested = false;
                Client.ProcessServerChannelData += (sender, Args) =>
                {
                    if (!isTested && sender is ChannelObject ServeObject)
                    {
                        isTested = true;
                        Assert.That(ServeObject.Player.Message.Trim(),
                            Is.EqualTo(ExpectedValue.Trim()),
                            $"Player.Message '{ServeObject.Player.Message}' ExpectedValue: {ExpectedValue}"
                            );
                    }
                };
                Client.ParseServerChannel(testc, false);
                Client.ProcessServerChannelData -= (sender, Args) =>
                {
                    if (!isTested && sender is ChannelObject ServeObject)
                    {
                        isTested = true;
                        Assert.That(ServeObject.Player.Message.Trim(),
                            Is.EqualTo(ExpectedValue.Trim()),
                            $"Player.Message '{ServeObject.Player.Message}' ExpectedValue: {ExpectedValue}"
                            );
                    }
                };
                Logger.Debug($"ServerStatus: {Client.ServerStatus}");
            }
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            DisconnectTests();
            Client.Error -= (e, o) => Logger.Error($"{e} {o}");

            Client.Dispose();
            Options = null;
        }

        public void DisconnectTests()
        {
            Client.ServerStatusChanged += (sender, e) =>
            {
                if (e.ConnectPhase == ConnectionPhase.Disconnected)
                {
                    Assert.Multiple(() =>
                    {
                        Assert.That(Client.ServerStatus,
                             Is.EqualTo(ConnectionPhase.Disconnected),
                            $"Client.ServerStatus {Client.ServerStatus}");
                        Assert.That(Client.IsServerSocketConnected,
                             Is.EqualTo(false),
                            $"Client.IsServerSocketConnected {Client.IsServerSocketConnected}");
                    });
                }
            };
            Client.Disconnect();
            Client.ServerStatusChanged -= (sender, e) =>
            {
                if (e.ConnectPhase == ConnectionPhase.Disconnected)
                {
                    Assert.Multiple(() =>
                    {
                        Assert.That(Client.ServerStatus,
                             Is.EqualTo(ConnectionPhase.Disconnected),
                            $"Client.ServerStatus {Client.ServerStatus}");
                        Assert.That(Client.IsServerSocketConnected,
                             Is.EqualTo(false),
                            $"Client.IsServerSocketConnected {Client.IsServerSocketConnected}");
                    });
                }
            };
        }

        //   [TestCase(YouWhisper, "whisper")]
        [TestCase(YouWhisper2, "whisper")]
        [TestCase(WhisperTest, "whisper")]
        [TestCase(GeroWhisperHi, "whisper")]
        //  [TestCase(PingTest, "say")]
        [TestCase(YouShouYo, "shout")]
        [TestCase(GeroShout, "shout")]
        [TestCase(EmitWarning, "@emit")]
        [TestCase(Emit, "@emit")]
        [TestCase(EmitBlah, "@emit")]
        [TestCase(Emote, "emote")]
        public void ExpectedChannelNameIs(string ChannelCode, string ExpectedValue)
        {
            lock (parseLock)
            {
                //if (!Client.StandAlone)
                //    HaltFor(DreamEntranceDelay);
                bool isTested = false;
                Client.ProcessServerChannelData += (sender, Args) =>
                {
                    if (!isTested && sender is ChannelObject ServeObject)
                    {
                        isTested = true;
                        Assert.That(Args.Channel,
                            Is.EqualTo(ExpectedValue));
                    }
                };

                Client.ParseServerChannel(ChannelCode, false);
                Client.ProcessServerChannelData -= (sender, Args) =>
                {
                    if (!isTested && sender is ChannelObject ServeObject)
                    {
                        isTested = true;
                        Assert.That(Args.Channel,
                            Is.EqualTo(ExpectedValue));
                    }
                };
            }
        }

        [TestCase(WhisperTest, "Gerolkae")]
        // [TestCase(PingTest, "Gerolkae")]
        [TestCase(YouWhisper2)]
        [TestCase(GeroShout, "Gerolkae")]
        [TestCase(YouShouYo)]
        [TestCase(EmitWarning, "Silver Monkey")]
        [TestCase(Emit, "Furcadia Game Server")]
        [TestCase(EmitBlah, "Furcadia Game Server")]
        [TestCase(Emote, "Silver monkey")]
        [TestCase(GeroWhisperHi, "Gerolkae")]
        public void ExpectedCharachter(string testc, string ExpectedValue = "you")
        {
            lock (parseLock)
            {
                bool isTested = false;
                Client.ProcessServerChannelData += (sender, Args) =>
                {
                    if (!isTested && sender is ChannelObject ServeObject)
                    {
                        isTested = true;
                        if (ExpectedValue == "you")
                            Assert.That(ServeObject.Player.ShortName,
                                Is.EqualTo(Client.ConnectedFurre.ShortName));
                        else
                            Assert.That(ServeObject.Player.ShortName,
                                Is.EqualTo(ExpectedValue.ToFurcadiaShortName()));
                    }
                };

                Client.ParseServerChannel(testc, false);
                Client.ProcessServerChannelData -= (sender, Args) =>
                {
                    if (!isTested && sender is ChannelObject ServeObject)
                    {
                        isTested = true;
                        if (ExpectedValue == "you")
                            Assert.That(ServeObject.Player.ShortName,
                                Is.EqualTo(Client.ConnectedFurre.ShortName));
                        else
                            Assert.That(ServeObject.Player.ShortName,
                                Is.EqualTo(ExpectedValue.ToFurcadiaShortName()));
                    }
                };
                Logger.Debug($"ServerStatus: {Client.ServerStatus}");
            }
        }

        [OneTimeSetUp]
        public void Initialize()
        {
            var Character = new IniParser();
            Options = Options = Character.LoadOptionsFromIni(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dbugger.ini"));

            Client = new NetConnection(Options);

            Client.Error += (e, o) => Logger.Error($"{e} {o}");
            ClientHasConnected();
        }

        [TestCase(GeroShout, "ping")]
        public void ClientSession_InstructionObjectPlayerIs(string testc, string ExpectedValue)
        {
            lock (parseLock)
            {
                bool isTested = false;
                Client.SendFormattedTextToServer("- Shout");
                Client.ProcessServerChannelData += (sender, Args) =>
                {
                    if (!isTested && sender is ChannelObject InstructionObject)
                    {
                        Assert.That(InstructionObject.Player.Message,
                            Is.EqualTo(ExpectedValue));
                        isTested = true;
                    }
                };

                Client.ParseServerChannel(testc, false);
                Client.ProcessServerChannelData -= (sender, Args) =>
                {
                    if (!isTested && sender is ChannelObject InstructionObject)
                    {
                        Assert.That(InstructionObject.Player.Message,
                            Is.EqualTo(ExpectedValue));
                        isTested = true;
                    }
                };
            }
        }

        #endregion Public Methods
    }
}