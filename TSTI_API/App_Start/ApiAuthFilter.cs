using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Filters;

namespace TSTI_API.App_Start
{
    public class ApiAuthFilter: ActionFilterAttribute, IActionFilter
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            System.Diagnostics.Debug.WriteLine(actionExecutedContext.Request.Headers.GetValues("X-MBX-APIKEY"));
            
            base.OnActionExecuted(actionExecutedContext);
        }
    }
}