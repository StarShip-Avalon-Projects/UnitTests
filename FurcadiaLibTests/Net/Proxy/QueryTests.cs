﻿using Furcadia.IO;
using Furcadia.Logging;
using Furcadia.Net;
using Furcadia.Net.Options;
using Furcadia.Net.Proxy;
using Furcadia.Net.Utils.ChannelObjects;
using Furcadia.Net.Utils.ServerParser;
using NUnit.Framework;
using System;
using System.IO;
using static FurcadiaLibTests.Utilities;

namespace FurcadiaLibTests.Net.Proxy
{
    [TestFixture]
    [NonParallelizable]
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

        private ProxySession Proxy;

        #endregion Private Fields

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

            //  HaltFor(ConnectWaitTime);

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

        public void BotHaseDisconnected(bool StandAlone = true)
        {
            Proxy.Disconnect();
            //if (!Proxy.StandAlone)
            //    HaltFor(CleanupDelayTime);

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

        private object qLock = new object();

        [NonParallelizable]
        [TestCase(BillJoinBot, QueryType.join, "Bill Sanders")]
        [TestCase(JoeFollowBot, QueryType.follow, "Joe Wilkins")]
        [TestCase(AngelLeadBot, QueryType.lead, "Angel")]
        [TestCase(ProteusSummonBot, QueryType.summon, "Proteus")]
        [TestCase(GeroCuddleBot, QueryType.cuddle, "Gerolkae")]
        public void ChannelIsQueryOfType(string ChannelCode, QueryType ExpectedValue, string ExpectedName)
        {
            lock (qLock)
            {
                bool isTested = false;
                Proxy.ProcessServerChannelData += (sender, Args) =>
                {
                    if (!isTested && sender is QueryChannelObject queryObject)
                    {
                        isTested = true;
                        Assert.Multiple(() =>
                        {
                            Assert.That(queryObject.Query,
                                Is.EqualTo(ExpectedValue), queryObject.ToString());
                            Assert.That(queryObject.Player.ShortName,
                                Is.EqualTo(ExpectedName.ToFurcadiaShortName()), queryObject.ToString());
                        });
                    }
                };

                Proxy.ParseServerChannel(ChannelCode, false);

                Proxy.ProcessServerChannelData -= (sender, Args) =>
                {
                    if (!isTested && sender is QueryChannelObject queryObject)
                    {
                        isTested = true;
                        Assert.Multiple(() =>
                        {
                            Assert.That(queryObject.Query,
                                Is.EqualTo(ExpectedValue), queryObject.ToString());
                            Assert.That(queryObject.Player.ShortName,
                                Is.EqualTo(ExpectedName.ToFurcadiaShortName()), queryObject.ToString());
                        });
                    }
                };
            }
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            BotHaseDisconnected();
            Proxy.ClientData2 -= (data) => Proxy.SendToServer(data);
            Proxy.ServerData2 -= (data) => Proxy.SendToClient(data);
            Proxy.Error -= (e, o) => Logger.Error($"{e} {o}");

            Proxy.Dispose();
            Options = null;
        }

        [OneTimeSetUp]
        public void Initialize()
        {
            var furcPath = new Paths();
#pragma warning disable CS0618 // Obsolete, Place holder till Accounts are ready
            var CharacterFile = Path.Combine(furcPath.CharacterPath,
#pragma warning restore CS0618 // Obsolete, Place holder till Accounts are ready
                  "silvermonkey.ini");

            Options = new ProxyOptions()
            {
                Standalone = true,
                CharacterIniFile = CharacterFile,
                ResetSettingTime = 10
            };

            Proxy = new ProxySession(Options);
            Proxy.ClientData2 += (data) => Proxy.SendToServer(data);
            Proxy.ServerData2 += (data) => Proxy.SendToClient(data);
            Proxy.Error += (e, o) => Logger.Error($"{e} {o}");

            BotHasConnected();
        }

        #endregion Public Methods
    }
}