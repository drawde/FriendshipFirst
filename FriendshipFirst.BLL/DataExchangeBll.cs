using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FriendshipFirst.DAL;
using FriendshipFirst.DAL.Impl;
using FriendshipFirst.Model;
using FriendshipFirst.Common.Util;
using FriendshipFirst.Common;

namespace FriendshipFirst.BLL
{
    public class DataExchangeBll : BaseBLL<HS_DataExchange>
    {
        private IRepository<HS_DataExchange> _repository = new Repository<HS_DataExchange>();
        private DataExchangeBll()
        {
        }
        public static DataExchangeBll Instance = new DataExchangeBll();

        public async Task AsyncInsert(string Action, string Controller, string QueryData, string ResultData, DataSourceEnum dataSource = DataSourceEnum.API)
        {
            using (FriendshipFirstContext context = new FriendshipFirstContext())
            {
                HS_DataExchange rec = new HS_DataExchange();
                rec.Action = Action;
                rec.AddTime = DateTime.Now;
                rec.Controller = Controller;
                rec.IP = StringUtil.GetIP();
                rec.QueryData = QueryData;
                rec.ResultData = ResultData;
                rec.URL = "/" + rec.Controller + "/" + rec.Action;
                rec.DataSource = (int)dataSource;
                //rec.DataCode = RandomUtil.CreateRandomStr(10);

                context.hs_dataexchange.Add(rec);
                var res = context.Entry(rec).GetValidationResult();
                if (res.IsValid)
                {
                    await context.SaveChangesAsync();
                }
                else
                {
                    Log.Default.Debug(res.ValidationErrors.ToJsonString());
                }
            }
            //HS_DataExchange rec = new HS_DataExchange();
            //rec.Action = Action;
            //rec.AddTime = DateTime.Now;
            //rec.Controller = Controller;
            //rec.IP = StringUtil.GetIP();
            //rec.QueryData = QueryData;
            //rec.ResultData = ResultData;
            //rec.URL = "/" + rec.Controller + "/" + rec.Action;
            //rec.DataSource = (int)dataSource;
            //rec.DataCode = RandomUtil.CreateRandomStr(10);
            //_repository.Insert(rec);

            //var res = Insert(rec);
            //AsyncInsert(rec);
        }
    }
}
