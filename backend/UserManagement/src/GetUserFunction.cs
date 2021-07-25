using System;
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
    public static class GetUserFunction
    {
        private static IDbAdapter db = new PostgresDbAdapter(
            Constants.DBHost, Constants.DBUser, Constants.DBName, Constants.DBPassword, Constants.DBPort
        );

        public static void setDBAdapter(IDbAdapter newdb) {
            GetUserFunction.db = newdb;
        }
        public static ILoggingAdapter logger = new LoggingAdapter("GET /user");

        [FunctionName("GetUser")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/{user_id:int?}")] HttpRequest req, int? user_id,
            ILogger log)
        {

            log.LogInformation("GetUserFunction HTTP trigger function received a request.");
            int jwt_id = -1;
            bool is_admin = false;
            if (((string)req.Headers[Constants.TOKEN_KEY]) == null || ((string)req.Headers[Constants.TOKEN_KEY]) == String.Empty)
            {
                logger.LogFailureMetric($"No JWT present (user_id = {user_id})", "GetUser Failures 401");
                return new StatusCodeResult(401);
            }
            string jwt_string = (string)req.Headers[Constants.TOKEN_KEY];
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
                logger.LogFailureMetric($"JWT could not be decoded (user_id = {user_id})", "GetUser Failures 400", e.ToString());
                return new BadRequestObjectResult(new { message = "JWT could not be decoded" });
            }

            log.LogInformation("GetUserFunction HTTP trigger function processed a request.");

            string msg = "Found user";
            UserProfile res;
            try
            {
                res = db.GetUser((int)user_id);
            } catch (Exception e)
            {
                string message = "Service unavailable";
                logger.LogFailureMetric(message, "GetUser Failures 503", e.ToString());
                return new StatusCodeResult(503);
            }
            if (res == null)
            {
                msg = "Could not find user";
            }
            else
            {
                // Return only public fields if requesting user does not match retrieved user
                if (jwt_id != user_id && !is_admin)
                {
                    UserPrefs prefs;
                    try 
                    {
                        prefs = db.GetUserPrefs((int)user_id);
                    } catch (Exception e)
                    {
                        string message = "Service unavailable";
                        logger.LogFailureMetric(message, "GetUser Failures 503", e.ToString());
                        return new StatusCodeResult(503);
                    }
                    if (prefs != null)
                    { // Prefs shouldnt be null, but if they are default to all fields being public
                        res = GetUserPrefs.privatizeProfile(res, prefs);
                    }
                }
            }

            logger.LogSuccessMetric($"Retrieved user (user_id = {user_id})'s profile", "GetUser Successes");
            // Return user
            return new OkObjectResult(new { message = msg, res = res?.export() });
        }

        /**CAVEAT: CANNOT QUERY USERNAMES THAT ARE NUMBERS BC {user-id:int} ROUTE WILL BE INVOKED**/
        [FunctionName("GetUser_v2")]
        public static IActionResult Run2(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/{username}")] HttpRequest req, string username,
            ILogger log)
        {
	    
            log.LogInformation("GetUserFunction HTTP trigger function received a request.");
            string jwt_string = (string)req.Headers[Constants.TOKEN_KEY];
            int jwt_id = -1;
            bool is_admin = false;
            if (((string)req.Headers[Constants.TOKEN_KEY]) == null || ((string)req.Headers[Constants.TOKEN_KEY]) == String.Empty) {
                logger.LogFailureMetric($"JWT not present (username = {username})", "GetUser Failures 401");
                return new StatusCodeResult(401);
            }

            if (((string)req.Headers[Constants.TOKEN_KEY]) != null && ((string)req.Headers[Constants.TOKEN_KEY]) != String.Empty) {
                try {
                    Claims claims = JwtDecoder.decodeString(jwt_string);
                    jwt_id = claims.user_id;
                    string role = claims.role;
                    is_admin = role.Equals("admin");
                } catch (Exception e) {
                    Console.WriteLine(e.ToString());
                    logger.LogFailureMetric($"JWT could not be decoded (user = {username})", "GetUserV2 Failures 400", e.ToString());
                    return new BadRequestObjectResult(new {message = "JWT could not be decoded"});
                }
            }

            log.LogInformation("GetUserFunction HTTP trigger function processed a request.");
            string msg = "Found user";
            UserProfile res;
            UserProfile requester;
            try
            {
                res = db.GetUser(username);
                requester = db.GetUser(jwt_id);
            } catch (Exception e)
            {
                string message = "Service unavailable";
                logger.LogFailureMetric(message, "GetUserV2 Failures 503", e.ToString());
                return new StatusCodeResult(503);
            }
            if (res == null)
            {
                msg = "Could not find user";
            }
            else
            {
                // Return only public fields if requesting user does not match retrieved user
                if (requester == null || (jwt_id != res.user_id && !is_admin))
                {
                    UserPrefs prefs;
                    try {
                        prefs = db.GetUserPrefs(res.user_id);
                    } catch (Exception e)
                    {
                        string message = "Service unavailable";
                        logger.LogFailureMetric(message, "GetUserV2 Failures 503", e.ToString());
                        return new StatusCodeResult(503);
                    }
                    if (prefs != null)
                    { // Prefs shouldnt be null, but if they are default to all fields being public
                        res = GetUserPrefs.privatizeProfile(res, prefs);
                    }
                }
                else
                {
                    Console.WriteLine((jwt_id != res.user_id && !is_admin));
                }
            }

            logger.LogSuccessMetric($"Retrieved user {username}'s profile", "GetUserV2 Successes");
            // Return user
            return new OkObjectResult(new {message = msg, res = res?.export() });
        }

    }
}
