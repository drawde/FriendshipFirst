using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FriendshipFirst.DAL
{
    public interface IUnitOfWork
    {
        bool IsCommitted { get; }
        int Commit();
        void Rollback();
    }
}
