using SevenSpikes.Nop.Framework.Data;
using SevenSpikes.Nop.Plugins.UltimateCarousel.Mappings.Carousels;
using System.Collections.Generic;
using System.Data.Entity;

namespace SevenSpikes.Nop.Plugins.UltimateCarousel.Data
{
    /// <summary>
    /// Object context
    /// </summary>
    public class UltimateCarouselObjectContext : Base7SpikesObjectContext
    {
        public UltimateCarouselObjectContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {

        }
        
        public override void AddEntityFrameworkMappings(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new CarouselMap());
            modelBuilder.Configurations.Add(new CarouselItemMap());
        }

        protected override List<string> GetTablesToDrop()
        {
            var tablesForDropping = new List<string>
            {
                "SS_UC_CarouselItem",
                "SS_UC_Carousel"
            };

            return tablesForDropping;
        }

        /// <summary>
        /// It's required to set initializer to null (for SQL Server Compact).
        /// otherwise, you'll get something like "The model backing the 'your context name' context has changed since the database was created. Consider using Code First Migrations to update the database"
        /// </summary>
        protected override void SetDatabaseInitializerToNull()
        {
            Database.SetInitializer<UltimateCarouselObjectContext>(null);
        }
    }
}