using com.chalkline.wopi.Security;
using com.chalkline.wopi.Utils;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace com.chalkline.wopi.Controllers
{
    [WopiTokenValidationFilter]
    public class foldersController : ApiController
    {
        [WopiTokenValidationFilter]
        [HttpGet]
        [Route("wopi/folders/{id}")]
        public async Task<HttpResponseMessage> Get(Guid id)
        {
            return await HttpContext.Current.ProcessWopiRequest();
        }

        [WopiTokenValidationFilter]
        [HttpGet]
        [Route("wopi/folders/{id}/contents")]
        public async Task<HttpResponseMessage> Contents(Guid id)
        {
            return await HttpContext.Current.ProcessWopiRequest();
        }

        [WopiTokenValidationFilter]
        [HttpPost]
        [Route("wopi/folders/{id}")]
        public async Task<HttpResponseMessage> Post(Guid id)
        {
            return await HttpContext.Current.ProcessWopiRequest();
        }

        [WopiTokenValidationFilter]
        [HttpPost]
        [Route("wopi/folders/{id}/contents")]
        public async Task<HttpResponseMessage> PostContents(Guid id)
        {
            return await HttpContext.Current.ProcessWopiRequest();
        }
    }
}
