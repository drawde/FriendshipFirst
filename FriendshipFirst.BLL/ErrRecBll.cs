using FriendshipFirst.DAL;
using FriendshipFirst.DAL.Impl;
using FriendshipFirst.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FriendshipFirst.BLL
{
    public class ErrRecBll : BaseBLL<HS_ErrRec>
    {
        private IRepository<HS_ErrRec> _repository = new Repository<HS_ErrRec>();
        private ErrRecBll()
        {
        }
        public static ErrRecBll Instance = new ErrRecBll();
    }
}
