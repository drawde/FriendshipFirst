using FriendshipFirst.BLL;
using FriendshipFirst.Redis;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace FriendshipFirst.API
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            var config = new HubConfiguration();
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            
            using (var redisClient = RedisManager.GetClient())
            {
                //设置超级管理员
                redisClient.Set<string>(RedisKey.GetKey(RedisAppKeyEnum.Alpha, RedisCategoryKeyEnum.SuperAdminUserCode), "58657C04BCADF3C6AA26F2B79D24994D");

                redisClient.Set<string>(RedisKey.GetKey(RedisAppKeyEnum.Alpha, RedisCategoryKeyEnum.CSSAndJSVersion), SystemConfigBll.Instance.GetValueByKey(RedisCategoryKeyEnum.CSSAndJSVersion.ToString()));
            }
        }
    }
}
