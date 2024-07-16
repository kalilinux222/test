using Nop.Data.Mapping;
using Nop.Plugin.Tax.Avalara.Domain;

namespace Nop.Plugin.Tax.Avalara.Data
{
    public partial class TaxTransactionLogMap : NopEntityTypeConfiguration<TaxTransactionLog>
    {
        public TaxTransactionLogMap()
        {
            this.ToTable(nameof(TaxTransactionLog));
            this.HasKey(x => x.Id);
        }
    }
}