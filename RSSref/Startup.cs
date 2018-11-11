using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(RSSref.Startup))]
namespace RSSref
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
