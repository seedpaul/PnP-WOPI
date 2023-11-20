using Owin;
using Microsoft.IdentityModel.Logging;

namespace com.microsoft.dx.officewopi
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            IdentityModelEventSource.ShowPII = true;
            ConfigureAuth(app);
        }
    }
}
