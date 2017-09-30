using System;
using System.Collections.Generic;

namespace FriendshipFirst.Common
{
    [Serializable]
    public class PageResult<T> : IPagedItemsResult<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int TotalItemsCount { get; set; }
    }
}
