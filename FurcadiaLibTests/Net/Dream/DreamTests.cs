using NUnit.Framework;
using Furcadia.Net.DreamInfo;

namespace FurcadiaLibTests.Net.Dreams
{
    [TestFixture]
    public class DreamTests
    {
        [Test]
        public void DreamIsNullcheck()
        {
            Furcadia.Net.DreamInfo.Dream dream = null;
            Assert.Multiple(() =>
            {
                Assert.That(dream, Is.EqualTo(null));
                dream = new Furcadia.Net.DreamInfo.Dream();
                Assert.That(dream, !Is.EqualTo(null));
            });
        }

        [Test]
        public void DreamVsDreamIsNull()
        {
            Furcadia.Net.DreamInfo.Dream Dream1 = null;
            Furcadia.Net.DreamInfo.Dream Dream2 = null;
            Assert.Multiple(() =>
            {
                Assert.That(Dream1, Is.EqualTo(Dream2), $"F1 {Dream1} == F2 {Dream2}");
                Dream1 = new Furcadia.Net.DreamInfo.Dream();
                Dream2 = new Furcadia.Net.DreamInfo.Dream();
                Assert.That(Dream1, Is.EqualTo(Dream2), $"F1 {Dream1} == F2 {Dream2}");
                Dream1 = null;
                Dream2 = new Furcadia.Net.DreamInfo.Dream();
                Assert.That(Dream1, !Is.EqualTo(Dream2), $"F1 {Dream1} != F2 {Dream2}");
                Dream1 = new Furcadia.Net.DreamInfo.Dream()
                {
                    DreamOwner = "Boo"
                };
                Dream2 = new Furcadia.Net.DreamInfo.Dream()
                {
                    DreamOwner = "BooBack"
                };
                Assert.That(Dream1, !Is.EqualTo(Dream2), $"F1 {Dream1} != F2 {Dream2}");

                Dream1 = new Furcadia.Net.DreamInfo.Dream()
                {
                    Title = "Boo"
                };
                Dream2 = new Furcadia.Net.DreamInfo.Dream()
                {
                    Title = "BooBack"
                };
                Assert.That(Dream1, !Is.EqualTo(Dream2), $"F1 {Dream1} != F2 {Dream2}");
                Dream1 = new Furcadia.Net.DreamInfo.Dream()
                {
                    DreamOwner = "Boo"
                };
                Dream2 = new Furcadia.Net.DreamInfo.Dream()
                {
                    DreamOwner = "boo"
                };
                Assert.That(Dream1, Is.EqualTo(Dream2), $"F1 {Dream1} == F2 {Dream2}");
                Dream1 = new Furcadia.Net.DreamInfo.Dream()
                {
                    Title = "Boo"
                };
                Dream2 = new Furcadia.Net.DreamInfo.Dream()
                {
                    Title = "boo"
                };
                Assert.That(Dream1, Is.EqualTo(Dream2), $"F1 {Dream1} == F2 {Dream2}");

                Dream1 = new Furcadia.Net.DreamInfo.Dream()
                {
                    DreamOwner = "Boo",
                    Title = "back"
                };
                Dream2 = new Furcadia.Net.DreamInfo.Dream()
                {
                    DreamOwner = "Boo",
                    Title = "back"
                };
                Assert.That(Dream1, Is.EqualTo(Dream2), $"F1 {Dream1} == F2 {Dream2}");

                Dream1 = new Furcadia.Net.DreamInfo.Dream()
                {
                    DreamOwner = "Boo",
                    Title = "back"
                };
                Dream2 = new Furcadia.Net.DreamInfo.Dream()
                {
                    DreamOwner = "Boo",
                    Title = "whoo"
                };
                Assert.That(Dream1, !Is.EqualTo(Dream2), $"F1 {Dream1} != F2 {Dream2}");

                Dream1 = new Furcadia.Net.DreamInfo.Dream()
                {
                    DreamOwner = "Good",
                    Title = "Who"
                };
                Dream2 = new Furcadia.Net.DreamInfo.Dream()
                {
                    DreamOwner = "Boo",
                    Title = "whoo"
                };
                Assert.That(Dream1, !Is.EqualTo(Dream2), $"F1 {Dream1} != F2 {Dream2}");

                Dream1 = new Furcadia.Net.DreamInfo.Dream()
                {
                    DreamOwner = "Good",
                    Title = "bye"
                };
                Dream2 = new Furcadia.Net.DreamInfo.Dream()
                {
                    DreamOwner = "Boo",
                    Title = "whoo"
                };
                Assert.That(Dream1, !Is.EqualTo(Dream2), $"F1 {Dream1} != F2 {Dream2}");
            });
        }
    }
}