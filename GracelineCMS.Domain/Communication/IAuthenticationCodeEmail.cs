using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GracelineCMS.Domain.Communication
{
    public interface IAuthenticationCodeEmail
    {
        Task GetCodeAndEmailUser(string email);
    }
}
