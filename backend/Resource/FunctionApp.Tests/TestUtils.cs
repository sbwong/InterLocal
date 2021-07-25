/**
 * Large part taken from "backend/Authentication/Authentication.Tests/TestFactory.cs".
 */

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;

namespace FunctionApp.Tests
{
    public class TestUtils
    {
        public static int ADMIN_USER_ID = 81;
        private static Dictionary<int, string> jwts = new Dictionary<int, string>()
        {
            { 0, "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjowLCJ1c2VybmFtZSI6InRlc3RfdXNlciIsInJvbGUiOiJ1c2VyIiwiZXhwIjo1NTE2MjM5MDIyfQ.QbJDQW2DXSJHzznBIdO0pTE1nAmrDLlKwvGOFRwIVZQ"},
            { 107, "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoxMDcsInVzZXJuYW1lIjoidGVzdF91c2VyIiwicm9sZSI6InVzZXIiLCJleHAiOjU1MTYyMzkwMjJ9.EB963db5kMxz5hP4LtaxaiYieMK7lbTBHJJtumNDEyc" },
            { 33, "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjozMywidXNlcm5hbWUiOiJ0ZXN0X3VzZXIiLCJyb2xlIjoidXNlciIsImV4cCI6NTUxNjIzOTAyMn0.jAsOjDkCqIyyXrfmAgmiVRJNb13r3flVvCHvmcvBuzA"},
            
            // admin user
            { ADMIN_USER_ID, "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjo4MSwidXNlcm5hbWUiOiJPSVNTX0FkbWluIiwicm9sZSI6ImFkbWluIiwiZXhwIjo1NTE2MjM5MDIyfQ.-3dTbQGTO_G9cyWJH-awnn0KeABKuE9RpuQFHjSdPHc"}
        };

        /**
         * Creates an HTTP request with the provided query parameters and request body.
         * If no query parameters or request body are needed, leave the arguments as `null`.
         * 
         * There is no need to specify the request method, as the triggers function do not care.
         */
        public static HttpRequest CreateHttpRequest(
            Dictionary<string, StringValues> queryParams = null, Dictionary<string, object> requestBody = null, int user_id = -1, string requestHeader = null)
        {
            // Create request object.
            var context = new DefaultHttpContext();
            var request = context.Request;
            if (queryParams != null)
            {
                request.Query = new QueryCollection(queryParams);
            }
            if (user_id != -1)
            {
                request.Headers.Add("token", jwts[user_id]);
            }


            // Serialize the request body as a JSON.
            // https://mahmutcanga.com/2019/12/13/unit-testing-httprequest-in-c/
            if (requestBody != null)
            {
                var asString = JsonConvert.SerializeObject(requestBody);
                byte[] byteArray = Encoding.ASCII.GetBytes(asString);
                MemoryStream stream = new MemoryStream(byteArray);
                request.Body = stream;
            }

            if (requestHeader != null)
            {
                request.Headers.Add("token", requestHeader);
            }

            return request;
        }

        /**
         * Gets the value of a field from an anonymous object.
         */
        public static object GetField(object o, string field)
        {
            return GetPropertyValue(o, field);
        }

        public static object GetPropertyValue(object src, string propName)
        {
            if (src == null) throw new ArgumentException("Value cannot be null.", "src");
            if (propName == null) throw new ArgumentException("Value cannot be null.", "propName");

            if(propName.Contains("."))//complex type nested
            {
                var temp = propName.Split(new char[] { '.' }, 2);
                return GetPropertyValue(GetPropertyValue(src, temp[0]), temp[1]);
            }
            else
            {
                var prop = src.GetType().GetProperty(propName);
                return prop != null ? prop.GetValue(src, null) : null;
            }
        }
    }
}