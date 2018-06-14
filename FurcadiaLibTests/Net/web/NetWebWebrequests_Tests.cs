using Furcadia.Net.Web;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace FurcadiaLibTests.Net.web
{
    [TestFixture]
    internal class NetWebWebrequests_Tests
    {
        [SetUp]
        public void Initialize()
        {
        }

        private WebRequests WebRequest;

        private void WebRequestsInitialize()
        {
            WebRequest = new WebRequests();
        }

        //  [TestCase("https://silvermonkey.tsprojects.org/postecho.php")]
        //  [TestCase("http://silvermonkey.tsprojects.org/postecho.php")]
        public void WebRequest_IsNotNull(string url)
        {
            WebRequestsInitialize();
            List<IVariable> Variables = new List<IVariable>
            {
                new Variable("Hello", "there"),
                new Variable("meep")
            };

            Console.WriteLine(WebRequests.SendPostRequest(WebUtils.PrepWebData(Variables), url));
        }
    }
}