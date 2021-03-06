﻿using Furcadia.Net.Web;
using NUnit.Framework;

namespace FurcadiaLibTests.Net.web

{
    [TestFixture]
    public class NetWebTests
    {
        [TestCase("http://silvermonkey.tsprojects.org")]
        [TestCase("https://silvermonkey.tsprojects.org")]
        public void TestPostDataIsNotNull(string url)
        {
            NetWeb var = new NetWeb(url);
            var list = var.PostData();
            Assert.IsNotNull(var.PostData());
        }
    }
}