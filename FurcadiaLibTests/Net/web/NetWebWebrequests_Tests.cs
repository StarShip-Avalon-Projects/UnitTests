using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Furcadia.Net.Web;
using System.Diagnostics;

namespace FurcadiaLibTests.Net.web
{
    [TestFixture]
    internal class NetWebWebrequests_Tests
    {
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