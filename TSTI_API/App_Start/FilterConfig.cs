using System.Web;
using System.Web.Mvc;
using TSTI_API.App_Start;

namespace TSTI_API
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
