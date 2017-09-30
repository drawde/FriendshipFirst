using FriendshipFirst.Common;
using FriendshipFirst.Common.Enum;
using FriendshipFirst.Common.JsonModel;
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
    public class GameRecordBll : BaseBLL<FF_GameRecord>
    {
        private IRepository<FF_GameRecord> _repository = new Repository<FF_GameRecord>();
        private GameRecordBll()
        {
        }
        public static GameRecordBll Instance = new GameRecordBll();

        public APIResultBase GetRecord(string roundCode, string userCode)
        {
            var where = LDMFilter.True<FF_GameRecord>();
            where = where.And(c => c.RoundCode == roundCode && c.UserCode == userCode);
            var lst = _repository.GetList(where, "AddTime", false).Result;
            if (lst != null && lst.TotalItemsCount > 0)
            {
                return JsonModelResult.PackageSuccess(lst.Items.FirstOrDefault());
            }
            return JsonModelResult.PackageFail(OperateResCodeEnum.查询不到需要的数据);
        }

        
    }
}
