using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Authentication
{
    public static class LogoutFunction
    {
        private static LoggingAdapter logger = new LoggingAdapter("POST /logout");

        [FunctionName("LogoutFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "logout")] HttpRequest req,
            ILogger log)
        {
            string jwt_string = req.Headers[Constants.TOKEN_KEY];
            Claims claims = JwtDecoder.decodeString(jwt_string);
            if (claims == null)
            {
                string message = "Invalid JWT";
                logger.logMetric(message, "LogoutFunction User Failures", 1);
                return new UnauthorizedResult();
            }
            int user_id = claims.user_id;
            // Set up cookie parameters.
            CookieOptions option = new CookieOptions();
            if (Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") != "Development")
            {
                option.SameSite = SameSiteMode.None;
                option.Secure = true;
            }
            // Clear the access token from the client's cookies.
            req.HttpContext.Response.Cookies.Delete(Constants.TOKEN_KEY, option);
            // Log success
            logger.logMetric(String.Format("User ID {0} successfully logged out.", user_id), "Successful Logout Attempts", 1, LoggingAdapter.SUCCESS_NAMESPACE_ID);

            // Return an OK response.
            return new OkObjectResult("OK");
        }
    }
}
