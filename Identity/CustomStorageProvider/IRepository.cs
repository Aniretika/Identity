using System;
using ObjectRelationMapping.Interfaces;

namespace ObjectRelationMapping
{
    public interface IRepository<T>
        where T : IEntityBase
    {
        int Add(T item);

        int Update(T item);

        int Delete(int id);

        T GetById(int id);

        T Include(T item, Type joinedInstance);
    }
}