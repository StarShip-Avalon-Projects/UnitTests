using Furcadia.IO;
using Furcadia.Logging;
using Furcadia.Drawing;
using NUnit.Framework;
using System.IO;
using static FurcadiaLibTests.Utilities;

namespace FurcadiaLibTests.Drawing
{
    /// <summary>
    /// Summary description for ViewAreaTests
    /// </summary>
    [TestFixture]
    public class ViewAreaTests
    {
        public ViewAreaTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        [TestCase(8, 9, 12, 14, true)]
        [TestCase(8, 9, 12, 30, false)]
        public void PostionIsInViewArea(int SourceX, int SourceY, int TargetX, int TargetY, bool EspectedResult)
        {
            ViewArea TargetArea = new ViewArea(SourceX, SourceY);
            FurrePosition TargetCoordinates = new FurrePosition(TargetX, TargetY);

            Assert.Multiple(() =>
            {
                Assert.That(TargetArea.InRange(TargetCoordinates),
                    Is.EqualTo(EspectedResult), $"TargetArea {TargetArea}");
            });
        }
    }
}