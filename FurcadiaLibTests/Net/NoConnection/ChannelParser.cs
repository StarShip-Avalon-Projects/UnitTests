using Furcadia.IO;
using Furcadia.Logging;
using Furcadia.Net.DreamInfo;
using Furcadia.Net.Options;
using Furcadia.Net.Proxy;
using Furcadia.Net.Utils.ServerParser;
using NUnit.Framework;
using System;
using System.IO;

//using static Libraries.MsLibHelper;
//using static SmEngineTests.Utilities;

namespace FurcadiaLibTests.Net.NoConnection
{
    [TestFixture]
    public class ProxyChannelParser
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
        private const string OocEmote = "<font color='emote'><name shortname='solariastarwisp'>Solaria|Starwisp</name>[Test]</font>";
        private const string Emit = "<font color='dragonspeak'><img src='fsh://system.fsh:91' alt='@emit' /><channel name='@emit' /> Silver|Monkey has arrived...</font>";
        private const string EmitTest = "<font color='emit'><img src='fsh://system.fsh:91' alt='@emit' /><channel name='@emit' /> test</font>";
        private const string EmitBlah = "<font color='emit'><img src='fsh://system.fsh:91' alt='@emit' /><channel name='@emit' /> Blah</font>";
        private const string EmitWarning = "<font color='warning'><img src='fsh://system.fsh:91' alt='@emit' /><channel name='@emit' /> (<name shortname='silvermonkey'>Silver|Monkey</name> just emitted.)</font>";

        private const string CookieBank = "<font color='emit'><img src='fsh://system.fsh:90' alt='@cookie' /><channel name='@cookie' /> Cookie <a href='http://www.furcadia.com/cookies/Cookie%20Economy.html'>bank</a> has currently collected: 0</font>";
        private ProxySession Proxy;

        public string SettingsFile { get; private set; }
        public string BackupSettingsFile { get; private set; }
        public ProxyOptions options { get; private set; }
        private Paths FurcPath = new Paths();

        [SetUp]
        public void Initialize()
        {
            var MsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "Bugreport 165 From Jake.ms");
            var CharacterFile = Path.Combine(FurcPath.CharacterPath,
                "dbugger.ini");

            options = new ProxyOptions()
            {
                Standalone = true,
                CharacterIniFile = CharacterFile,
                ResetSettingTime = 10
            };

            Proxy = new ProxySession(options);
            // Set Dream info and Furre List
            PopulateFurreList();
            Proxy.Error += (e, o) => Logger.Error($"{e} {o}");
        }

        private void PopulateFurreList()
        {
            Proxy.Dream.Furres.Add(new Furre(1, "John"));
            Proxy.Dream.Furres.Add(new Furre(2, "Bill Nye"));
            Proxy.Dream.Furres.Add(new Furre(3, "John More"));
            Proxy.Dream.Furres.Add(new Furre(4, "Silver Monkey"));
            Proxy.Dream.Furres.Add(new Furre(5, "Gerolkae"));
            Proxy.Dream.Furres.Add(new Furre(6, "Solaria|Starwisp"));

            Proxy.ConnectedFurreName = "Silver|Monkey";
            Proxy.ConnectedFurreId = 4;
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
            Proxy.ProcessServerChannelData += (sender, Args) =>
            {
                if (sender is ChannelObject ServeObject)
                {
                    Assert.That(ServeObject.Player.Message.Trim(),
                        Is.EqualTo(ExpectedValue.Trim()));
                }
            };

            Proxy.ParseServerChannel(testc, false);

            Proxy.ProcessServerChannelData -= (sender, Args) =>
            {
                if (sender is ChannelObject ServeObject)
                {
                    Assert.That(ServeObject.Player.Message.Trim(),
                        Is.EqualTo(ExpectedValue.Trim()));
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
        [TestCase(OocEmote, "emote", "Solaria|Starwisp")]
        public void ExpectedCharachter(string testc, string channel, string ExpectedValue = "you")
        {
            Proxy.ProcessServerChannelData += (sender, Args) =>
            {
                if (sender is ChannelObject ServeObject)
                {
                    Assert.That(Args.Channel, Is.EqualTo(channel), $"Channel = Args:'{Args.Channel}' Expected:'{ channel}'");
                    if (ExpectedValue == "you")
                    {
                        Assert.That(ServeObject.Player, Is.EqualTo(Proxy.ConnectedFurre));
                        Assert.That(Proxy.Dream.Furres.Contains(ServeObject.Player), $"Furre List doesn't contain '{ServeObject.Player}'");
                    }
                    else
                    {
                        Assert.That(ServeObject.Player.ShortName,
                            Is.EqualTo(ExpectedValue.ToFurcadiaShortName()));
                        if (ServeObject.Player.ShortName != "furcadiagameserver" && Args.Channel != "@emit")
                            Assert.That(Proxy.Dream.Furres.Contains(ServeObject.Player), $"Furre List doesn't contain '{ServeObject.Player}'");
                    }
                }
            };

            Proxy.ParseServerChannel(testc, false);

            Proxy.ProcessServerChannelData -= (sender, Args) =>
            {
                if (sender is ChannelObject ServeObject)
                {
                    Assert.That(Args.Channel, Is.EqualTo(channel), $"Channel = Args:'{Args.Channel}' Expected:'{ channel}'");
                    if (ExpectedValue == "you")
                    {
                        Assert.That(ServeObject.Player,
                            Is.EqualTo(Proxy.ConnectedFurre));
                        Assert.That(Proxy.Dream.Furres.Contains(ServeObject.Player), $"Furre List doesn't contain '{ServeObject.Player}'");
                    }
                    else
                    {
                        Assert.That(ServeObject.Player.ShortName,
                            Is.EqualTo(ExpectedValue.ToFurcadiaShortName()));
                        if (ServeObject.Player.ShortName != "furcadiagameserver" && Args.Channel != "@emit")
                            Assert.That(Proxy.Dream.Furres.Contains(ServeObject.Player), $"Furre List doesn't contain '{ServeObject.Player}'");
                    }
                }
            };
        }

        [TestCase(GeroShout, "ping")]
        public void ChannelParserNoConnectionInstructionObjectPlayerIs(string testc, string ExpectedValue)
        {
            //Turn the channel on
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

        [TestCase("]q pmvinca 3041088975   modern", true, true, "pmvinca")]
        public void LoadDreamSettingsTest(string ServerInstruction, bool IsPermament, bool IsModern, string ExpectedValue)
        {
            LoadDream loadDream = new LoadDream(ServerInstruction);
            Proxy.Dream.Load(loadDream);
            Assert.Multiple(() =>
            {
                Assert.That(Proxy.Dream.FileName, Is.EqualTo(ExpectedValue), $"Drean Cache file{ Proxy.Dream.FileName}");
                Assert.That(Proxy.Dream.IsModern, Is.EqualTo(IsModern), $"IsModern { Proxy.Dream.IsModern}");
                Assert.That(Proxy.Dream.IsPermanent, Is.EqualTo(IsPermament), $"IsPermament { Proxy.Dream.IsPermanent}");
            });
        }

        //(You enter the dream of Silver|Monkey.)

        [TestCase("]C0furc://silvermonkey:stargatebase/", "Silver Monkey", "Stargate Base")]
        [TestCase("]C0furc://imaginarium/", "Imaginarium")]
        [TestCase("]C0furc://vinca/", "Vinca")]
        public void DreamBookmarkSettingsTest(string ServerInstruction, string DreamOwner, string DreamTitle = "")
        {
            DreamBookmark bookmark = new DreamBookmark(ServerInstruction);
            Proxy.Dream.BookMark = bookmark;
            Logger.Debug(bookmark);
            Assert.Multiple(() =>
            {
                Assert.That(Proxy.Dream.Title, Is.EqualTo(DreamTitle.ToFurcadiaShortName()), $"Dream Owner: { Proxy.Dream.Title}");
                Assert.That(Proxy.Dream.DreamOwner, Is.EqualTo(DreamOwner.ToFurcadiaShortName()), $"Dream Title: { Proxy.Dream.DreamOwner}");
                if (string.IsNullOrWhiteSpace(DreamTitle))
                    Assert.That(Proxy.Dream.DreamUrl, Is.EqualTo($"furc://{DreamOwner.ToFurcadiaShortName()}/"), $"Dream URL { Proxy.Dream.DreamUrl}");
                else
                    Assert.That(Proxy.Dream.DreamUrl, Is.EqualTo($"furc://{DreamOwner.ToFurcadiaShortName()}:{DreamTitle.ToFurcadiaShortName()}/"), $"Dream URL { Proxy.Dream.DreamUrl}");
            });
        }

        [TearDown]
        public void Cleanup()
        {
            Proxy.Error -= (e, o) => Logger.Error($"{e} {o}");
            Proxy.Dispose();
            options = null;
        }
    }
}