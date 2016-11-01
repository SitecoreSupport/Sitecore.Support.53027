using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Sitecore.Common;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Pipelines;
using Sitecore.Pipelines.Logout;
using Sitecore.Sites;
using Sitecore.Support.Controllers;
using Sitecore.Web;
using Sitecore.Web.Pipelines.InitializeSpeakLayout;



namespace Sitecore.Support.Mvc.Pipelines.Initialize
    {
        public class InitializeAuthenticationRoute
        {
            public virtual void Process(PipelineArgs args)
            {
                Assert.ArgumentNotNull(args, "args");
                this.RegisterRoutes(RouteTable.Routes, args);
            }

            protected virtual void RegisterRoutes(RouteCollection routes, PipelineArgs args)
            {
                routes.MapRoute("Sitecore.Support.Authentication", SpeakSettings.Mvc.CommandRoutePrefix + "Authentication/Logout", new
                {
                    controller = "SupportAuthentication",
                    action = "Logout"
                }, new string[]
                {
                "Sitecore.Support.Controllers"
                });
            }
        }
}




namespace Sitecore.Support.Controllers
{
    public class ShellSiteAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            Switcher<SiteContext, SiteContextSwitcher>.Exit();
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Switcher<SiteContext, SiteContextSwitcher>.Enter(Factory.GetSite("shell"));
            new DisableAnalytics().Process(new InitializeSpeakLayoutArgs());
        }
    }
}


namespace Sitecore.Controllers
{
    using Sitecore;
    using Sitecore.Configuration;
    using Sitecore.Diagnostics;
    using Sitecore.Mvc;
    using Sitecore.Pipelines.Logout;
    using Sitecore.Security.Accounts;
    using Sitecore.Sites;
    using System;
    using System.Web.Mvc;

    [ShellSite]
    public class SupportAuthenticationController : Controller
    {
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult Logout()
        {
            SitecoreViewModelResult result = new SitecoreViewModelResult();
            User user = ClientHost.Context.User;
            if (user.IsAuthenticated)
            {
                string name = user.Identity.Name;
                SiteContext site = Factory.GetSite("shell");
                Assert.IsNotNull(site, $"Configuration for '{"shell"}' site not found.");
                using (new SiteContextSwitcher(site))
                {
                    ClientHost.Pipelines.Run("speak.logout", new LogoutArgs());
                    ((dynamic)result.Result).Redirect = Context.Site.LoginPage;
                    ((dynamic)result.Result).Success = true;
                }
                Log.Audit($"Logout: {name}.", this);
            }
            return result;
        }
    }
}

namespace Sitecore.Support.Pipelines.Logout
{
    public class ClearEECookie
    {
        public void Process(LogoutArgs args)
        {
            foreach (Site current in SiteManager.GetSites())
            {
                WebUtil.SetCookieValue(Factory.GetSite(current.Name).GetCookieKey("sc_mode"), string.Empty);
            }
            WebUtil.SetCookieValue("sc_mode", string.Empty);
        }
    }
}
