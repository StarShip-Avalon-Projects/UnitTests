using NUnit.Framework;
using Furcadia.Net.DreamInfo;
using Furcadia.Net.Utils.ServerParser;
using static FurcadiaLibTests.Utilities;
using Furcadia.Logging;

namespace FurcadiaLibTests.Net.Dreams
{
    // TODO: Test dream URL's and dream settings
    // LoadDream
    // dreambookmark
    // furc://imaginarium/
    // Dream Owner = Furc game server
    // %DREAMOWNER
    // %DREAMNAME
    // and dream name is
    // and dream name is not
    // You enter the dream of [FURRE]
    [TestFixture]
    public class DreamTests
    {
        [SetUp]
        public void Initialize()
        {
        }

        [Test]
        public void DreamIsNullcheck()
        {
            Dream dream = null;
            Assert.Multiple(() =>
            {
                Assert.That(dream, Is.EqualTo(null));
                dream = new Dream();
                Assert.That(dream, !Is.EqualTo(null));
            });
        }

        [TestCase("]q pmvinca 3041088975   modern", true, true, "pmvinca")]
        public void LoadDreamSettingsTest(string ServerInstruction, bool IsPermament, bool IsModern, string ExpectedValue)
        {
            Dream TestDream = new Dream();
            LoadDream loadDream = new LoadDream(ServerInstruction);
            TestDream.Load(loadDream);
            Assert.Multiple(() =>
            {
                Assert.That(TestDream.FileName, Is.EqualTo(ExpectedValue), $"Drean Cache file{TestDream.FileName}");
                Assert.That(TestDream.IsModern, Is.EqualTo(IsModern), $"IsModern {TestDream.IsModern}");
                Assert.That(TestDream.IsPermanent, Is.EqualTo(IsPermament), $"IsPermament {TestDream.IsPermanent}");
            });
        }

        //(You enter the dream of Silver|Monkey.)

        [TestCase("]C0furc://silvermonkey:stargatebase/", "Silver Monkey", "Stargate Base")]
        [TestCase("]C0furc://imaginarium/", "Imaginarium", "")]
        [TestCase("]C0furc://vinca/", "Vinca", "")]
        public void DreamBookmarkSettingsTest(string ServerInstruction, string DreamOwner, string DreamTitle)
        {
            Dream TestDream = new Dream();
            DreamBookmark bookmark = new DreamBookmark(ServerInstruction);
            TestDream.BookMark = bookmark;
            Logger.Debug(bookmark);
            Assert.Multiple(() =>
            {
                Assert.That(TestDream.Title, Is.EqualTo(DreamTitle.ToFurcadiaShortName()), $"Dream Owner: {TestDream.Title}");
                Assert.That(TestDream.DreamOwner, Is.EqualTo(DreamOwner.ToFurcadiaShortName()), $"Dream Title: {TestDream.DreamOwner}");
                if (string.IsNullOrWhiteSpace(DreamTitle))
                    Assert.That(TestDream.DreamUrl, Is.EqualTo($"furc://{DreamOwner.ToFurcadiaShortName()}/"), $"Dream URL {TestDream.DreamUrl}");
                else
                    Assert.That(TestDream.DreamUrl, Is.EqualTo($"furc://{DreamOwner.ToFurcadiaShortName()}:{DreamTitle.ToFurcadiaShortName()}/"), $"Dream URL {TestDream.DreamUrl}");
            });
        }

        [Test]
        public void DreamVsDreamIsNull()
        {
            Dream Dream1 = null;
            Dream Dream2 = null;
            Assert.Multiple(() =>
            {
                Assert.That(Dream1, Is.EqualTo(Dream2), $"F1 {Dream1} == F2 {Dream2}");
                Dream1 = new Dream();
                Dream2 = new Dream();
                Assert.That(Dream1, Is.EqualTo(Dream2), $"F1 {Dream1} == F2 {Dream2}");
                Dream1 = null;
                Dream2 = new Dream();
                Assert.That(Dream1, !Is.EqualTo(Dream2), $"F1 {Dream1} != F2 {Dream2}");
                Dream1 = new Dream()
                {
                    DreamOwner = "Boo"
                };
                Dream2 = new Dream()
                {
                    DreamOwner = "BooBack"
                };
                Assert.That(Dream1, !Is.EqualTo(Dream2), $"F1 {Dream1} != F2 {Dream2}");

                Dream1 = new Dream()
                {
                    Title = "Boo"
                };
                Dream2 = new Dream()
                {
                    Title = "BooBack"
                };
                Assert.That(Dream1, !Is.EqualTo(Dream2), $"F1 {Dream1} != F2 {Dream2}");
                Dream1 = new Dream()
                {
                    DreamOwner = "Boo"
                };
                Dream2 = new Dream()
                {
                    DreamOwner = "Boo"
                };
                Assert.That(Dream1, Is.EqualTo(Dream2), $"F1 {Dream1} == F2 {Dream2}");
                Dream1 = new Dream()
                {
                    Title = "Boo"
                };
                Dream2 = new Dream()
                {
                    Title = "Boo"
                };
                Assert.That(Dream1, Is.EqualTo(Dream2), $"F1 {Dream1} == F2 {Dream2}");

                Dream1 = new Dream()
                {
                    DreamOwner = "Boo",
                    Title = "back"
                };
                Dream2 = new Dream()
                {
                    DreamOwner = "Boo",
                    Title = "back"
                };
                Assert.That(Dream1, Is.EqualTo(Dream2), $"F1 {Dream1} == F2 {Dream2}");

                Dream1 = new Dream()
                {
                    DreamOwner = "Boo",
                    Title = "back"
                };
                Dream2 = new Dream()
                {
                    DreamOwner = "Boo",
                    Title = "whoo"
                };
                Assert.That(Dream1, !Is.EqualTo(Dream2), $"F1 {Dream1} != F2 {Dream2}");

                Dream1 = new Dream()
                {
                    DreamOwner = "Good",
                    Title = "Who"
                };
                Dream2 = new Dream()
                {
                    DreamOwner = "Boo",
                    Title = "whoo"
                };
                Assert.That(Dream1, !Is.EqualTo(Dream2), $"F1 {Dream1} != F2 {Dream2}");

                Dream1 = new Dream()
                {
                    DreamOwner = "Good",
                    Title = "bye"
                };
                Dream2 = new Dream()
                {
                    DreamOwner = "Boo",
                    Title = "whoo"
                };
                Assert.That(Dream1, !Is.EqualTo(Dream2), $"F1 {Dream1} != F2 {Dream2}");
            });
        }
    }
}