using System;

using Microsoft.AspNetCore.Http;

namespace FunctionApp
{
    /// <summary>
    /// Wrapper class to obtain the user_id from either JWT or request body
    /// </summary>
    public static class UserIDWrapper
    {
        /// <summary>
        /// Retrieves the user_id from the JWT or request body. If both are given,
        /// checks for equality.
        /// </summary>
        /// <param name="uidStr">user_id from request body</param>
        /// <param name="jwt_uidStr">user_id from JWT</param>
        /// <returns>user_id or appropriate error code</returns>
        public static int getUID(string uidStr, string jwt_uidStr)
        {
            int uid = -1;
            int jwt_uid = -1;
            try
            {
                if (!string.IsNullOrEmpty(uidStr))
                {
                    uid = Int32.Parse(uidStr);
                }
                if (!string.IsNullOrEmpty(jwt_uidStr))
                {
                    jwt_uid = Int32.Parse(jwt_uidStr);
                }
            }
            catch (FormatException)
            {
                return -400;
            }
            

            if (uid != -1 && jwt_uid != -1)
            {
                if (uid != jwt_uid)
                {
                    return -403;
                }
            }
            else if (uid == -1 && jwt_uid != -1)
            {
                uid = jwt_uid;
            }
            else if (uid == -1 && jwt_uid == -1)
            {
                return -400;
            }

            return uid;
        }
    }
}