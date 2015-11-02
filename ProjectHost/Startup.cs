using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ProjectHost.Startup))]
namespace ProjectHost
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
