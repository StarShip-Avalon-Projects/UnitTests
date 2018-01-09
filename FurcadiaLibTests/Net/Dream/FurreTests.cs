using System;
using Furcadia.Net.DreamInfo;
using NUnit.Framework;

namespace FurcadiaLibTests.Net.Dream
{
    [TestFixture]
    public class FurreTests
    {
        [Test]
        public void FurreIsNull()
        {
            Furre Furre1 = null;
            Assert.Multiple(() =>
            {
                Assert.That(Furre1, Is.EqualTo(null));
                Furre1 = new Furre();
                Assert.That(Furre1, !Is.EqualTo(null));
            });
        }

        [Test]
        public void FurreVsFurreIsNull()
        {
            Furre Furre1 = null;
            Furre Furre2 = null;
            Assert.Multiple(() =>
            {
                Assert.That(Furre1, Is.EqualTo(Furre2), $"F1 {Furre1} == F2 {Furre2}");
                Furre1 = new Furre();
                Furre2 = new Furre();
                Assert.That(Furre1, Is.EqualTo(Furre2), $"F1 {Furre1} == F2 {Furre2}");
                Furre1 = null;
                Furre2 = new Furre();
                Assert.That(Furre1, !Is.EqualTo(Furre2), $"F1 {Furre1} != F2 {Furre2}");
                Furre1 = new Furre();
                Furre2 = null;
                Assert.That(Furre1, !Is.EqualTo(Furre2), $"F1 {Furre1} != F2 {Furre2}");

                Furre1 = new Furre(5, "joe");
                Furre2 = new Furre(5, "joe");
                Assert.That(Furre1, Is.EqualTo(Furre2), $"F1 {Furre1} == F2 {Furre2}");
                Furre1 = new Furre(5, "joe");
                Furre2 = new Furre(5);
                Assert.That(Furre1, Is.EqualTo(Furre2), $"F1 {Furre1} == F2 {Furre2}");
                Furre1 = new Furre(5, "joe");
                Furre2 = new Furre(6);
                Assert.That(Furre1, !Is.EqualTo(Furre2), $"F1 {Furre1} != F2 {Furre2}");
            });
        }
    }
}