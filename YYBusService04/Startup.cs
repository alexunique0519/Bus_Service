using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(YYBusService04.Startup))]
namespace YYBusService04
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
