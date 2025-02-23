using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GracelineCMS.Domain.Auth
{
    public interface IAuthenticationCode
    {
        Task<string> CreateAuthCodeAsync(string email);
    }
}
