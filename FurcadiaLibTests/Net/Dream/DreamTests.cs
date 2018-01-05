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
    }
}