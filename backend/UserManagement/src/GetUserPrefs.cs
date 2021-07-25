using System;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Net.Http;
using JWT.Builder;
using JWT.Algorithms;

namespace UserManagement
{
    public static class GetUserPrefs
    {
        public static IDbAdapter db = new PostgresDbAdapter(
            Constants.DBHost, Constants.DBUser, Constants.DBName, Constants.DBPassword, Constants.DBPort
        );
        public static void setDBAdapter(IDbAdapter newdb) {
            GetUserPrefs.db = newdb;
        }
        public static ILoggingAdapter logger = new LoggingAdapter("GET /user/prefs");
        [FunctionName("GetUserPrefs")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/prefs/{user_id:int?}")] HttpRequest req, int? user_id,
            ILogger log)
        {
	    
            log.LogInformation("GetUserPrefs HTTP trigger function received a request.");
            int jwt_id = -1;
            if (((string)req.Headers[Constants.TOKEN_KEY]) == null || ((string)req.Headers[Constants.TOKEN_KEY]) == String.Empty)
            {
                logger.LogFailureMetric($"No JWT present (user_id = {user_id})", "GetUserPrefs Failures 401");
                return new StatusCodeResult(401);
            }
            else
            {
                string jwt_string = (string)req.Headers[Constants.TOKEN_KEY];
                try
                {
                    Claims claims = JwtDecoder.decodeString(jwt_string);
                    jwt_id = claims.user_id;
                    string role = claims.role;
                    if (user_id == null) {
                        user_id = jwt_id;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    logger.LogFailureMetric($"JWT could not be decoded (user_id = {user_id})", "GetUserPrefs Failures 400", e.ToString());
                    return new BadRequestObjectResult(new { message = "JWT could not be decoded" });
                }
            }
            UserPrefs res;
            try
            {
                res = db.GetUserPrefs((int)user_id);
            } catch (Exception e)
            {
                string message = "Service unavailable";
                logger.LogFailureMetric(message, "GetUserPrefs Failures 503", e.ToString());
                return new StatusCodeResult(503);
            }
            string msg = "Found user";
            if (res == null)
            {
                msg = "Could not find user";
                return new OkObjectResult(new {message = msg});
            }
            // Return user
            logger.LogSuccessMetric($"Retrieved user (user_id = {user_id}) preferences", "GetUserPrefs Successes");
            return new OkObjectResult(new {message = msg, res = res.export()});
        }

        public static UserProfile privatizeProfile(UserProfile user, UserPrefs prefs) {
            if (!prefs.is_email_public) {
                user.email = null;
            }
            if (!prefs.is_phone_public) {
                user.phone_number = null;
            }
            if (!prefs.is_country_public) {
                user.country = null;
            }
            if (!prefs.is_year_public) {
                user.year = null;
            }
            if (!prefs.is_residential_college_public) {
                user.college = null;
            }
            return user;
        }
    }
}