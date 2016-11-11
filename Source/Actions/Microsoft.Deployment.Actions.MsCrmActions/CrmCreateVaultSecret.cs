namespace Microsoft.Deployment.Common.Actions.MsCrm
{
    using Microsoft.Azure;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Azure.Management.KeyVault;
    using Microsoft.Deployment.Common.ActionModel;
    using Microsoft.Deployment.Common.Actions;
    using Microsoft.Deployment.Common.Helpers;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Threading.Tasks;

    [Export(typeof(IAction))]
    public class CrmCreateVaultSecret : BaseAction
    {
        private string _azureToken = "NO_TOKEN";
        private readonly Guid _crmServicePrincipal = new Guid("b861dbcc-a7ef-4219-a005-0e4de4ea7dcf"); // DO NOT CHANGE THIS

        public async Task<string> GetAccessToken(string authority, string resource, string scope)
        {
            return _azureToken;
        }


        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            string _azureToken = request.DataStore.GetValue("AzureToken");
            string subscriptionID = request.DataStore.GetValue("SelectedSubscription");
            string resourceGroup = request.DataStore.GetValue("SelectedResourceGroup");
            string vaultName = request.DataStore.GetValue("VaultName") ?? "bpst-mscrm-vault";
            string secretName = request.DataStore.GetValue("SecretName") ?? "bpst-mscrm-secret";
            string connectionString = request.DataStore.GetAllValues("SqlConnectionString")[0];
            string organizationId = request.DataStore.GetValue("OrganizationId");
            string tenantId = request.DataStore.GetValue("TenantId") ?? "72f988bf-86f1-41af-91ab-2d7cd011db47";

            SubscriptionCloudCredentials credentials = new TokenCloudCredentials(subscriptionID, _azureToken);
            KeyVaultManagementClient client = new KeyVaultManagementClient(credentials);

            // Check if a vault already exists
            Vault vault = null;
            VaultListResponse vaults = client.Vaults.List(resourceGroup, 100);
            foreach (var v in vaults.Vaults)
            {
                if (v.Name.EqualsIgnoreCase(vaultName))
                {
                    vault = (Vault)v;
                    break;
                }
            }

            // Create the vault
            if (vault == null)
            {
                vault = client.Vaults.CreateOrUpdate(resourceGroup, vaultName, new VaultCreateOrUpdateParameters()).Vault;
            }

            AccessPolicyEntry ape = new AccessPolicyEntry();
            ape.PermissionsToSecrets = new [] { "get" };
            ape.ApplicationId = _crmServicePrincipal;

            vault.Properties.AccessPolicies.Add(ape);
            

            // Create the secret
            KeyVaultClient kvClient = new KeyVaultClient(GetAccessToken);
            Secret secret = await kvClient.SetSecretAsync(vault.Properties.VaultUri, secretName, connectionString, new Dictionary<string, string>() {{organizationId, tenantId}},
                                                 null, new SecretAttributes() {Enabled = true});
            return new ActionResponse(ActionStatus.Success, secret.Id, true);
        }
    }
}