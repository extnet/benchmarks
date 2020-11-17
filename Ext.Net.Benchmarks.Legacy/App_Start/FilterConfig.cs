using System.Web;
using System.Web.Mvc;

namespace Ext.Net.Benchmarks.Legacy
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
