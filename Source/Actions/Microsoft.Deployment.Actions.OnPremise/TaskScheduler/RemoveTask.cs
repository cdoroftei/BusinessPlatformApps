using System;
using System.ComponentModel.Composition;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Helpers;
using Microsoft.Win32.TaskScheduler;
using Task = Microsoft.Win32.TaskScheduler.Task;

namespace Microsoft.Deployment.Actions.OnPremise.TaskScheduler
{
    [Export(typeof(IAction))]
    public class RemoveTask : BaseAction
    {
        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            var taskName = request.DataStore.GetValue("TaskName");
            using (TaskService ts = new TaskService())
            {
                TaskCollection tasks = ts.RootFolder.GetTasks(new Regex(taskName));

                try
                {
                    if (tasks.Exists(taskName))
                    {
                        foreach (Task task in tasks)
                        {
                            if (task.Name.EqualsIgnoreCase(taskName))
                            {
                                ts.RootFolder.DeleteTask(taskName, false);
                            }
                        }

                    }
                }
                catch (Exception e)
                {
                    return new ActionResponse(ActionStatus.Failure, e);
                }

                return new ActionResponse(ActionStatus.Success);
            }
        }
    }
}