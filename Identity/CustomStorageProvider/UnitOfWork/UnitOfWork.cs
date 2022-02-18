using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using ObjectRelationMapping.Interfaces;
using ObjectRelationMapping.Repository;

namespace ObjectRelationMapping.UnitOfWork
{
    public class UnitOfWork :
        IUnitOfWork
    {
        private readonly Dictionary<Type, object> repositories;
        private bool disposedValue = false;

        public UnitOfWork(string connectionString)
        {
            this.Context = new SqlConnection(connectionString);
            try
            {
                this.Context.Open();
                this.repositories = new Dictionary<Type, object>();
                Console.WriteLine("Connection is executed");
            }
            catch
            {
                Console.WriteLine("Connection isn't executed");
            }
        }

        public SqlConnection Context { get; set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IRepository<TEntity> GetRepository<TEntity>()
            where TEntity : IEntityBase
        {
            if (this.repositories.Keys.Contains(typeof(TEntity)))
            {
                return this.repositories[typeof(TEntity)] as IRepository<TEntity>;
            }

            var repository = new Repository<TEntity>(this.Context);
            this.repositories.Add(typeof(TEntity), repository);
            return repository;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposedValue)
            {
                return;
            }

            if (disposing)
            {
                this.Context.Close();
            }

            this.disposedValue = true;
        }
    }
}