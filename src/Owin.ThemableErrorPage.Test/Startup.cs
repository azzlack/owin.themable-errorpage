using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Owin.ThemableErrorPage.Test.Startup))]

namespace Owin.ThemableErrorPage.Test
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseThemableErrorPage();

            app.Run(
                (x) =>
                    {
                        throw new Exception("Owin.ThemableErrorPage.Test");
                    });
        }
    }
}
