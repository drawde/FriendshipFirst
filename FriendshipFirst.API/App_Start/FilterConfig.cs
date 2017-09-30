using FriendshipFirst.API.Filters;
using System.Web;
using System.Web.Mvc;

namespace FriendshipFirst.API
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new ActionAllowOriginAttribute());
            filters.Add(new RecordDataExchangeAttribute());
            filters.Add(new ExceptionAttribute());
        }
    }
}
