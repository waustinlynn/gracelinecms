using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GracelineCMS.Infrastructure.Auth
{
    public class AuthCodeRequest
    {
        public required string EmailAddress { get; set; }
    }
}
