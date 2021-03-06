﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FriendshipFirst.Common.JsonModel
{
    public class APIResultBase
    {
        /// <summary>
        /// 返回值
        /// </summary>
        public int code { get; set; }

        /// <summary>
        /// 返回值说明
        /// </summary>
        public string msg { get; set; }
    }
}
