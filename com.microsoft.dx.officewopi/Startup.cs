using Microsoft.IdentityModel.Logging;
using Owin;

namespace com.chalkline.wopi
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
