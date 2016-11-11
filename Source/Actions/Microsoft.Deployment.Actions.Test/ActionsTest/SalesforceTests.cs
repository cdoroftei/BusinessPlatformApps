using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Deployment.Actions.Test.TestHelpers;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.Deployment.Common.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Actions.Test.ActionsTest
{
    [TestClass]
    public class SalesforceTests
    {
        public string sfUsername = Credential.Instance.Salesforce.Username;
        public string sfPassword = Credential.Instance.Salesforce.Password;
        public string sfToken = Credential.Instance.Salesforce.Token;
        public string sqlServer = Credential.Instance.Sql.Server;
        public string sqlUsername = Credential.Instance.Sql.Username;
        public string sqlDatabase = Credential.Instance.Sql.Database;
        public string sqlPassword = Credential.Instance.Sql.Password;

        [TestMethod]
        public void SalesforceSqlArtefactsDeploysSuccessful()
        {
            this.CleanDb();

            DataStore dataStore = new DataStore();
            dataStore.AddToDataStore("SalesforceUser", this.sfUsername, DataStoreType.Public);
            dataStore.AddToDataStore("SalesforcePassword", this.sfPassword, DataStoreType.Private);
            dataStore.AddToDataStore("SalesforceToken", this.sfToken, DataStoreType.Private);
            dataStore.AddToDataStore("SalesforceUrl", "https://login.salesforce.com/", DataStoreType.Public);
            dataStore.AddToDataStore("ObjectTables", "Opportunity,Account,Lead,Product2,OpportunityLineItem,OpportunityStage,User,UserRole", DataStoreType.Public);

            ActionResponse sqlResponse = GetSqlPagePayload();
            dataStore.AddToDataStore("SqlConnectionString", (sqlResponse.Body as JObject)["value"].ToString(), DataStoreType.Private);

            var response = TestHarness.ExecuteAction("Microsoft-SalesforceGetObjectMetadata", dataStore);

            dataStore.AddObjectDataStore("Objects", JsonUtility.GetJObjectFromObject(response.Body), DataStoreType.Any);

            response = TestHarness.ExecuteAction("Microsoft-SalesforceSqlArtefacts", dataStore);

            Assert.IsTrue(response.Status == ActionStatus.Success);
        }


        [TestMethod]
        public void RunSalesforceCredentialValidation()
        {
            DataStore dataStore = new DataStore();
            dataStore.AddToDataStore("SalesforceUser", this.sfUsername, DataStoreType.Public);
            dataStore.AddToDataStore("SalesforcePassword", this.sfPassword, DataStoreType.Private);
            dataStore.AddToDataStore("SalesforceToken", this.sfToken, DataStoreType.Private);
            dataStore.AddToDataStore("SalesforceUrl", "https://login.salesforce.com/", DataStoreType.Public);

            var result = TestHarness.ExecuteAction("Microsoft-ValidateSalesforceCredentials", dataStore);
            Assert.IsTrue(result.Status == ActionStatus.Success);
        }

        [TestMethod]
        public void CleanDb()
        {
            ActionResponse sqlResponse = GetSqlPagePayload();

            var dataStore = new DataStore();

            dataStore.AddToDataStore("SqlServerIndex", 0, DataStoreType.Any);
            dataStore.AddToDataStore("SqlScriptsFolder", "Service/Database/Cleanup", DataStoreType.Any);
            dataStore.AddToDataStore("SqlConnectionString", (sqlResponse.Body as JObject)["value"].ToString(), DataStoreType.Private);

            var response = TestHarness.ExecuteAction("Microsoft-DeploySQLScripts", dataStore);

            Assert.IsTrue(response.Status == ActionStatus.Success);
        }

        private ActionResponse GetSqlPagePayload()
        {
            var dataStore = new DataStore();

            dynamic sqlPayload = new ExpandoObject();
            sqlPayload.SqlCredentials = new ExpandoObject();
            sqlPayload.SqlCredentials.Server = this.sqlServer;
            sqlPayload.SqlCredentials.AuthType = "azuresql";
            sqlPayload.SqlCredentials.User = this.sqlUsername;
            sqlPayload.SqlCredentials.Password = this.sqlPassword;
            sqlPayload.SqlCredentials.Database = this.sqlDatabase;

            dataStore.AddObjectDataStore("SqlCredentials", JsonUtility.GetJObjectFromObject(sqlPayload), DataStoreType.Any);

            ActionResponse sqlResponse = TestHarness.ExecuteAction("Microsoft-GetSqlConnectionString", dataStore);
            Assert.IsTrue(sqlResponse.Status == ActionStatus.Success);
            return sqlResponse;
        }

    }
}