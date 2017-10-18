using Furcadia.Net.Options;
using Furcadia.Net.Proxy;
using NUnit.Framework;

namespace Furcadia.Net.Tests
{
    [TestFixture()]
    public class NetProxyTests
    {
        private NetProxy Proxy;

        private NetProxy NetProxyInitialize()
        {
            var options = new ProxySessionOptions()
            {
                CharacterIniFile = ""
            };
            var proxy = new ProxySession(ref options);

            return proxy;
        }

        /// <summary>
        /// Constructor Logic?
        /// </summary>
        [Test()]
        public void NetProxyDefaultConstructorHasNoError()
        {
            Proxy = NetProxyInitialize();
        }
    }
}