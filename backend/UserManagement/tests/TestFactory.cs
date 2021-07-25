using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UserManagement;

// Based on https://docs.microsoft.com/en-us/azure/azure-functions/functions-test-a-function
// Create HTTP requests containing mock data to use in tests (AuthenticationTests.cs).
namespace UserManagementTest
{
    public class TestFactory
    {
        public static string ADMIN_JWT = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjowLCJ1c2VybmFtZSI6InRlc3RfdXNlciIsInJvbGUiOiJhZG1pbiIsImV4cCI6NTUxNjIzOTAyMn0.haPrituANaCERWkjo9kkSliqjbT2LFWXgZy_rBkjrbc";
        // user_id = 0
        public static string USER_JWT = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjowLCJ1c2VybmFtZSI6InRlc3RfdXNlciIsInJvbGUiOiJ1c2VyIiwiZXhwIjo1NTE2MjM5MDIyfQ.QbJDQW2DXSJHzznBIdO0pTE1nAmrDLlKwvGOFRwIVZQ";
        // user_id = 1
        public static string ALT_USER_JWT = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoxLCJ1c2VybmFtZSI6InRlc3RfdXNlciIsInJvbGUiOiJ1c2VyIiwiZXhwIjo1NTE2MjM5MDIyfQ.adGIViOXrbc97tmo_j8ohdLQFPlnkKDOoJT3VwsiOJ4";
        public static string MALFORMED_JWT = "eyJhbGciOiJIUzI1NiIsInr3cCI6IkpXVCJ9.eyJ1c2VYX2lkIjowLCJ1c2VybmFtZSI6InRlc3RfdXNlciIsInJvbGUiOiJhZG1pbiI123V4cCI6NTUxNjIzOTAyMn0.haPrituANaCERWK6o9kkSliqjbT2LFWXgZy_rBkjrbc";
        public static string EXPIRED_JWT = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjowLCJ1c2VybmFtZSI6InRlc3RfdXNlciIsInJvbGUiOiJhZG1pbiIsImV4cCI6NTE2MjM5MDIyfQ.0lK6puFi0BDJ5qIAybdi1J0X9b8Vwm5jBGPlnV6Ybfs";
        public static string WRONG_SECRET_JWT = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjowLCJ1c2VybmFtZSI6InRlc3RfdXNlciIsInJvbGUiOiJhZG1pbiIsImV4cCI6NTUxNjIzOTAyMn0.R961YC-JkGIxGdWnKBiw-OwtTZ2Bn7PbkilOahmLpLE";

        // Data: This property returns an IEnumerable collection of sample data.
        // The key value pairs represent values that are passed into a query string.
        //public static IEnumerable<object[]> Data()
        //{
        //    return new List<object[]>
        //    {
        //        new object[] { "username", "Bill" },
        //        new object[] { "username", "Paul" },
        //        new object[] { "username", "Steve" }

        //    };
        //}

        // This method accepts a key/value pair as arguments and returns a new Dictionary used to
        // create QueryCollection to represent query string values.
        private static Dictionary<string, StringValues> CreateGetDictionary(string key, string value)
        {
            var qs = new Dictionary<string, StringValues>
            {
                { key, value }
            };
            return qs;
        }

        // This method creates an HTTP GET request initialized with the given query string parameters.
        public static HttpRequest CreateHttpGetRequest(string queryStringKey, string queryStringValue)
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Query = new QueryCollection(CreateGetDictionary(queryStringKey, queryStringValue));
            return request;
        }

        // This method accepts a key/value pair as arguments and returns a new Dictionary used to
        // represent the request body.
        private static Dictionary<string, string> CreatePostDictionary(string key, string value)
        {
            var qs = new Dictionary<string, string>
            {
                { key, value }
            };
            return qs;
        }

        // This method creates an HTTP POST request initialized with the given request body data.
        public static HttpRequest CreateHttpPostRequest(string bodyStringKey, string bodyStringValue)
        {
            var requestBody = CreatePostDictionary(bodyStringKey, bodyStringValue);
            return CreateHttpPostRequestWithDictionary(requestBody);
        }

        // This method creates an HTTP POST request using a dictionary as the request body data.
        public static HttpRequest CreateHttpPostRequestWithDictionary(Dictionary<string, string> requestBody)
        {
            // Create request object.
            var context = new DefaultHttpContext();
            var request = context.Request;

            // Serialize the request body as a JSON.
            // https://mahmutcanga.com/2019/12/13/unit-testing-httprequest-in-c/
            var asString = JsonConvert.SerializeObject(requestBody);
            byte[] byteArray = Encoding.ASCII.GetBytes(asString);
            MemoryStream stream = new MemoryStream(byteArray);
            request.Body = stream;
            return request;
        }

        // This method creates an HTTP POST request using a string as the request body data.
        public static HttpRequest CreateHttpPostRequestWithString(string requestBody)
        {
            // Create request object.
            var context = new DefaultHttpContext();
            var request = context.Request;

            byte[] byteArray = Encoding.ASCII.GetBytes(requestBody);
            MemoryStream stream = new MemoryStream(byteArray);
            request.Body = stream;
            return request;
        }

        // Based on the logger type, this method returns a logger class used for testing.
        // The ListLogger keeps track of logged messages available for evaluation in tests.
        public static ILogger CreateLogger(LoggerTypes type = LoggerTypes.Null)
        {
            ILogger logger;

            if (type == LoggerTypes.List)
            {
                logger = new ListLogger();
            }
            else
            {
                logger = NullLoggerFactory.Instance.CreateLogger("Null Logger");
            }

            return logger;
        }

        public static ILoggingAdapter CreateLoggingAdapter()
        {
            var mockLogger = new Mock<ILoggingAdapter>();
            mockLogger.Setup(_ => _.LogDebug(It.IsAny<string>(), It.IsAny<string>()));
            mockLogger.Setup(_ => _.LogMetric(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));
            mockLogger.Setup(_ => _.LogSuccessMetric(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));
            mockLogger.Setup(_ => _.LogFailureMetric(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));
            return mockLogger.Object;
        }
    }
}