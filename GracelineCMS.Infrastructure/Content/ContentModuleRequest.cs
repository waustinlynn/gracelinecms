using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GracelineCMS.Infrastructure.Content
{
    public class ContentModuleRequest
    {
        public required string OrganizationId { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
    }
}
