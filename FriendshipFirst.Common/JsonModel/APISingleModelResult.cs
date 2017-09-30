using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FriendshipFirst.Common.JsonModel
{
    public class APISingleModelResult<T> : APIResultBase
    {
        public T data { get; set; }
    }
}
