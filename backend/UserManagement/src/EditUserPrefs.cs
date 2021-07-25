using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;

namespace UserManagement
{
    public static class EditUserPrefs
    {
         private static IDbAdapter db = new PostgresDbAdapter(
            Constants.DBHost, Constants.DBUser, Constants.DBName, Constants.DBPassword, Constants.DBPort
        );

        public static void setDBAdapter(IDbAdapter newdb) {
            EditUserPrefs.db = newdb;
        }
       public static ILoggingAdapter logger = new LoggingAdapter("POST /user/prefs");

        [FunctionName("EditUserPrefs")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "user/prefs/{user_id:int?}")] HttpRequest req, int? user_id,
            ILogger log)
        {
            log.LogInformation("EditUserFunction HTTP trigger function received a request.");
            int jwt_id = -1;
            bool is_admin = false;
            string jwt_string = (string)req.Headers[Constants.TOKEN_KEY];
            if (jwt_string == null || jwt_string == string.Empty)
            {
                logger.LogFailureMetric($"JWT not present", "EditUserPrefs Failures 401");
                return new UnauthorizedResult();
            }

            try
            {
                Claims claims = JwtDecoder.decodeString(jwt_string);
                jwt_id = claims.user_id;
                string role = claims.role;
                is_admin = role.Equals("admin");
                if (user_id == null) user_id = jwt_id;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                logger.LogFailureMetric($"JWT could not be decoded (user_id = {user_id})", "EditUserPrefs Failures 400", e.ToString());
                return new BadRequestObjectResult(new { message = "JWT could not be decoded" });
            }

            if (user_id != jwt_id && !is_admin)
            {
                logger.LogFailureMetric($"Forbidden attempt to edit user (user_id = {user_id}) preferences.", "EditUserPrefs Failures 403");
                return new StatusCodeResult(403);
            }

            dynamic data;
            string requestBody = String.Empty;
            try
            {
                using (StreamReader streamReader = new StreamReader(req.Body))
                {
                    requestBody = await streamReader.ReadToEndAsync();
                }
                data = JsonConvert.DeserializeObject(requestBody);
            } catch (Exception e) {
                string message = "Bad request syntax";
                logger.LogFailureMetric($"{message} (user_id = {user_id})", "EditUserPrefs Failures 400", e.ToString());
                return new BadRequestObjectResult(message);
            }

            // Confirm request has all required fields.
            if (data == null)
            {
                logger.LogFailureMetric($"Empty request body (user_id = {user_id})", "EditUserPrefs Failures 400");
                return (ActionResult)new BadRequestResult();
            }

            string msg = "Updated user";
            int nRows;
            try
            {
                nRows = await db.EditUserPrefsAsync((int)user_id, data);
            } catch (Exception e)
            {
                string message = "Service unavailable";
                logger.LogFailureMetric(message, "EditUserPrefs Failures 503", e.ToString());
                return new StatusCodeResult(503);
            }
            if (nRows == 0)
            {
                msg = "Could not find user";
            }
            logger.LogSuccessMetric($"User (user_id = {user_id}) preferences were updated.", "EditUserPrefs Successes");
            return new OkObjectResult(new {message = msg});
        }

    }
}
