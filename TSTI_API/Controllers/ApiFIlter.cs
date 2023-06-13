using System.Net;
using System.Web.Mvc;


namespace TSTI_API.Controllers
{
	public class ApiFilter : ActionFilterAttribute
	{
		static string API_KEY = "6xdTlREsMbFd0dBT28jhb5W3BNukgLOos";
		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{

		    string apiKey = filterContext.HttpContext.Request.Headers["X-MBX-APIKEY"];


			if (API_KEY != apiKey)
			{
				var response = new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
				filterContext.Result = response;
			}
			else
			{
				// 驗證成功，繼續執行後續的 Action 方法
				base.OnActionExecuting(filterContext);
			}
		}

	}
}

