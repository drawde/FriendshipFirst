using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using FriendshipFirst.Common.Util;
using FriendshipFirst.Model.CustomModels;
using Newtonsoft.Json;
using FriendshipFirst.Common;
using FriendshipFirst.BLL;
using FriendshipFirst.Model;

namespace FriendshipFirst.Web.Filters
{
    /// <summary>
    /// 用户授权验证
    /// </summary>
    public class OAuthAttribute : AuthorizeAttribute
    {

        /// <summary>
        /// 设置加载顺序
        /// </summary>
        public OAuthAttribute()
        {
            Order = 1;
        }
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            string controllerName = filterContext.RouteData.Values["controller"].ToString().ToLower();
            string actionName = filterContext.RouteData.Values["action"].ToString().ToLower();
            string returnUrl = "/" + controllerName + "/" + actionName + filterContext.RequestContext.HttpContext.Request.Url.Query;

            string userJson = CookieHelper.GetCookieValue("User");
            //Log.Default.Debug(userJson);
            if (!userJson.IsNullOrEmpty())
            {
                CUsers user = null;
                try
                {
                    user = JsonConvert.DeserializeObject<CUsers>(userJson);
                    FF_User hs_user = UsersBll.Instance.GetUserByAdmin(user.UserCode);
                    filterContext.Controller.ViewBag.User = hs_user;
                    DateTime now = DateTime.Now;
                    string SecretCode = hs_user.SecretCode;
                    filterContext.Controller.ViewBag.ConfusionStringToHTML = SignUtil.CreateConfusionStringToHTML(SecretCode, now);
                }
                catch (Exception ex)
                {
                    Log.Default.Error(ex);
                }
                if (user == null || UsersBll.Instance.IsUserCodeRepeat(user.UserCode) == false)
                {
                    SetContextResult(filterContext, returnUrl);
                    return;
                }
            }
            else
            {
                SetContextResult(filterContext, returnUrl);
                return;
            }
        }

        public void SetContextResult(AuthorizationContext filterContext,string returnUrl)
        {            
            filterContext.Result = new RedirectResult("/UserCentre/Login?returnUrl=" + System.Web.HttpUtility.UrlEncode(returnUrl));
        }
    }
}
