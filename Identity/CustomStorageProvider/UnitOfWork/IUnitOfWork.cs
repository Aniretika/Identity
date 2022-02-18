using System;
using ObjectRelationMapping.Interfaces;

namespace ObjectRelationMapping.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<T> GetRepository<T>()
            where T : IEntityBase;
    }
}