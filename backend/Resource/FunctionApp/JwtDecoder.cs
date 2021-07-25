using System;
using System.Collections.Generic;

using JWT.Builder;
using JWT.Algorithms;
using Microsoft.AspNetCore.Http;

namespace FunctionApp
{
    public static class JwtDecoder
    {
        public static readonly string secret = "COMP_410_SPRING_2021_SECRET_KEY";

        public static readonly string token_key = "token";

        /// <summary>
        /// Decodes the JWT string into a dictionary of fields
        /// </summary>
        /// <param name="jwt_string">The JWT string</param>
        /// <returns>
        /// A dictionary with the following fields:
        ///  user_id: int
        ///  username: string
        ///  role: string
        ///  exp: int
        /// </returns>
        public static Claims decodeString(dynamic data, HttpRequest req)
        {
            string jwt_string = req.Headers[token_key];
            string uid = data?.user_id;
            try
            {
                var claims = JwtBuilder.Create().WithAlgorithm(new HMACSHA256Algorithm()).WithSecret(secret).MustVerifySignature().Decode<IDictionary<string, object>>(jwt_string);
                string jwt_uid = Convert.ToString(claims["user_id"]);
                int user_id = UserIDWrapper.getUID(uid, jwt_uid);

                return new Claims
                {
                    user_id = user_id,
                    role = Convert.ToString(claims["role"])
                };
            }
            catch (Exception e)
            {
                return new Claims
                {
                    user_id = -400,
                    role = ""
                };
            }
        }

        public static Claims decodeString(HttpRequest req)
        {
            string jwt_string = req.Headers[token_key];
            try
            {
                if (string.IsNullOrEmpty(jwt_string))
                {
                    return new Claims
                    {
                        user_id = -200,
                        role = ""
                    };
                }
                var claims = JwtBuilder.Create().WithAlgorithm(new HMACSHA256Algorithm()).WithSecret(secret).MustVerifySignature().Decode<IDictionary<string, object>>(jwt_string);
                string jwt_uid = Convert.ToString(claims["user_id"]);
                return new Claims
                {
                    user_id = Convert.ToInt32(jwt_uid),
                    role = Convert.ToString(claims["role"])
                };
            }
            catch (Exception e)
            {
                return new Claims
                {
                    user_id = -400,
                    role = ""
                };
            }
        }
    }

    public class Claims
    {
        public int user_id { get; set; }
        public string role { get; set; }
    }
}