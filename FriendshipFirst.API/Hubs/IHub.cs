﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FriendshipFirst.API.Hubs
{
    interface IHub
    {
        /// <summary>
        /// 退出房间
        /// </summary>
        /// <param name="param"></param>
        void LeaveRoom(string param);

        /// <summary>
        /// 发送聊天消息
        /// </summary>
        /// <param name="userCode"></param>
        /// <param name="userName"></param>
        /// <param name="chatContent"></param>
        void SendChat(string userCode, string chatContent);

        /// <summary>
        /// 发送广播消息
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="message"></param>
        void Bordcast(string param);
    }
}