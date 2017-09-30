using FriendshipFirst.Common.JsonModel;
using FriendshipFirst.Common.Enum;
using FriendshipFirst.Common.Util;
using FriendshipFirst.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using FriendshipFirst.BLL;

namespace FriendshipFirst.Web.Filters
{
    /// <summary>
    /// WEB层异常过滤器
    /// </summary>
    public class WebException : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            HS_ErrRec ex = new HS_ErrRec();
            try
            {
                ContentResult contentResult = new ContentResult();
                contentResult.Content = JsonStringResult.Error(OperateResCodeEnum.内部错误);
                filterContext.Result = contentResult;
                filterContext.ExceptionHandled = true;

                ex.Action = (filterContext.RouteData.Values["action"]).ToString();
                ex.AddTime = DateTime.Now;
                ex.Controller = (filterContext.RouteData.Values["controller"]).ToString();
                ex.ErrorMsg = filterContext.Exception.Message;
                ex.IP = StringUtil.GetIP();
                ex.StackTrace = filterContext.Exception.StackTrace;
                ex.Arguments = "";
                ex.DataSource = (int)DataSourceEnum.Web;
                ErrRecBll.Instance.AsyncInsert(ex);
            }
            catch (Exception ep)
            {

            }
        }
    }
}
