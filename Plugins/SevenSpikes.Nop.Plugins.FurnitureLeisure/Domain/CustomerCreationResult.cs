using System.Collections.Generic;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Domain
{
    public class CustomerCreationResult
    {
        public CustomerCreationResult()
        {
            Errors = new List<string>();
        }

        public bool Success { get; set; }

        public IList<string> Errors { get; set; }
    }
}
