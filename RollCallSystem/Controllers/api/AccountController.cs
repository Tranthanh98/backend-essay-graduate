using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using RollCallSystem.Models;
using RollCallSystem.Services;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace RollCallSystem.Controllers.api
{
    public class AccountController : BaseApiController
    {
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return OwinHttpRequestMessageExtensions.GetOwinContext(this.Request).Authentication;
            }
        }
        [HttpPost]
        public ApiResult<User> Login(LoginModel login)
        {
            var apiResult = RCSService.Login(login);
            if (apiResult.IsSuccess)
            {
                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, apiResult.Data.Id.ToString()));
                claims.Add(new Claim(ClaimTypes.Role, apiResult.Data.Role.ToString()));
                var userIdentity = new UserIdentity()
                {
                    AuthenticationType = CookieAuthenticationDefaults.AuthenticationType,
                    IsAuthenticated = true
                };
                var claimIdentity = new ClaimsIdentity(userIdentity, claims);
                AuthenticationManager.SignIn(claimIdentity);
            }
            return apiResult;
        }
        [HttpGet]
        public ApiResult<User> Logout()
        {
            var apiResult = new ApiResult<User>();
            AuthenticationManager.SignOut();
            apiResult.IsSuccess = true;
            return apiResult;
        }
        [HttpGet]
        public ApiResult<User> CheckLogin()
        {
            var apiResult = new ApiResult<User>();
            if (ClaimsPrincipal.Current.Identity.IsAuthenticated == true)
            {
                apiResult = RCSService.GetCurrentUser();
            }
            else
            {
                apiResult.IsSuccess = false;
            }
            return apiResult;
        }
        [HttpGet]
        public ApiResult<User> GetCurrentUser()
        {
            return RCSService.GetCurrentUser();
        }
        [HttpPost]
        public ApiResult<List<User>> CreateAccount(List<UserCreateModel> newUsers)
        {
            return RCSService.CreateAccount(newUsers);
        }
    }
}
