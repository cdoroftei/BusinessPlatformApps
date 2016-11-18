using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;

namespace Microsoft.Deployment.Site.Web.Tests
{
    [TestClass]
    public class Microsoft_SCCMTemplate
    {
        private RemoteWebDriver driver;
        private const string sccmUrl = "Microsoft-SCCMTemplate";
        private const string uninstallType = "uninstall";
        private const string msiLocation = "..\\..\\..\\..\\..\\..\\Private\\Build\\Msi\\bin\\x64\\Release\\Microsoft.Bpst.App.Msi.exe";

        [Ignore]
        [TestMethod]
        public void TestSCCMUninstall()
        {
            var options = new ChromeOptions
            {
                BinaryLocation = msiLocation
            };

            options.AddArgument($"?name={sccmUrl}&type={uninstallType}");
            driver = new ChromeDriver(options);
            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(30));
            var element = driver.FindElementsByTagName("Button").FirstOrDefault(p => p.Text == "Next");
            var js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].click()", element);
        }

        [TestCleanup]
        public void Cleanup()
        {
            driver.Quit();
        }
    }
}
