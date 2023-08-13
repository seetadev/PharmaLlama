using System;
using System.Diagnostics;
using Windows.ApplicationModel.Background;

namespace doc_onlook
{
    public sealed class SampleBackgroundTask : IBackgroundTask
    {
        public SampleBackgroundTask()
        {
            Debug.WriteLine("SampleBackgroundTask created.");
        }
        void IBackgroundTask.Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.Canceled += taskInstance_Canceled;
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();
            Debug.WriteLine(DateTime.Now.ToString());

            deferral.Complete();
        }

        void taskInstance_Canceled(IBackgroundTaskInstance sender,
                BackgroundTaskCancellationReason reason)
        {
            Debug.WriteLine("Task cancelled.");
        }
    }
}
