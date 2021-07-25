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
    public static class RegisterFunction
    {
        public static IDbAdapter db = new PostgresDbAdapter(
            Constants.DBHost, Constants.DBUser, Constants.DBName, Constants.DBPassword, Constants.DBPort
        );
        private static readonly TokenIssuer TokenIssuer = new TokenIssuer();
        private static LoggingAdapter logger = new LoggingAdapter("POST /register");

        [FunctionName("RegisterFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "register")] HttpRequest req,
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
                logger.logMetric(message, "RegisterFunction User Failures", 1);
            }
            string username = data?.username;
            string password = data?.password;

            if (username == null || username == "" || password == null || password == "")
            {
                string message = "Incorrect request format. Expected username and password.";
                logger.logMetric(message, "RegisterFunction User Failures", 1);
                return new BadRequestObjectResult(message);
            }

            int userId = -1;
            try
            {
                userId = db.GetIDByUsername(username);
            }
            catch (Exception)
            {
                string message = "Service unavailable";
                logger.logMetric(message, "RegisterFunction DB Failures", 1);
                return new StatusCodeResult(503);
            }

            if (userId == -1)
            {
                logger.logMetric(String.Format("User {0} already exists.", username), "RegisterFunction User Failures", 1);
                return new UnauthorizedResult();
            }

            bool idExist = true;
            try
            {
                idExist = db.DoesIdExist(userId);
            }
            catch (Exception)
            {
                string message = "Service unavailable";
                logger.logMetric(message, "RegisterFunction DB Failures", 1);
                return new StatusCodeResult(503);
            }

            if (idExist)
            {
                string message = "User ID already exists.";
                logger.logMetric(message, "RegisterFunction User Failures", 1);
                return new ConflictObjectResult(message);
            }

            string encryptedPassword = PasswordEncryption.Encrypt(password);

            try
            {
                await db.CreateCredentialsEntry(userId, encryptedPassword);
            }
            catch (Exception)
            {
                string message = "Service unavailable";
                logger.logMetric(message, "RegisterFunction DB Failures", 1);
                return new StatusCodeResult(503);
            }
            // Log success
            logger.logMetric(String.Format("User {0} successfully signed up.", username), "Successful Registrations", 1, "User Journey Centered Metrics");

            return new OkObjectResult("OK");
        }
    }
}