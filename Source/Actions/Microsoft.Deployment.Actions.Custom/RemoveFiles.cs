using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Actions.Custom
{
    [Export(typeof(IAction))]
    public class RemoveFiles : BaseAction
    {
        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            string targetPath = request.DataStore.GetValue("TargetPath") == null
                                ? FileUtility.GetLocalTemplatePath(request.Info.AppName)
                                : request.DataStore.GetValue("TargetPath");

            ActionResponse response = null;

            if (Directory.Exists(targetPath))
            {
                try
                {
                    RetryUtility.Retry(5, () =>
                    {
                        Directory.Delete(targetPath, true);
                        Thread.Sleep(500);
                    });
                    response = new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject());
                }
                catch (Exception ex)
                {
                    response = new ActionResponse(ActionStatus.Failure, JsonUtility.GetEmptyJObject(), ex, string.Empty);
                }
            }
            return response;
        }
    }
}