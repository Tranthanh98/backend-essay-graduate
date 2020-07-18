using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using System.Web;
using System.Web.Routing;

namespace RollCallSystem
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            CookieAuthenticationProvider provider = new CookieAuthenticationProvider();

            var originalHandler = provider.OnApplyRedirect;

            //Our logic to dynamically modify the path (maybe needs some fine tuning)
            provider.OnApplyRedirect = context =>
            {
                var mvcContext = new HttpContextWrapper(HttpContext.Current);
                var routeData = RouteTable.Routes.GetRouteData(mvcContext);

                //Get the current language  
                RouteValueDictionary routeValues = new RouteValueDictionary();


                //Reuse the RetrunUrl
                Uri uri = new Uri(context.RedirectUri);
                string returnUrl = HttpUtility.ParseQueryString(uri.Query)[context.Options.ReturnUrlParameter];
                routeValues.Add(context.Options.ReturnUrlParameter, returnUrl);
                originalHandler.Invoke(context);
            };

            var expiredTime = DateTime.Now.AddYears(1).ToLocalTime();
            // Configure the db context and user manager to use a single instance per request
            //app.CreatePerOwinContext(ApplicationDbContext.Create);
            //app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider

            app.UseCookieAuthentication(new CookieAuthenticationOptions()
            {
                ExpireTimeSpan = expiredTime - DateTime.Now,
                Provider = provider
            });
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
        }
    }
}