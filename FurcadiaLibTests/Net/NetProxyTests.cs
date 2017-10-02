using NUnit.Framework;
using Furcadia.Net;
using FakeItEasy;
using NUnit;
using Furcadia.Net.Options;

namespace Furcadia.Net.Tests
{
    [TestFixture()]
    public class NetProxyTests
    {

        NetProxy Proxy;
        ProxyOptions options;

       void NetProxyInitialize()
        {

        }

        [SetUp]
        void SetUp()
        {
            options = new ProxyOptions()
            {
                CharacterIniFile = ""
            };
                
            Proxy = new NetProxy(ref options);
        }

        /// <summary>
        /// Constructor Logic?
        /// </summary>
        [Test()]
        public void NetProxyDefaultConstructorHasNoError()
        {


        }

     

        [Test()]
        public void ClientDisconnectTest()
        {
            Assert.Fail();
        }

        [Test()]
        public void CloseClientTest()
        {
            Assert.Fail();
        }

        /// <summary>
        /// check auto Port if Currently in use
        /// </summary>
        [Test()]
        public void ConnectTest()
        {
            Assert.Fail();
        }

        [Test()]
        public void DisconnectTest()
        {
            Assert.Fail();
        }

        [Test()]
        public void SendToClientTest()
        {
            Assert.Fail();
        }

        [Test()]
        public void SendToClientTest1()
        {
            Assert.Fail();
        }

        [Test()]
        public void SendToServerTest()
        {
            Assert.Fail();
        }

        [Test()]
        public void SendToServerTest1()
        {
            Assert.Fail();
        }

        [Test()]
        public void DisposeTest()
        {
            Assert.Fail();
        }

        [TearDown]
        public void TearDown()
        {

            Proxy.Dispose();
        }
    }
}