using System;

namespace BoxOptions.Public.Exceptions
{
    public class ExceptionData
    {
        public string Controller { get; private set; }
        public string Action { get; private set; }
        public Exception Exception { get; private set; }

        public ExceptionData(string controller, string action, Exception e)
        {
            Controller = controller;
            Action = action;
            Exception = e;
        }
    }
}
