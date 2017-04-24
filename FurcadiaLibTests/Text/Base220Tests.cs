using NUnit.Framework;
using static Furcadia.Text.Base220;

namespace FurcadiaLibTests.Text
{
    [TestFixture]
    public class Base220Tests
    {
        #region Public Methods

        [TestCase(@"/Hello world!", 12)]
        [TestCase(@"/Hello world! Pft", 12)]
        public void Base220StringLengeth_Returns_StringLengeth_And_Outputs_ReferenceString(string input, int ExpectedResult)
        {
            int length = Base220StringLengeth(ref input);
        }

        #endregion Public Methods
    }
}