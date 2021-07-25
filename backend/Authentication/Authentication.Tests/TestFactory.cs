using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

// Based on https://docs.microsoft.com/en-us/azure/azure-functions/functions-test-a-function
// Create HTTP requests containing mock data to use in tests (AuthenticationTests.cs).
namespace Authentication.Tests
{
    public class TestFactory
    {
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
        //private static Dictionary<string, StringValues> CreateGetDictionary(string key, string value)
        //{
        //    var qs = new Dictionary<string, StringValues>
        //    {
        //        { key, value }
        //    };
        //    return qs;
        //}

        // This method creates an HTTP GET request initialized with the given query string parameters.
        //public static HttpRequest CreateHttpGetRequest(string queryStringKey, string queryStringValue)
        //{
        //    var context = new DefaultHttpContext();
        //    var request = context.Request;
        //    request.Query = new QueryCollection(CreateGetDictionary(queryStringKey, queryStringValue));
        //    return request;
        //}

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
    }
}