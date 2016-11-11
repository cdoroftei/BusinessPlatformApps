using Microsoft.Deployment.Actions.Test.TestHelpers;
using Microsoft.Deployment.Common;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Actions.Test.ActionsTest
{
    [TestClass]
    public class AzureCommonTests
    {
        [TestMethod]
        public async Task GetAzureToken()
        {
            DataStore dataStore = new DataStore();

            var datastore = await AAD.GetTokenWithDataStore();
            var result = await TestHarness.ExecuteActionAsync("Microsoft-GetAzureSubscriptions", datastore);
            Assert.IsTrue(result.IsSuccess);
            var responseBody = JObject.FromObject(result.Body);
        }

        [Ignore]
        [TestMethod]
        public async Task DeployArmTemplateTest()
        {
            var datastore = await AAD.GetTokenWithDataStore();

            var subscriptionResult = await TestHarness.ExecuteActionAsync("Microsoft-GetAzureSubscriptions", datastore);
            Assert.IsTrue(subscriptionResult.IsSuccess);
            var subscriptionId = subscriptionResult.Body.GetJObject()["value"][0];
            datastore.AddToDataStore("SelectedSubscription", subscriptionId, DataStoreType.Public);

            var locationResult = await TestHarness.ExecuteActionAsync("Microsoft-GetLocations", datastore);
            Assert.IsTrue(locationResult.IsSuccess);
            var location = locationResult.Body.GetJObject()["value"][5];
            datastore.AddToDataStore("SelectedLocation", location, DataStoreType.Public);


            datastore.AddToDataStore("SelectedResourceGroup", "Test");
            var deleteResourceGroupResult = await TestHarness.ExecuteActionAsync("Microsoft-DeleteResourceGroup", datastore);

            var resourceGroupResult = await TestHarness.ExecuteActionAsync("Microsoft-CreateResourceGroup", datastore);
            Assert.IsTrue(resourceGroupResult.IsSuccess);

            datastore.AddToDataStore("AzureArmFile", "Service/Arm/armtemplate.json");
            var paramFile = JsonUtility.GetJsonObjectFromJsonString(System.IO.File.ReadAllText(@"Apps/TestApps/TestApp/Service/Arm/armparam.json"));
            paramFile["AzureArmParameters"]["SqlServerName"] = "sqltestserver" + RandomGenerator.GetRandomLowerCaseCharacters(10);
            datastore.AddToDataStore("AzureArmParameters", paramFile["AzureArmParameters"]);
            var armResult = await TestHarness.ExecuteActionAsync("Microsoft-DeployAzureArmTemplate", datastore);
            Assert.IsTrue(armResult.IsSuccess);
        }
    }
}
