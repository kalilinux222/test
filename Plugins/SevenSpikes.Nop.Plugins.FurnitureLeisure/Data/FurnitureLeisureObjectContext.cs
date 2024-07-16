using SevenSpikes.Nop.Framework.Data;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Domain;
using System.Collections.Generic;
using System.Data.Entity;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Data
{
    public class FurnitureLeisureObjectContext : Base7SpikesObjectContext
    {
        public FurnitureLeisureObjectContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
        }

        public override void AddEntityFrameworkMappings(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new ProductSearchTermMap());
        }

        public DbSet<ProductSearchTerms> ProductSearchTerm { get; set; }

        protected override List<string> GetTablesToDrop()
        {
            var tablesForDropping = new List<string>
            {
                "SS_FL_ProductSearchTerms"
            };

            return tablesForDropping;
        }

        protected override void SetDatabaseInitializerToNull()
        {
            Database.SetInitializer<FurnitureLeisureObjectContext>(null);
        }
    }
}
