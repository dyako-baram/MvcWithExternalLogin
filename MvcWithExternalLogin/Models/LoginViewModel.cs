using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace MvcWithExternalLogin.Models
{
    public class LoginViewModel
    {
        public string ReturnUrl { get; set; }
        public IList<AuthenticationScheme> ExternalProviders { get; set; }
    }
}
