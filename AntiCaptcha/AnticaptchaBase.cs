using AnkamaAccGen.Helper;
using AnkamaAccGen.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;

namespace AnkamaAccGen.AntiCaptcha
{
    public abstract class AnticaptchaBase
    {
        private const string Host = "api.anti-captcha.com";

        public string ErrorMessage { get; private set; }
        public int TaskId { get; private set; }
        public string ClientKey { set; private get; }
        public string TaskInfo { get; set; }

        public abstract JObject GetPostData();

        internal bool CreateTaskViaWebClient(string task)
        {
            var taskJson = GetPostData();

            if (taskJson == null)
            {
                DebugHelper.Out(task, "A task preparing error.", DebugHelper.Type.Error);

                return false;
            }

            var jsonPostData = new JObject
            {
                ["clientKey"] = ClientKey,
                ["task"] = taskJson
            };

            DebugHelper.Out(task, "Connecting to " + Host, DebugHelper.Type.Info);

            dynamic postResult = JsonConvert.DeserializeObject(JsonPostRequestViaWebClient(task, ApiMethod.CreateTask, jsonPostData));

            if (postResult.errorId != "0")
            {
                ErrorMessage = postResult.errorDescription;

                DebugHelper.Out(task,
                    "API error " + postResult.errorId + ": " + postResult.errorDescription,
                    DebugHelper.Type.Error
                );

                return false;
            }

            if (postResult.taskId == null)
            {
                DebugHelper.JsonFieldParseError(task, "taskId", postResult);

                return false;
            }

            TaskId = (int)postResult.taskId;
            DebugHelper.Out(task, "Task ID: " + TaskId, DebugHelper.Type.Success);

            return true;
        }

        internal bool WaitForResultViaWebClient(string task, int maxSeconds = 120, int currentSecond = 0)
        {
            if (currentSecond >= maxSeconds)
            {
                DebugHelper.Out(task, "Time's out.", DebugHelper.Type.Error);

                return false;
            }

            if (currentSecond.Equals(0))
            {
                DebugHelper.Out(task, $"Waiting for 3 seconds...", DebugHelper.Type.Info);
                Thread.Sleep(3000);
            }
            else
            {
                Thread.Sleep(1000);
            }

            //DebugHelper.Out(task, "Requesting the task status", DebugHelper.Type.Info);

            var jsonPostData = new JObject
            {
                ["clientKey"] = ClientKey,
                ["taskId"] = TaskId
            };

            dynamic postResult = JsonConvert.DeserializeObject(JsonPostRequestViaWebClient(task, ApiMethod.GetTaskResult, jsonPostData));

            if (postResult.errorId != "0")
            {
                DebugHelper.Out(task, "API error : " + ErrorMessage, DebugHelper.Type.Error);
                return false;
            }

            if (postResult.status == "processing")
            {
                DebugHelper.Out(task, $"The task is still processing... {currentSecond} elasped", DebugHelper.Type.Info);
                return WaitForResultViaWebClient(task, maxSeconds, currentSecond + 1);
            }

            if (postResult.status == "ready")
            {
                if (postResult.solution.gRecaptchaResponse == "")
                {
                    DebugHelper.Out(task, "Got no 'solution' field from API", DebugHelper.Type.Error);
                    return false;
                }

                DebugHelper.Out(task, $"The task is complete! {postResult.solution.gRecaptchaResponse}", DebugHelper.Type.Success);
                TaskInfo = postResult.solution.gRecaptchaResponse;
                return true;
            }

            ErrorMessage = "An unknown API status, please update your software";
            DebugHelper.Out(task, ErrorMessage, DebugHelper.Type.Error);

            return false;
        }

        private dynamic JsonPostRequestViaWebClient(string task, ApiMethod methodName, JObject jsonPostData)
        {
            string error;
            var methodNameStr = char.ToLowerInvariant(methodName.ToString()[0]) + methodName.ToString().Substring(1);

            dynamic data = HttpHelper.PostViaWebClient(task, new Uri("https://" + Host + "/" + methodNameStr), JsonConvert.SerializeObject(jsonPostData, Formatting.Indented), out error);

            if (string.IsNullOrEmpty(error))
                if (data == null)
                    error = "Got empty or invalid response from API";
                else
                    return data;
            else
                error = "HTTP or JSON error: " + error;

            DebugHelper.Out(task, error, DebugHelper.Type.Error);

            return false;
        }

        private enum ApiMethod
        {
            CreateTask,
            GetTaskResult,
            GetBalance
        }
    }
}