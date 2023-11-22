using Owin;
using Microsoft.IdentityModel.Logging;

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
