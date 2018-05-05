using Furcadia.IO;
using Furcadia.Logging;
using Furcadia.Net.DirectConnection;
using Furcadia.Net.DreamInfo;
using Furcadia.Net.Options;
using Furcadia.Net.Utils.ServerParser;
using NUnit.Framework;
using System;
using System.IO;

//using static Libraries.MsLibHelper;
//using static SmEngineTests.Utilities;

namespace FurcadiaLibTests.Net.NoConnection
{
    [TestFixture]
    public class DirectConnectionChannelParser
    {
        private const string GeroSayPing = "<name shortname='gerolkae'>Gerolkae</name>: ping";
        private const string GeroWhisperCunchatize = "<font color='whisper'>[ <name shortname='gerolkae' src='whisper-from'>Gerolkae</name> whispers, \"crunchatize\" to you. ]</font>";
        private const string GeroWhisperRollOut = "<font color='whisper'>[ <name shortname='gerolkae' src='whisper-from'>Gerolkae</name> whispers, \"roll out\" to you. ]</font>";

        // private const string PingTest = @"<name shortname='gerolkae'>Gerolkae</name>: ping";
        private const string WhisperTest = "<font color='whisper'>[ <name shortname='gerolkae' src='whisper-from'>Gerolkae</name> whispers, \"hi\" to you. ]</font>";

        // private const string PingTest2 = @"<name shortname='gerolkae'>Gerolkae</name>: Ping";
        private const string GeroWhisperHi = "<font color='whisper'>[ <name shortname='gerolkae' src='whisper-from'>Gerolkae</name> whispers, \"Hi\" to you. ]</font>";

        //  private const string YouWhisper = "<font color='whisper'>[You whisper \"Logged on\" to<name shortname='gerolkae' forced src='whisper-to'>Gerolkae</name>. ]</font>";
        private const string YouWhisper2 = "<font color='whisper'>[ You whisper \"Logged on\" to <name shortname='gerolkae' forced src='whisper-to'>Gerolkae</name>. ]</font>";

        private const string YouShoutYo = "<font color='shout'>You shout, \"Yo Its Me\"</font>";
        private const string GeroShout = "<font color='shout'>{S} <name shortname='gerolkae'>Gerolkae</name> shouts: ping</font>";
        private const string Emote = "<font color='emote'><name shortname='silvermonkey'>Silver|Monkey</name> Emoe</font>";

        private const string Emit = "<font color='dragonspeak'><img src='fsh://system.fsh:91' alt='@emit' /><channel name='@emit' /> Silver|Monkey has arrived...</font>";
        private const string EmitTest = "<font color='emit'><img src='fsh://system.fsh:91' alt='@emit' /><channel name='@emit' /> test</font>";
        private const string EmitBlah = "<font color='emit'><img src='fsh://system.fsh:91' alt='@emit' /><channel name='@emit' /> Blah</font>";
        private const string EmitWarning = "<font color='warning'><img src='fsh://system.fsh:91' alt='@emit' /><channel name='@emit' /> (<name shortname='silvermonkey'>Silver|Monkey</name> just emitted.)</font>";

        private const string CookieBank = "<font color='emit'><img src='fsh://system.fsh:90' alt='@cookie' /><channel name='@cookie' /> Cookie <a href='http://www.furcadia.com/cookies/Cookie%20Economy.html'>bank</a> has currently collected: 0</font>";
        private NetConnection Client;

        public string SettingsFile { get; private set; }
        public string BackupSettingsFile { get; private set; }
        public ClientOptions options { get; private set; }
        private Paths FurcPath = new Paths();

        [SetUp]
        public void Initialize()
        {
            var MsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "Bugreport 165 From Jake.ms");
            var CharacterFile = Path.Combine(FurcPath.CharacterPath,
                "dbugger.ini");

            options = new ClientOptions()
            {
            };

            Client = new NetConnection(options);
            // Set Dream info and Furre List
            PopulateFurreList();
            Client.Error += (e, o) => Logger.Error($"{e} {o}");
        }

        private void PopulateFurreList()
        {
            Client.Furres.Add(new Furre(1, "John"));
            Client.Furres.Add(new Furre(2, "Bill Nye"));
            Client.Furres.Add(new Furre(3, "John More"));
            Client.Furres.Add(new Furre(4, "Silver Monkey"));
            Client.Furres.Add(new Furre(5, "Gerolkae"));

            Client.ConnectedFurreId = 4;
            Client.ConnectedFurreName = "Silver Monkey";
        }

        [TestCase(WhisperTest, "hi")]
        [TestCase(GeroWhisperHi, "Hi")]
        [TestCase(YouShoutYo, "Yo Its Me")]
        [TestCase(GeroShout, "ping")]
        [TestCase(EmitWarning, "(<name shortname='silvermonkey'>Silver|Monkey</name> just emitted.)")]
        [TestCase(Emit, "Silver|Monkey has arrived...")]
        [TestCase(EmitBlah, "Blah")]
        [TestCase(Emote, "Emoe")]
        [TestCase(EmitTest, "test")]
        public void ChannelTextIs(string testc, string ExpectedValue)
        {
            Client.ProcessServerChannelData += (sender, Args) =>
            {
                if (sender is ChannelObject ServeObject)
                {
                    Assert.That(ServeObject.Player.Message.Trim(),
                        Is.EqualTo(ExpectedValue.Trim()),
                        $"Player.Message '{ServeObject.Player.Message}' ExpectedValue: {ExpectedValue}"
                        );
                }
            };

            Client.ParseServerChannel(testc, false);

            Client.ProcessServerChannelData -= (sender, Args) =>
            {
                if (sender is ChannelObject ServeObject)
                {
                    Assert.That(ServeObject.Player.Message.Trim(),
                        Is.EqualTo(ExpectedValue.Trim()),
                        $"Player.Message '{ServeObject.Player.Message}' ExpectedValue: {ExpectedValue}"
                        );
                }
            };
        }

        [TestCase(WhisperTest, "whisper", "Gerolkae")]
        [TestCase(YouWhisper2, "whisper")]
        [TestCase(GeroShout, "shout", "Gerolkae")]
        [TestCase(YouShoutYo, "shout")]
        [TestCase(EmitWarning, "@emit", "Silver Monkey")]
        [TestCase(Emit, "@emit", "Furcadia Game Server")]
        [TestCase(EmitBlah, "@emit", "Furcadia Game Server")]
        [TestCase(Emote, "emote", "Silver Monkey")]
        [TestCase(GeroWhisperHi, "whisper", "Gerolkae")]
        public void ExpectedCharachter(string testc, string channel, string ExpectedValue = "you")
        {
            Client.ProcessServerChannelData += (sender, Args) =>
            {
                if (sender is ChannelObject ServeObject)
                {
                    Assert.That(Args.Channel, Is.EqualTo(channel), $"Channel = Args:'{Args.Channel}' Expected:'{ channel}'");
                    if (ExpectedValue == "you")
                    {
                        Assert.That(ServeObject.Player,
                            Is.EqualTo(Client.ConnectedFurre));
                        Assert.That(Client.Furres.Contains(ServeObject.Player));
                    }
                    else
                    {
                        Assert.That(ServeObject.Player.ShortName,
                            Is.EqualTo(ExpectedValue.ToFurcadiaShortName()));
                        if (ServeObject.Player.ShortName != "furcadiagameserver")
                            Assert.That(Client.Furres.Contains(ServeObject.Player));
                    }
                }
            };

            Client.ParseServerChannel(testc, false);

            Client.ProcessServerChannelData -= (sender, Args) =>
            {
                if (sender is ChannelObject ServeObject)
                {
                    Assert.That(Args.Channel, Is.EqualTo(channel), $"Channel = Args:'{Args.Channel}' Expected:'{ channel}'");
                    if (ExpectedValue == "you")
                    {
                        Assert.That(ServeObject.Player,
                            Is.EqualTo(Client.ConnectedFurre));
                        Assert.That(Client.Furres.Contains(ServeObject.Player));
                    }
                    else
                    {
                        Assert.That(ServeObject.Player.ShortName,
                            Is.EqualTo(ExpectedValue.ToFurcadiaShortName()));
                        if (ServeObject.Player.ShortName != "furcadiagameserver" && Args.Channel != "@emit")
                            Assert.That(Client.Furres.Contains(ServeObject.Player));
                    }
                }
            };
        }

        [TestCase(GeroShout, "ping")]
        public void DirectConnection_InstructionObjectPlayerIs(string testc, string ExpectedValue)
        {
            //Turn the channel on
            Client.SendFormattedTextToServer("- Shout");

            Client.ProcessServerChannelData += (sender, Args) =>
            {
                if (sender is ChannelObject ServeObject)
                {
                    Assert.That(ServeObject.Player.Message.Trim(),
                        Is.EqualTo(ExpectedValue.Trim()));
                }
            };

            Client.ParseServerChannel(testc, false);

            Client.ProcessServerChannelData -= (sender, Args) =>
            {
                if (sender is ChannelObject ServeObject)
                {
                    Assert.That(ServeObject.Player.Message.Trim(),
                        Is.EqualTo(ExpectedValue.Trim()));
                }
            };
        }

        [TestCase("]q pmvinca 3041088975   modern", true, true, "pmvinca")]
        public void LoadDreamSettingsTest(string ServerInstruction, bool IsPermament, bool IsModern, string ExpectedValue)
        {
            LoadDream loadDream = new LoadDream(ServerInstruction);
            Client.Dream.Load(loadDream);
            Assert.Multiple(() =>
            {
                Assert.That(Client.Dream.FileName, Is.EqualTo(ExpectedValue), $"Drean Cache file{ Client.Dream.FileName}");
                Assert.That(Client.Dream.IsModern, Is.EqualTo(IsModern), $"IsModern { Client.Dream.IsModern}");
                Assert.That(Client.Dream.IsPermanent, Is.EqualTo(IsPermament), $"IsPermament { Client.Dream.IsPermanent}");
            });
        }

        //(You enter the dream of Silver|Monkey.)

        [TestCase("]C0furc://silvermonkey:stargatebase/", "Silver Monkey", "Stargate Base")]
        [TestCase("]C0furc://imaginarium/", "Imaginarium")]
        [TestCase("]C0furc://vinca/", "Vinca")]
        public void DreamBookmarkSettingsTest(string ServerInstruction, string DreamOwner, string DreamTitle = "")
        {
            DreamBookmark bookmark = new DreamBookmark(ServerInstruction);
            Client.Dream.BookMark = bookmark;
            Logger.Debug(bookmark);
            Assert.Multiple(() =>
            {
                Assert.That(Client.Dream.Title, Is.EqualTo(DreamTitle.ToFurcadiaShortName()), $"Dream Owner: { Client.Dream.Title}");
                Assert.That(Client.Dream.DreamOwner, Is.EqualTo(DreamOwner.ToFurcadiaShortName()), $"Dream Title: { Client.Dream.DreamOwner}");
                if (string.IsNullOrWhiteSpace(DreamTitle))
                    Assert.That(Client.Dream.DreamUrl, Is.EqualTo($"furc://{DreamOwner.ToFurcadiaShortName()}/"), $"Dream URL { Client.Dream.DreamUrl}");
                else
                    Assert.That(Client.Dream.DreamUrl, Is.EqualTo($"furc://{DreamOwner.ToFurcadiaShortName()}:{DreamTitle.ToFurcadiaShortName()}/"), $"Dream URL { Client.Dream.DreamUrl}");
            });
        }

        [TearDown]
        public void Cleanup()
        {
            Client.Error -= (e, o) => Logger.Error($"{e} {o}");
            Client.Dispose();
            options = null;
        }
    }
}