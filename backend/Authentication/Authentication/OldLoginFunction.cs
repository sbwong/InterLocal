using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;

namespace Authentication
{
    public static class OldLoginFunction
    {
        public static IDbAdapter db = new PostgresDbAdapter(
            Constants.DBHost, Constants.DBUser, Constants.DBName, Constants.DBPassword, Constants.DBPort
        );
        private static readonly TokenIssuer TokenIssuer = new TokenIssuer();

        [FunctionName("OldLoginFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "login")] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string username = data?.username;

            // Verify that required fields are present in request.
            if (username == null)
            {
                return new BadRequestObjectResult("Incorrect request format. Expected username and password.");
            }
            // Get the user id associated with the provided username, if one exists.
            int user_id = db.GetIDByUsername(username);
            if (user_id == -1)
            {
                return new UnauthorizedResult();
            }

            string status = db.GetStatusByID(user_id);
            if (status == null)
            {
                return new UnauthorizedResult();
            }

            // Set up cookie parameters
            CookieOptions option = new CookieOptions();
            option.Expires = DateTimeOffset.Now.AddDays(1);
            option.HttpOnly = true;
            bool inDevelopment = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") == "Development";
            if (!inDevelopment)
            {
                option.SameSite = SameSiteMode.None;
                option.Secure = true;
            }

            var credentials = new Credentials
            {
                username = username,
                user_id = user_id,
                status = status
            };
            // Attach the access token as a cookie value
            req.HttpContext.Response.Cookies.Append(Constants.TOKEN_KEY, TokenIssuer.IssueTokenForUser(credentials), option);

            return new OkObjectResult(new { user_id, status });
        }
    }
}