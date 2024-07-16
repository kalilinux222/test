using Nop.Core;
using Nop.Data;
using Nop.Plugin.Tax.Avalara.Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Nop.Plugin.Tax.Avalara.Data
{
    /// <summary>
    /// Represents custom object context of Avalara tax provider
    /// </summary>
    public class TaxTransactionLogObjectContext : DbContext, IDbContext
    {
        #region Ctor
        
        public TaxTransactionLogObjectContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            //((IObjectContextAdapter) this).ObjectContext.ContextOptions.LazyLoadingEnabled = true;
        }

        #endregion

        #region Utilities

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new TaxTransactionLogMap());

            base.OnModelCreating(modelBuilder);
        }

        #endregion

        #region Methods

        public string CreateDatabaseScript()
        {
            return ((IObjectContextAdapter)this).ObjectContext.CreateDatabaseScript();
        }

        public new IDbSet<TEntity> Set<TEntity>() where TEntity : BaseEntity
        {
            return base.Set<TEntity>();
        }

        public IList<TEntity> ExecuteStoredProcedureList<TEntity>(string commandText, params object[] parameters) where TEntity : BaseEntity, new()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TElement> SqlQuery<TElement>(string sql, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public int ExecuteSqlCommand(string sql, bool doNotEnsureTransaction = false, int? timeout = null, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public void Detach(object entity)
        {
            throw new NotImplementedException();
        }

        public bool ProxyCreationEnabled { get; set; }
        public bool AutoDetectChangesEnabled { get; set; }

        public void Install()
        {
            //create the table
            var dbScript = CreateDatabaseScript();
            Database.ExecuteSqlCommand(dbScript);
            SaveChanges();
        }

        /// <summary>
        /// Uninstall object context
        /// </summary>
        public void Uninstall()
        {
            //drop the table
            this.DropPluginTable(nameof(TaxTransactionLog));
        }

        #endregion
    }
}