using AnkamaAccGen.Helpers;

namespace AnkamaAccGen.AntiCaptcha.ApiResponse
{
    public class CreateTaskResponse
    {
        public CreateTaskResponse(string task, dynamic json)
        {
            ErrorId = JsonHelper.ExtractInt(task, json, "errorId");

            if (ErrorId != null)
            {
                if (ErrorId.Equals(0))
                {
                    TaskId = JsonHelper.ExtractInt(task, json, "taskId");
                }
                else
                {
                    ErrorCode = JsonHelper.ExtractStr(task, json, "errorCode");
                    ErrorDescription = JsonHelper.ExtractStr(task, json, "errorDescription") ?? "(no error description)";
                }
            }
            else
            {
                DebugHelper.Out(task, "Unknown error", DebugHelper.Type.Error);
            }
        }

        public int? ErrorId { get; private set; }
        public string ErrorCode { get; private set; }
        public string ErrorDescription { get; private set; }
        public int? TaskId { get; private set; }
    }
}