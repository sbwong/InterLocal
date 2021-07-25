using System;
using System.Collections.Generic;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

namespace FunctionApp.Tests
{
    public class CreateCommentFunctionTests
    {
        private static ILogger logger = new Mock<ILogger>().Object;

        private static int post_id = 284;
        private static int user_id = 107;

        [Fact]
        public async System.Threading.Tasks.Task TestCreateComment()
        {
            HttpRequest req = TestUtils.CreateHttpRequest(
                requestBody: new Dictionary<string, object> {
                    { "post_id", post_id },
                    { "user_id", user_id },
                    { "content_body", "created by CreateCommentFunctionTests.cs" }
                },
                user_id: user_id
            );
            var result = (OkObjectResult)await CreateCommentFunction.Run(req, logger);
            Assert.Equal(200, result.StatusCode);

            object body = result.Value;
            Assert.Equal(true, TestUtils.GetField(body, "success"));

            var comment = TestUtils.GetField(body, "comment");
            Assert.IsType<int>(TestUtils.GetField(comment, "comment_id"));
            Assert.Equal(post_id, TestUtils.GetField(comment, "post_id"));
            Assert.Equal(user_id, TestUtils.GetField(comment, "author_id"));
            Assert.Equal("created by CreateCommentFunctionTests.cs", TestUtils.GetField(comment, "content_body"));
            Assert.IsType<DateTime>(TestUtils.GetField(comment, "created_time"));
            Assert.IsType<int>(TestUtils.GetField(comment, "up_count"));
            Assert.IsType<int>(TestUtils.GetField(comment, "down_count"));
        }

        [Fact]
        public async System.Threading.Tasks.Task TestCreateCommentFailed()
        {
            // post_id is null
            HttpRequest req = TestUtils.CreateHttpRequest(
                requestBody: new Dictionary<string, object> {
                    { "user_id", user_id },
                    { "content_body", "created by CreateCommentFunctionTests.cs" }
                },
                user_id: user_id
            );
            var result = (BadRequestResult)await CreateCommentFunction.Run(req, logger);
            Assert.Equal(400, result.StatusCode);

            // user_id is null
            req = TestUtils.CreateHttpRequest(
                requestBody: new Dictionary<string, object> {
                    { "post_id", post_id },
                    { "content_body", "created by CreateCommentFunctionTests.cs" }
                }
            );
            var unauth = (UnauthorizedResult)await CreateCommentFunction.Run(req, logger);
            Assert.Equal(401, unauth.StatusCode);

            // content_body is null
            req = TestUtils.CreateHttpRequest(
                requestBody: new Dictionary<string, object> {
                    { "post_id", post_id },
                    { "user_id", user_id },
                },
                user_id: user_id
            );
            result = (BadRequestResult)await CreateCommentFunction.Run(req, logger);
            Assert.Equal(400, result.StatusCode);
        }
    }
}
