using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GracelineCMS.Infrastructure.Auth
{
    public class AuthCodeValidationRequest
    {
        public required string EmailAddress { get; set; }
        public required string AuthCode { get; set; }
    }
}
