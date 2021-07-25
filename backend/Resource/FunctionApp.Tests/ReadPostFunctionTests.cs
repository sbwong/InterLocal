using System;
using System.Collections.Generic;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Mvc;

namespace FunctionApp.Tests
{
    public class ReadPostFunctionTests
    {
        private static ILogger logger = new Mock<ILogger>().Object;

        // user id of 1 from google docs of test tokens 

        private string jwt_token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoxLCJ1c2VybmFtZSI6InRlc3RfdXNlcl8xIiwicm9sZSI6InVzZXIiLCJleHAiOjU1MTYyMzkwMjJ9.1-7UElPkeKw5-wRjqqPGT_M4tIy5RD9UEbn5ecAEH0c";


        private HttpRequest getRequest()
        {
            return TestUtils.CreateHttpRequest(
                user_id: 107

            );
        }

        [Fact]
        public async System.Threading.Tasks.Task TestReadPost()
        {
            //var response = (OkObjectResult)await ReadPostFunction.Run(TestUtils.CreateHttpRequest(), "62", logger);

            var response = (OkObjectResult)await ReadPostFunction.Run(getRequest(), "285", logger);
            object body = response.Value;
            Assert.Equal(200, response.StatusCode);
            Assert.Equal(true, TestUtils.GetField(body, "success"));
            Assert.Equal(285, TestUtils.GetField(body, "res.post_id"));
            Assert.Equal("lyds", TestUtils.GetField(body, "res.username"));
            Assert.Equal(false, TestUtils.GetField(body, "res.is_admin"));
            Assert.Equal(false, TestUtils.GetField(body, "res.is_upvote"));
        }

        [Fact]
        public async System.Threading.Tasks.Task TestReadPostFail()
        {
            // Malformed ID.
            var response2 = (BadRequestResult)await ReadPostFunction.Run(getRequest(), "awsd", logger);
            Assert.Equal(400, response2.StatusCode);
        }
    }
}
