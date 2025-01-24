using Microsoft.VisualStudio.TestTools.UnitTesting;
using UZonMailProPlugin.Services.Crawlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UZonMailProPlugin.Services.Crawlers.Tests
{
    [TestClass()]
    public class TikTokSignatureResolverTests
    {
        [DataRow("this is my email 23082@qq.com")]
        [DataRow("WhatsApp+8619139598963")]
        [DataRow(@"https://wa.me/8619139598963")]
        [TestMethod()]
        public void ResolveTest(string signature)
        {
            var resolver = new TikTokSignatureResolver(signature);
            var info = resolver.Resolve();
            Assert.IsNotNull(info);

            if (!string.IsNullOrEmpty(info.Email))
                Assert.AreEqual("23082@qq.com", info.Email);


            if (!string.IsNullOrEmpty(info.WhatsApp))
                Assert.AreEqual("8619139598963", info.WhatsApp);
        }
    }
}