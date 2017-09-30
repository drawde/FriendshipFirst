using FriendshipFirst.BLL;
using FriendshipFirst.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using FriendshipFirst.Common.Util;


namespace FriendshipFirst.API.Filters
{
    /// <summary>
    /// 接口访问记录器
    /// </summary>
    public class RecordDataExchangeAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            try
            {
                ContentResult cr = (ContentResult)filterContext.Result;
                string response = cr.Content;
                DataExchangeBll.Instance.AsyncInsert((filterContext.RouteData.Values["action"]).ToString(), (filterContext.RouteData.Values["controller"]).ToString(), filterContext.Controller.TempData["fullData"].TryParseString(), response);
            }
            catch (Exception)
            {

            }
            base.OnResultExecuted(filterContext);
        }
    }
}
