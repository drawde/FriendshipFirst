using FriendshipFirst.Web.Filters;
using System.Web;
using System.Web.Mvc;

namespace FriendshipFirst.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new WebException());
            filters.Add(new PageVariable());
        }
    }
}
