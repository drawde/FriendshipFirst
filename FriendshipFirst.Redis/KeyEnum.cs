﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FriendshipFirst.Redis
{
    /// <summary>
    /// Redis应用程序键
    /// </summary>
    public enum RedisAppKeyEnum
    {
        /// <summary>
        /// 第一个应用程序
        /// </summary>
        Alpha = 1,
    }

    /// <summary>
    /// Redis分类键
    /// </summary>
    public enum RedisCategoryKeyEnum
    {
        /// <summary>
        /// 游戏控制器
        /// </summary>
        GameControllers = 1,

        /// <summary>
        /// 卡牌模型对象
        /// </summary>
        CardsInstance = 2,

        /// <summary>
        /// 超级管理员
        /// </summary>
        SuperAdminUserCode = 3,

        /// <summary>
        /// CSS和JS文件版本号
        /// </summary>
        CSSAndJSVersion = 4,

        /// <summary>
        /// 游戏房间里的用户
        /// </summary>
        SignalRUser = 5,

        /// <summary>
        /// 游戏房间
        /// </summary>
        SignalRRoom = 6
    }
}
