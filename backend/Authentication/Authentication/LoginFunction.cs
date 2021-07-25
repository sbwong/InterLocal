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
    public static class LoginFunction
    {
        public static IDbAdapter db = new PostgresDbAdapter(
            Constants.DBHost, Constants.DBUser, Constants.DBName, Constants.DBPassword, Constants.DBPort
        );
        private static readonly TokenIssuer TokenIssuer = new TokenIssuer();
        private static LoggingAdapter logger = new LoggingAdapter("POST /v2/login");

        [FunctionName("LoginFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v2/login")] HttpRequest req,
            ILogger log)
        {
            dynamic data = null;
            try
            {
                string requestBody = String.Empty;
                using (StreamReader streamReader = new StreamReader(req.Body))
                {
                    requestBody = await streamReader.ReadToEndAsync();
                }
                data = JsonConvert.DeserializeObject(requestBody);
            }
            catch (Exception)
            {
                string message = "Bad request syntax";
                logger.logMetric(message, "LoginFunction User Failures", 1);
                return new BadRequestObjectResult(message);
            }

            string username = data?.username;
            string password = data?.password;

            // Verify that required fields are present in request.
            if (username == null || password == null)
            {
                string message = "Incorrect request format. Expected username and password.";
                logger.logMetric(message, "LoginFunction User Failures", 1);
                return new BadRequestObjectResult(message);
            }

            // Get the user id associated with the provided username, if one exists.
            int user_id = -1;
            try
            {
                user_id = db.GetIDByUsername(username);
            }
            catch (Exception)
            {
                string message = "Service unavailable";
                logger.logMetric(message, "LoginFunction DB Failures", 1, "User Journey Centered Metrics");
                return new StatusCodeResult(503);
            }

            if (user_id == -1)
            {
                string message = "Username doesn't exist";
                logger.logMetric(message, "LoginFunction User Failures", 1);
                return new UnauthorizedResult();
            }

            // Look up userid in credential table and compare passwords.
            string encrypted_password;
            try
            {
                encrypted_password = db.LookupPasswordByUserId(user_id);
            }
            catch (Exception)
            {
                string message = "Service unavailable";
                logger.logMetric(message, "LoginFunction DB Failures", 1, "User Journey Centered Metrics");
                return new StatusCodeResult(503);
            }

            if (!PasswordEncryption.Verify(encrypted_password, password))
            {
                logger.logMetric(String.Format("User {0} entered wrong password.", username), "Incorrect Password", 1, "User Journey Centered Metrics");
                return new UnauthorizedResult();
            }

            string status;
            try
            {
                status = db.GetStatusByID(user_id);
            }
            catch (Exception)
            {
                string message = "Service unavailable";
                logger.logMetric(message, "LoginFunction DB Failures", 1, "User Journey Centered Metrics");
                return new StatusCodeResult(503);
            }

            if (status == null)
            {
                string message = "Status doesn't exist";
                logger.logMetric(message, "LoginFunction User Failures", 1);
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
            // Log success
            logger.logMetric(String.Format("User {0} successfully logged in.", username), "Successful Login Attempts", 1, LoggingAdapter.SUCCESS_NAMESPACE_ID);

            return new OkObjectResult(new { status, user_id });
        }
    }
}