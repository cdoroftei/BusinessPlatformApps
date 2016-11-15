import MainService from "../services/mainservice";
import * as Datastore from "../services/datastore";

export class JsonCustomParser {

    public static loadVariables(objToChange, obj: any, MS: MainService) {
        for (let propertyName in obj) {
            let val: string = obj[propertyName];
            if (JsonCustomParser.isVariable(val)) {
                const codeToRun = JsonCustomParser.extractVariable(val);
                val = eval(codeToRun);

                if (JsonCustomParser.isPermenantEntryIntoDataStore(obj[propertyName])) {
                    MS.DataStore.addToDataStore(propertyName, val, Datastore.DataStoreType.Private);
                }
            }

            objToChange[propertyName] = val;

            if (val && typeof (val) === 'object') {
                this.loadVariables(objToChange[propertyName], val, MS);
            }
        }
    }

    public static isVariable(value: string): boolean {
        value = value.toString();
        if (value.startsWith('$(') && value.endsWith(')')) {
            return true;
        } else {
            return false;
        }
    }

    public static extractVariable(value: string): string {
        let intermediate: string = value.replace('$(', '');
        let result = intermediate.slice(0, intermediate.length - 1);
        let resultSplit = result.split(',');
        return resultSplit[0].trim();
    }

    public static isPermenantEntryIntoDataStore(value: string): boolean {
        let intermediate: string = value.replace('$(', '');
        let result = intermediate.slice(0, intermediate.length - 1);
        let resultSplit = result.split(',');

        for (let index =0; index< resultSplit.length; index++) {
            if (index < 1) {
                continue;
            }

            let param: string = resultSplit[index].trim().toLowerCase();
            let paramSplit = param.split('=');

            if (paramSplit[0] === 'issaved' && paramSplit[1] === 'true') {
                return true;
            }
        }

        return false;
    }
}