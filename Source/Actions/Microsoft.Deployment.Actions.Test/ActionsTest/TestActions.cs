using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Deployment.Actions.Test.TestHelpers;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Deployment.Actions.Test.CommonActions
{
    [TestClass]
    public class TestActions
    {
        [TestMethod]
        public void RunSimpleTestActionTest()
        {
            var result = TestHarness.ExecuteAction("Microsoft-Test", new DataStore());
            Assert.IsTrue(result.Status == ActionStatus.Success);
        }
    }
}
