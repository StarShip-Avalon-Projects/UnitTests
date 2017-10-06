using NUnit.Framework;
using Furcadia.Net;
using FakeItEasy;
using NUnit;
using Furcadia.Net.Options;
using System;

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

        [Test()]
        public void AsyncListenerGetsDisposedExceptions()
        {
            var IAr = new Fake<IAsyncResult>();
            Assert.DoesNotThrow(() =>  Proxy.AsyncListener((IAsyncResult)IAr));
        }
            


        [Test()]
        [Ignore("Not there yet")]
        public void ClientDisconnectTest()
        {
            Assert.Fail();
        }

        [Test()]
        [Ignore("Not there yet")]
        public void CloseClientTest()
        {
            Assert.Fail();
        }

        /// <summary>
        /// check auto Port if Currently in use
        /// </summary>
        [Test()]
        [Ignore("Not there yet")]
        public void ConnectTest()
        {
            Assert.Fail();
        }

        [Test()]
        [Ignore("Not there yet")]
        public void DisconnectTest()
        {
            Assert.Fail();
        }

        [Test()]
        [Ignore("Not there yet")]
        public void SendToClientTest()
        {
            Assert.Fail();
        }

        [Test()]
        [Ignore("Not there yet")]
        public void SendToClientTest1()
        {
            Assert.Fail();
        }

        [Test()]
        [Ignore("Not there yet")]
        public void SendToServerTest()
        {
            Assert.Fail();
        }

        [Test()]
        [Ignore("Not there yet")]
        public void SendToServerTest1()
        {
            Assert.Fail();
        }

        [Test()]
        [Ignore("Not there yet")]
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