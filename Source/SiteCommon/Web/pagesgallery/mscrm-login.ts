import { AzureLogin } from './azure-login';
import { DataStoreType } from '../services/datastore';
import { ActionResponse } from '../services/actionresponse';

export class MsCrmLogin extends AzureLogin {
    hasToken: boolean = false;

    constructor() {
        super();
    }

    async OnLoaded() {
        this.isValidated = false;
        this.showValidation = false;
        if (this.subscriptionsList.length > 0) {
            this.isValidated = true;
            this.showValidation = true;
        } else {
            let queryParam = this.MS.UtilityService.GetItem('queryUrl');
            if (queryParam) {
                let token = this.MS.UtilityService.GetQueryParameterFromUrl('code', queryParam);
                var tokenObj = {
                    code: token
                };
                this.authToken = await this.MS.HttpService.executeAsync('Microsoft-GetAzureToken', tokenObj);
                if (this.authToken.IsSuccess) {
                    this.hasToken = true;

                    this.MS.DataStore.addToDataStore('MsCrmToken', this.authToken.Body.AzureToken.access_token, DataStoreType.Private);

                    var response = await this.MS.HttpService.executeAsync('Microsoft-CrmGetOrgs', {});
                    console.log(response);
                }
                this.MS.UtilityService.RemoveItem('queryUrl');
            }
        }
    }

    async connect() {
        if (this.connectionType.toString() === AzureConnection.Microsoft.toString()) {
            this.MS.DataStore.addToDataStore('AADTenant', this.azureDirectory, DataStoreType.Public);
        } else {
            this.MS.DataStore.addToDataStore('AADTenant', 'common', DataStoreType.Public);
        }
        this.MS.DataStore.addToDataStore('IsMsCrm', true, DataStoreType.Public);
        let response: ActionResponse = await this.MS.HttpService.executeAsync('Microsoft-GetAzureAuthUri', {});
        window.location.href = response.Body.value;
    }

    public async NavigatingNext(): Promise<boolean> {
        return true;
    }
}

enum AzureConnection {
    Microsoft,
    Organizational
}