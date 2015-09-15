using System.Collections.Generic;
using System.Linq;

namespace Pipes.Models.Resources
{
    internal class ResourceGroup
    {
        private ResourceGroup()
        {
            
        }

        public static ResourceGroup CreateNew()
        {
            return new ResourceGroup();
        }

        public static ResourceGroup AcquireResources(IReadOnlyCollection<Resource> resources)
        {
            var acquiredResources = new List<ResourceIdentifier>();
            foreach (var resourceIdentifier in resources.OrderBy(r => r.GetCurrentIdentifier()))
            {
                
            }
        }
    }
}
