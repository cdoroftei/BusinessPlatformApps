﻿import { ViewModelBase } from '../services/viewmodelbase';
import { DataStoreType } from '../services/datastore';
import { SqlServerValidationUtility } from '../base/sql-server-validation-utility';
import { ActionResponse } from '../services/actionresponse';

export class SqlServer extends ViewModelBase {
    subtitle: string = '';
    title: string = '';

    auth: string = 'Windows';
    azureSqlSuffix: string = '.database.windows.net';
    checkSqlVersion: boolean = false;
    database: string = null;
    databases: string[] = [];
    hideSqlAuth: boolean = false;
    isAzureSql: boolean = false;
    isWindowsAuth: boolean = true;
   
    newSqlDatabase: string = null;
    password: string = '';
    passwordConfirmation: string = '';
    showAllWriteableDatabases: boolean = true;
    showAzureSql: boolean = true;

    showDatabases: boolean = false;
    showNewSqlOption: boolean = false;
    sqlInstance: string = 'ExistingSql';
    sqlServer: string = '';
    username: string = '';
    validateWindowsCredentials: boolean = false;
    validationTextBox: string = '';

    useImpersonation: boolean = false;

    constructor() {
        super();
        this.isValidated = false;
    }

    Invalidate() {
        super.Invalidate();
        this.database = null;
        this.databases = [];
        this.onAuthChange();
        this.showDatabases = false;
    }

    onAuthChange() {
        this.isWindowsAuth = this.auth.toLowerCase() === 'windows';
    }

    async OnValidate(): Promise<boolean> {
        if (!super.OnValidate()) {
            return false;
        }

        this.sqlServer = this.sqlServer.toLowerCase();
        if (this.sqlInstance === 'ExistingSql') {
            let databasesResponse = await this.GetDatabases();
            if (databasesResponse.IsSuccess) {
                this.databases = databasesResponse.Body.value;
                this.isValidated = true;
                this.showDatabases = true;
            } else {
                this.isValidated = false;
                this.showDatabases = false;
            }
        } else if (this.sqlInstance === 'NewSql') {
            let newSqlError: string = SqlServerValidationUtility.validateAzureSQLCreate(this.sqlServer, this.username, this.password, this.passwordConfirmation);
            if (newSqlError) {
                this.MS.ErrorService.message = newSqlError;
            } else {
                let databasesResponse = await this.ValidateAzureServerIsAvailable();
                if ((databasesResponse.IsSuccess)) {
                    this.isValidated = true;
                } else {
                    this.isValidated = false;
                }
            }
        }

        return this.isValidated;
    }

    async NavigatingNext(): Promise<boolean> {

        let body = this.GetBody(true);
        let response:ActionResponse = null;

        if (this.sqlInstance === 'ExistingSql') {
            response = await this.MS.HttpService.executeAsync('Microsoft-GetSqlConnectionString', body);
        } else if (this.sqlInstance === 'NewSql') {
            response = await this.CreateDatabaseServer();
        }

        if (!response || !response.IsSuccess) {
            return false;
        }

        this.MS.DataStore.addToDataStore('SqlConnectionString', response.Body.value, DataStoreType.Private);
        this.MS.DataStore.addToDataStore('Server', this.getSqlServer(), DataStoreType.Public);
        this.MS.DataStore.addToDataStore('Database', this.database, DataStoreType.Public);
        this.MS.DataStore.addToDataStore('Username', this.username, DataStoreType.Public);

        if (this.checkSqlVersion) {
            let responseVersion = await this.MS.HttpService.executeAsync('Microsoft-CheckSQLVersion', body);
            if (!responseVersion.IsSuccess) {
                return false;
            }
        }

        return true;
    }

    private async GetDatabases() {
        let body: any = this.GetBody(true);
        return this.showAllWriteableDatabases
            ? await this.MS.HttpService.executeAsync('Microsoft-ValidateAndGetWritableDatabases', body)
            : await this.MS.HttpService.executeAsync('Microsoft-ValidateAndGetAllDatabases', body);
    }

    private GetBody(withDatabase: boolean) {
        let body: any = {};

        body.UseImpersonation = this.useImpersonation;
        body['SqlCredentials'] = {};
        body['SqlCredentials']['Server'] = this.getSqlServer();
        body['SqlCredentials']['User'] = this.username;
        body['SqlCredentials']['Password'] = this.password;
        body['SqlCredentials']['AuthType'] = this.isWindowsAuth ? 'windows' : 'sql';

        if (this.isAzureSql) {
            body['SqlCredentials']['AuthType'] = 'sql';
        }

        if (withDatabase) {
            body['SqlCredentials']['Database'] = this.database;
        }

        return body;
    }

    private getSqlServer(): string {
        let sqlServer: string = this.sqlServer;
        if (this.isAzureSql && !sqlServer.includes(this.azureSqlSuffix)) {
            sqlServer += this.azureSqlSuffix;
        }
        return sqlServer;
    }

    private async CreateDatabaseServer() {
        this.navigationMessage = 'Creating a new SQL database, this may take 2-3 minutes';
        let body = this.GetBody(true);
        body['SqlCredentials']['Database'] = this.newSqlDatabase;
        return await this.MS.HttpService.executeAsync('Microsoft-CreateAzureSql', body);
    }

    private async ValidateAzureServerIsAvailable() {
        let body = this.GetBody(false);
        return await this.MS.HttpService.executeAsync('Microsoft-ValidateAzureSqlExists', body);
    }
}