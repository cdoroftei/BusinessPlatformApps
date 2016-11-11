using Microsoft.Deployment.Common.Controller;
using Newtonsoft.Json;

namespace Microsoft.Deployment.Common.ActionModel
{
    public class ActionRequest
    {
        [JsonIgnore]
        public CommonControllerModel ControllerModel;

        [JsonIgnore]
        public UserInfo Info;

        [JsonIgnore]
        public Logger Logger;

        public DataStore DataStore = new DataStore();
    }
}