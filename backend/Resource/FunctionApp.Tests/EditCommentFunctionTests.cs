using System;
using System.Collections.Generic;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

namespace FunctionApp.Tests
{
    public class EditCommentFunctionTests
    {
        private static ILogger logger = new Mock<ILogger>().Object;

        private static int user_id = 107;
        private static string good_comment_id = "737";
        private static string bad_comment_id = "694";

        private HttpRequest getRequest()
        {
            return TestUtils.CreateHttpRequest(
                requestBody: new Dictionary<string, object> {
                    { "user_id", user_id },
                    { "content_body", "updated by EditCommentFunctionTests.cs" }
                },
                user_id: user_id
            );
        }

        private HttpRequest getAdminRequest()
        {
            return TestUtils.CreateHttpRequest(
                requestBody: new Dictionary<string, object> {
                    { "content_body", "updated by EditCommentFunctionTests.cs" }
                },
                user_id: TestUtils.ADMIN_USER_ID
            );
        }

        [Fact]
        public async System.Threading.Tasks.Task TestEditComment()
        {
            var response = (OkObjectResult)await EditCommentFunction.Run(getRequest(), good_comment_id, logger);
            object body = response.Value;
            Assert.Equal(200, response.StatusCode);
            Assert.Equal(true, TestUtils.GetField(body, "success"));
            Assert.Equal(737, TestUtils.GetField(body, "comment_id"));
        }

        [Fact]
        public async System.Threading.Tasks.Task TestAdminEditComment()
        {
            var response = (OkObjectResult)await EditCommentFunction.Run(getAdminRequest(), good_comment_id, logger);
            object body = response.Value;
            Assert.Equal(200, response.StatusCode);
            Assert.Equal(true, TestUtils.GetField(body, "success"));
            Assert.Equal(737, TestUtils.GetField(body, "comment_id"));
        }

        [Fact]
        public async System.Threading.Tasks.Task TestEditCommentFail()
        {
            // ID does not exist.
            var response1 = (BadRequestResult)await EditCommentFunction.Run(getRequest(), "0", logger);
            Assert.Equal(400, response1.StatusCode);

            // Malformed ID.
            var response2 = (BadRequestResult)await EditCommentFunction.Run(getRequest(), "safw", logger);
            Assert.Equal(400, response2.StatusCode);

            // Provided ID does not match author ID in database.
            var response3 = (UnauthorizedResult)await EditCommentFunction.Run(getRequest(), bad_comment_id, logger);
            Assert.Equal(401, response3.StatusCode);
        }
    }
}
