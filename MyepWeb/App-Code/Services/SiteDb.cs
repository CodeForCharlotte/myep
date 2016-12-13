using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Site
{
    public interface IDb : IDisposable
    {
        IQueryable<T> Query<T>() where T : class;
        T Add<T>(T entity) where T : class;
        T Save<T>(T entity, bool? isNew = null) where T : class;
        T Delete<T>(T entity) where T : class;
        bool IsDetached<T>(T entity) where T : class;
        int SaveChanges();
        int ExecuteSqlCommand(string sql, params object[] parameters);
        List<T> SqlQuery<T>(string sql, params object[] parameters);
    };

    public class SiteDb : DbContext, IDb
    {
        public SiteDb(string cs) : base(cs)
        {
            Database.SetInitializer<SiteDb>(null);
        }

        public DbSet<Code> Codes { get; set; }
        public DbSet<Intern> Interns { get; set; }
        public DbSet<User> Users { get; set; }

        //REF: http://msdn.microsoft.com/en-us/data/jj819164.aspx
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.AddFromAssembly(typeof(SiteDb).Assembly);
            modelBuilder.HasDefaultSchema("dbo");
        }

        public virtual bool IsDetached<T>(T entity) where T : class
        {
            var entry = Entry(entity);
            return entry.State == EntityState.Detached;
        }

        public virtual T Attach<T>(T entity) where T : class
        {
            var entry = Entry(entity);
            if (entry.State == EntityState.Detached) Set<T>().Attach(entity);
            return entity;
        }

        public virtual T Modified<T>(T entity) where T : class
        {
            var entry = Entry(entity);
            if (entry.State == EntityState.Detached) entry.State = EntityState.Modified;
            return entity;
        }

        public virtual T Add<T>(T entity) where T : class
        {
            Entry(entity).State = EntityState.Added;
            return entity;
        }

        public virtual T Save<T>(T entity, bool? isNew = null) where T : class
        {
            var entry = Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                if (isNew == false)
                {
                    Modified(entity);
                }
                else
                {
                    Add(entity);
                }
            }
            return entity;
        }

        public virtual T Delete<T>(T entity) where T : class
        {
            var entry = Entry(entity);
            if (entry.State != EntityState.Detached) entry.State = EntityState.Deleted;
            return entity;
        }

        public void DeleteAll<T>(IEnumerable<T> entities) where T : class
        {
            entities.Each(x => Delete(x));
        }

        public IQueryable<T> Query<T>() where T : class
        {
            return Set<T>();
        }

        public List<T> SqlQuery<T>(string sql, params object[] parameters)
        {
            return Database
                .SqlQuery<T>(sql, parameters)
                .ToList();
        }

        public int ExecuteSqlCommand(string sql, params object[] parameters)
        {
            return Database
                .ExecuteSqlCommand(sql, parameters);
        }
    };
}
