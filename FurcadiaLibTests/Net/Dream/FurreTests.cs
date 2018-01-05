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
    }
}