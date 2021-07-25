using System;
using System.Collections.Generic;
using JWT.Algorithms;
using JWT.Builder;

namespace Authentication
{
    public static class JwtDecoder
    {
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
        public static Claims decodeString(string jwt_string)
        {
            try
            {
                var claims = JwtBuilder.Create()
                        .WithAlgorithm(new HMACSHA256Algorithm()) // symmetric
                        .WithSecret(Constants.SECRET_KEY)
                        .MustVerifySignature()
                        .Decode<IDictionary<string, object>>(jwt_string);
                return new Claims
                {
                    user_id = Convert.ToInt32(claims["user_id"]),
                    role = Convert.ToString(claims["role"])
                };
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public class Claims
    {
        public int user_id { get; set; }
        public string role { get; set; }
    }
}
