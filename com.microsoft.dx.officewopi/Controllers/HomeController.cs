using com.chalkline.wopi.Models;
using com.chalkline.wopi.Models.Wopi;
using com.chalkline.wopi.Security;
using com.chalkline.wopi.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace com.chalkline.wopi.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        /// <summary>
        /// Index view displays all files for the signed in user
        /// </summary>
        [Authorize]
        public async Task<ActionResult> Index()
        {
            // Get files for the user
            var filesSQL = AzureSQLUtil.GetItems(User.Identity.Name.ToLower());

            // Populate valid actions for each of the files
            await filesSQL.PopulateActions();

            // Return the view with the files
            return View(filesSQL);
        }

        /// <summary>
        /// Detail view hosts the WOPI host frame and loads the appropriate action view from Office Online
        /// </summary>
        [Authorize]
        [Route("Home/Detail/{id}")]
        public async Task<ActionResult> Detail(Guid id)
        {
            // Make sure an action was passed in
            if (String.IsNullOrEmpty(Request["action"]))
                return RedirectToAction("Error", "Home", new { error = "No action provided" });

            //// Get the specific file from SQLDB
            //var fileBlob = AzureSQLUtil.GetBlob(id.ToString(), User.Identity.Name.ToLower());

            //// Check for null file
            //if (fileBlob == null)
            //    return RedirectToAction("Error", "Home", new { error = "Files does not exist" });

            // Use discovery to determine endpoint to leverage
            List<WopiAction> discoData = await WopiUtil.GetDiscoveryInfo();
            var fileExt = "docx";
            var action = discoData.FirstOrDefault(i => i.name == Request["action"] && i.ext == fileExt);

            // Make sure the action isn't null
            if (action != null)
            {
                string urlsrc = WopiUtil.GetActionUrl(action, id.ToString(), Request.Url.Authority);

                // Generate JWT token for the user/document
                WopiSecurity wopiSecurity = new WopiSecurity();
                var token = wopiSecurity.GenerateToken(User.Identity.Name.ToLower(), id.ToString());
                ViewData["access_token"] = wopiSecurity.WriteToken(token);
                ViewData["access_token_ttl"] = token.ValidTo.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
                ViewData["wopi_urlsrc"] = urlsrc;
                return View();
            }
            else
            {
                // This will only hit if the extension isn't supported by WOPI
                return RedirectToAction("Error", "Home", new { error = "File is not a supported WOPI extension" });
            }
        }

        /// <summary>
        /// Adds the submitted files for Azure Blob Storage and metadata into DocumentDB
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Add()
        {
            try
            {
                // Create the file entity
                DetailedFileModel file = new DetailedFileModel()
                {
                    id = Guid.NewGuid(),
                    OwnerId = User.Identity.Name.ToLower(),
                    BaseFileName = HttpUtility.UrlDecode(Request["HTTP_X_FILE_NAME"]),
                    Size = Convert.ToInt32(Request["HTTP_X_FILE_SIZE"]),
                    Version = "1"
                };

                // Populate valid actions for each of the files
                await file.PopulateActions();

                // First stream the file into blob storage
                var stream = Request.InputStream;
                var bytes = new byte[stream.Length];
                await stream.ReadAsync(bytes, 0, (int)stream.Length);
                await AzureSQLUtil.UploadFile(file.id.ToString(), bytes, file.BaseFileName, User.Identity.Name.ToLower(), "1");

                // Return json representation of information
                return Json(new { success = true, file = file });
            }
            catch (Exception)
            {
                // Something failed...return false
                return Json(new { success = false });
            }
        }

        /// <summary>
        /// Deletes the file from Azure Blob Storage and metadata into DocumentDB
        /// </summary>
        [HttpDelete]
        [Authorize]
        [Route("Home/Delete/{id}{version}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                // Delete the file from sql db;
                await Utils.AzureSQLUtil.DeleteFile(id.ToString());

                //return json representation of information
                return Json(new { success = true, id = id.ToString() });
            }
            catch (Exception)
            {
                // Something failed...return false
                return Json(new { success = false, id = id.ToString() });
            }
        }

        /// <summary>
        /// Error view displays error messages passed from other controllers
        /// </summary>
        public ActionResult Error(string error)
        {
            ViewData["Error"] = error;
            return View();
        }

    }
}