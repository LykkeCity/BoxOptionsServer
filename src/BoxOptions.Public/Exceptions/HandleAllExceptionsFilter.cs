using System;
using Common.Log;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BoxOptions.Public.Exceptions
{
    
    public class HandleAllExceptionsFilter : IExceptionFilter
    {
        private readonly IHostingEnvironment _environment;
        private readonly ILog _log;

        public HandleAllExceptionsFilter(IHostingEnvironment environment, ILog log)
        {
            _environment = environment;
            _log = log;
        }

        public void OnException(ExceptionContext filterContext)
        {
            var controller = filterContext.RouteData.Values["controller"].ToString();
            var action = filterContext.RouteData.Values["action"].ToString();

            var context = filterContext.HttpContext.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress + ": " +
                          filterContext.HttpContext.User.Identity.Name;

            _log.WriteErrorAsync(controller + '/' + action, filterContext.HttpContext.Request.Query.ToString(), context, filterContext.Exception);

            if (filterContext == null)
            {
                throw new ArgumentNullException(nameof(filterContext));
            }

            // If custom errors are disabled, we need to let the normal ASP.NET exception handler
            // execute so that the user can see useful debugging information.
            if (filterContext.ExceptionHandled || !_environment.IsDevelopment())
            {
                return;
            }

            filterContext.HttpContext.Response.Clear();
            filterContext.Result = new ObjectResult(new ExceptionData(controller, action, filterContext.Exception))
            {
                StatusCode = 500,
                DeclaredType = typeof(ExceptionData)
            };
            filterContext.ExceptionHandled = true;
        }

    }
}
