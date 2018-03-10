using Furcadia.IO;
using Furcadia.Logging;
using Furcadia.Net;
using Furcadia.Net.DirectConnection;
using Furcadia.Net.Options;
using Furcadia.Net.Utils.ChannelObjects;
using NUnit.Framework;
using System.IO;

namespace FurcadiaLibMono.Net.Proxy
{
    [TestFixture]
    public class FurcadiaQueryTests
    {
        #region Public Fields

        public const string GeroCuddleBot = "<font color='query'><name shortname='gerolkae'>Gerolkae</name> asks you to cuddle with them. To accept the request, <a href='command://cuddle'>click here</a> or type `cuddle and press &lt;enter&gt;.</font>";
        public const string JoeFollowBot = "<font color='query'><name shortname='joewilkins'>Joe Wilkins</name> requests permission to follow you. To accept the request, <a href='command://lead'>click here</a> or type `lead and press &lt;enter&gt;.</font>";
        public const string BillJoinBot = "<font color='query'><name shortname='billsanders'>Bill Sanders</name> requests permission to join your company. To accept the request, <a href='command://summon'>click here</a> or type `summon and press &lt;enter&gt;.</font>";
        public const string AngelLeadBot = "<font color='query'><name shortname='angel'>Angel</name> requests permission to lead you. To accept the request, <a href='command://follow'>click here</a> or type `follow and press &lt;enter&gt;.</font>";
        public const string ProteusSummonBot = "<font color='query'><name shortname='proteus'>Proteus</name> asks you to join their company in <b>the dream of Silver|Monkey</b>. To accept the request, <a href='command://join'>click here</a> or type `join and press &lt;enter&gt;.</font>";

        #endregion Public Fields

        #region Private Fields

        private NetConnection Client;

        #endregion Private Fields

        #region Public Properties

        public ClientOptions Options { get; private set; }

        #endregion Public Properties

        #region Public Methods

        public void ClientHasConnected()
        {
            Client.Connect();

            //  HaltFor(ConnectWaitTime);

            Assert.Multiple(() =>
            {
                Assert.That(Client.ServerStatus,
                    Is.EqualTo(ConnectionPhase.Connected),
                    $"Proxy.ServerStatus {Client.ServerStatus}");
                Assert.That(Client.IsServerSocketConnected,
                    Is.EqualTo(true),
                    $"Proxy.IsServerSocketConnected {Client.IsServerSocketConnected}");
            });
        }

        public void BotHaseDisconnected(bool StandAlone = true)
        {
            Client.Disconnect();

            // HaltFor(CleanupDelayTime);

            Assert.Multiple(() =>
            {
                Assert.That(Client.ServerStatus,
                     Is.EqualTo(ConnectionPhase.Disconnected),
                    $"Proxy.ServerStatus {Client.ServerStatus}");
                Assert.That(Client.IsServerSocketConnected,
                     Is.EqualTo(false),
                    $"Proxy.IsServerSocketConnected {Client.IsServerSocketConnected}");
            });
        }

        private object qLock = new object();

        [TestCase(BillJoinBot, QueryType.join, "Bill Sanders")]
        [TestCase(JoeFollowBot, QueryType.follow, "Joe Wilkins")]
        [TestCase(AngelLeadBot, QueryType.lead, "Angel")]
        [TestCase(ProteusSummonBot, QueryType.summon, "Proteus")]
        [TestCase(GeroCuddleBot, QueryType.cuddle, "Gerolkae")]
        public void ChannelIsQueryOfType(string ChannelCode, QueryType ExpectedValue, string ExpectedName)
        {
            lock (qLock)
            {
                Client.ProcessServerChannelData += (sender, Args) =>
                {
                    if (sender is QueryChannelObject queryObject)
                        Assert.Multiple(() =>
                        {
                            Assert.That(queryObject.Query,
                                Is.EqualTo(ExpectedValue), queryObject.ToString());
                            Assert.That(queryObject.Player.ShortName,
                                Is.EqualTo(ExpectedName.ToFurcadiaShortName()), queryObject.ToString());
                        });
                };

                Client.ParseServerChannel(ChannelCode, false);

                Client.ProcessServerChannelData -= (sender, Args) =>
                {
                    if (sender is QueryChannelObject queryObject)
                        Assert.Multiple(() =>
                        {
                            Assert.That(queryObject.Query,
                                Is.EqualTo(ExpectedValue), queryObject.ToString());
                            Assert.That(queryObject.Player.ShortName,
                                Is.EqualTo(ExpectedName.ToFurcadiaShortName()), queryObject.ToString());
                        });
                };
            }
        }

        [TearDown]
        public void Cleanup()
        {
            BotHaseDisconnected();

            Client.Error -= (e, o) => Logger.Error($"{e} {o}");

            Client.Dispose();
            Options = null;
        }

        [SetUp]
        public void Initialize()
        {
            var furcPath = new Paths();
#pragma warning disable CS0618 // Obsolete, Place holder till Accounts are ready
            var CharacterFile = Path.Combine(furcPath.CharacterPath,
#pragma warning restore CS0618 // Obsolete, Place holder till Accounts are ready
                  "silvermonkey.ini");

            Options = new ClientOptions()
            {
            };

            Client = new NetConnection(Options);

            Client.Error += (e, o) => Logger.Error($"{e} {o}");

            ClientHasConnected();
        }

        #endregion Public Methods
    }
}