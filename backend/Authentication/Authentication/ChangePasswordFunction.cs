using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;

namespace Authentication
{
    public static class ChangePasswordFunction
    {
        public static IDbAdapter db = new PostgresDbAdapter(
            Constants.DBHost, Constants.DBUser, Constants.DBName, Constants.DBPassword, Constants.DBPort
        );
        private static LoggingAdapter logger = new LoggingAdapter("PUT /password");
        private static int MIN_PASSWORD_LENGTH = 8;

        [FunctionName("ChangePasswordFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "password")] HttpRequest req,
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
                logger.logMetric(message, "ChangePasswordFunction User Failures", 1);
                return new BadRequestObjectResult(message);
            }
            string currentPassword = data?.currentPassword;
            string newPassword = data?.newPassword;
            if (currentPassword == null || newPassword == null)
            {
                string message = "Incorrect request format. Expected current and new password.";
                logger.logMetric(message, "ChangePasswordFunction User Failures", 1);
                return new BadRequestObjectResult(message);
            }

            int length = newPassword.Length;
            bool isLetterPresent = newPassword.Any(c => char.IsLetter(c));
            bool isNonLetterPresent = newPassword.Any(c => !char.IsLetter(c));
            bool isWhiteSpacePresent = newPassword.Any(c => char.IsWhiteSpace(c));
            if (length < MIN_PASSWORD_LENGTH || !isNonLetterPresent || !isLetterPresent || isWhiteSpacePresent)
            {
                string message = String.Format("Password failed to meet requirements");
                logger.logMetric(message, "ChangePasswordFunction User Failures", 1);
                return new BadRequestObjectResult(message);
            }

            // Extract the userid from the JWT.
            Claims jwt = JwtDecoder.decodeString(req.Headers[Constants.TOKEN_KEY]);
            if (jwt == null)
            {
                string message = "Invalid JWT";
                logger.logMetric(message, "ChangePasswordFunction User Failures", 1);
                return new UnauthorizedResult();
            }

            int user_id = jwt.user_id;
            // Check if user provided the correct current password.
            string encrypted_password;
            try
            {
                encrypted_password = db.LookupPasswordByUserId(user_id);
            }
            catch (Exception)
            {
                string message = "Service unavailable";
                logger.logMetric(message, "ChangePasswordFunction DB Failures", 1);
                return new StatusCodeResult(503);
            }

            if (!PasswordEncryption.Verify(encrypted_password, currentPassword))
            {
                logger.logMetric(String.Format("User ID {0} entered incorrect current password.", user_id), "ChangePasswordFunction User Failures", 1);
                return new UnauthorizedResult();
            }
            // Update password to new password in the database.
            try
            {
                await db.UpdateCredentialsEntry(user_id, PasswordEncryption.Encrypt(newPassword));
            }
            catch (Exception)
            {
                string message = "Service unavailable";
                logger.logMetric(message, "ChangePasswordFunction DB Failures", 1);
                return new StatusCodeResult(503);
            }
            // log success
            logger.logMetric(String.Format("User ID {0} successfully changed password.", user_id), "Successful password change", 1, LoggingAdapter.SUCCESS_NAMESPACE_ID);
            return new OkObjectResult("OK");
        }
    }
}
