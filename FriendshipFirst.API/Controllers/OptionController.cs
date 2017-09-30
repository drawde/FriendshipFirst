using FriendshipFirst.API.Filters;
using FriendshipFirst.Common.JsonModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FriendshipFirst.API.Controllers
{
    public class OptionController : Controller
    {
        /// <summary>
        /// 同步服务器时间
        /// </summary>
        /// <returns></returns>
        [DataVerify(false)]
        public ActionResult SyncTime()
        {
            return Content(JsonStringResult.SuccessResult(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
        }
    }
}