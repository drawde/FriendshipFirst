using FriendshipFirst.BLL;
using FriendshipFirst.API.Filters;
using FriendshipFirst.Common.JsonModel;
using FriendshipFirst.Common.Enum;
using FriendshipFirst.Common.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using FriendshipFirst.Common;
using FriendshipFirst.Model;

namespace FriendshipFirst.API.Controllers
{
    public class UsersController : Controller
    {
        //[DataVerify(false)]
        //public ActionResult Register()
        //{
        //    string res = OperateJsonRes.VerifyFail();
        //    //转化成json对象
        //    var param = JObject.Parse(TempData["param"].TryParseString());

        //    res = UsersBll.Instance.Register(param["UserName"].TryParseString(), param["Password"].TryParseString(), param["Mobile"].TryParseString(),
        //        param["Email"].TryParseString(), param["InvitationCode"].TryParseString(), param["NickName"].TryParseString(), param["HeadImg"].TryParseString());
        //    return Content(res);
        //}

        [DataVerify(false)]
        public ActionResult ValidateUserName()
        {
            string res = JsonStringResult.VerifyFail();
            var param = JObject.Parse(TempData["param"].TryParseString());
            res = UsersBll.Instance.IsRepeat(param["UserName"].TryParseString()) ? JsonStringResult.Error(OperateResCodeEnum.用户名重复) : JsonStringResult.SuccessResult();
            return Content(res);
        }        
    }
}