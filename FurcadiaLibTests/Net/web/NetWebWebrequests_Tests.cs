using Furcadia.Net.Web;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using static FurcadiaLibTests.Utilities;

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

        [TestCase("https://silvermonkey.tsprojects.org/postecho.php")]
        [TestCase("http://silvermonkey.tsprojects.org/postecho.php")]
        public void WebRequest_IsNotNull(string url)
        {
            WebRequestsInitialize();
            List<IVariable> Variables = new List<IVariable>();
            Variables.Add(new Variable("Hello"));
            Variables.Add(new Variable("meep"));

            Console.WriteLine(WebRequests.SendPostRequest(WebUtils.PrepWebData(Variables), url));
        }
    }
}