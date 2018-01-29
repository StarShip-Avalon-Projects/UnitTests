using Furcadia.Logging;
using Furcadia.Net;
using Furcadia.Net.Options;
using Furcadia.Net.Proxy;
using Furcadia.Net.Utils.ServerParser;
using NUnit.Framework;
using System;
using System.IO;
using static FurcadiaLibTests.Utilities;

namespace FurcadiaLibTests.Net.Proxy
{
    [TestFixture]
    public class QueryTests
    {
        #region Public Fields

        public const string GeroCuddleBot = "<font color='query'><name shortname='gerolkae'>Gerolkae</name> asks you to cuddle with them. To accept the request, <a href='command://cuddle'>click here</a> or type `cuddle and press &lt;enter&gt;.</font>";
        public const string GeroFollowBot = "<font color='query'><name shortname='gerolkae'>Gerolkae</name> requests permission to follow you. To accept the request, <a href='command://lead'>click here</a> or type `lead and press &lt;enter&gt;.</font>";
        public const string GeroJoinBot = "<font color='query'><name shortname='gerolkae'>Gerolkae</name> requests permission to join your company. To accept the request, <a href='command://summon'>click here</a> or type `summon and press &lt;enter&gt;.</font>";
        public const string GeroLeadBot = "<font color='query'><name shortname='gerolkae'>Gerolkae</name> requests permission to lead you. To accept the request, <a href='command://follow'>click here</a> or type `follow and press &lt;enter&gt;.</font>";
        public const string GeroSummonBot = "<font color='query'><name shortname='gerolkae'>Gerolkae</name> asks you to join their company in <b>the dream of Silver|Monkey</b>. To accept the request, <a href='command://join'>click here</a> or type `join and press &lt;enter&gt;.</font>";

        #endregion Public Fields

        #region Private Fields

        private ProxySession Proxy;

        #endregion Private Fields

        #region Public Properties

        public string BackupSettingsFile { get; private set; }
        public ProxyOptions Options { get; private set; }
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

        public void BotHaseDisconnected_Standalone(bool StandAlone = false)
        {
            Proxy.Disconnect();
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

        [TestCase(GeroJoinBot, "join")]
        [TestCase(GeroFollowBot, "follow")]
        [TestCase(GeroLeadBot, "lead")]
        [TestCase(GeroSummonBot, "summon")]
        [TestCase(GeroCuddleBot, "cuddle")]
        public void ChannelIsQueryOfType(string ChannelCode, string ExpectedValue)
        {
            BotHasConnected_StandAlone();
            HaltFor(DreamEntranceDelay);

            Proxy.ProcessServerChannelData += delegate (object sender, ParseChannelArgs Args)
            {
                Assert.Multiple(() =>
                {
                    var ServeObject = (ChannelObject)sender;
                    Assert.That(Args.Channel,
                        Is.EqualTo("query"));
                });
            };

            Proxy.ParseServerChannel(ChannelCode, false);
            BotHaseDisconnected_Standalone();
        }

        [TearDown]
        public void Cleanup()
        {
            Proxy.ClientData2 -= (data) => Proxy.SendToServer(data);
            Proxy.ServerData2 -= (data) => Proxy.SendToClient(data);
            Proxy.Error -= (e, o) => Logger.Error($"{e} {o}");

            Proxy.Dispose();
            Options = null;
        }

        [TestCase(GeroJoinBot, "Gerolkae")]
        [TestCase(GeroFollowBot, "Gerolkae")]
        [TestCase(GeroLeadBot, "Gerolkae")]
        [TestCase(GeroSummonBot, "Gerolkae")]
        [TestCase(GeroCuddleBot, "Gerolkae")]
        public void ExpectedQueryCharacter(string testc, string ExpectedValue)
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
            BotHaseDisconnected_Standalone();
        }

        [SetUp]
        public void Initialize()
        {
            Furcadia.Logging.Logger.SingleThreaded = false;
            var CharacterFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "silvermonkey.ini");

            Options = new ProxyOptions()
            {
                Standalone = true,
                CharacterIniFile = CharacterFile,
            };

            Proxy = new ProxySession(Options);
            Proxy.ClientData2 += (data) => Proxy.SendToServer(data);
            Proxy.ServerData2 += (data) => Proxy.SendToClient(data);
            Proxy.Error += (e, o) => Logger.Error($"{e} {o}");
        }

        #endregion Public Methods
    }
}