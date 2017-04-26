using System;
using Common.Log;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BoxOptions.Public.Exceptions
{
    public class HandleAllExceptionsFilterFactory : IFilterFactory
    {
        public bool IsReusable => true;

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            var environment = (IHostingEnvironment)serviceProvider.GetService(typeof(IHostingEnvironment));
            var log = (ILog)serviceProvider.GetService(typeof(ILog));
            return new HandleAllExceptionsFilter(environment, log);
        }
    }
}
