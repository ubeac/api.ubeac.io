using System;

namespace uBeac
{
    public class Error
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public object Trace { get; set; }

        public Error()
        {
            Code = ErrorCodes.UNKNOWN_ERROR;
            Message = "";
        }

        public Error(string code, string message)
        {
            Code = code;
            Message = message;
        }

        public Error(Exception exception)
        {
            Code = ErrorCodes.UNHANDLED_EXCEPTION;
            Message = exception.Message;
            Trace = exception.StackTrace;
        }

    }
}
