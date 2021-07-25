using System;
using System.Collections.Generic;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

namespace FunctionApp.Tests
{
    public class CancelPostVoteFunctionTests
    {
        private static ILogger logger = new Mock<ILogger>().Object;

        private static int user_id = 0;

        [Fact]
        public async System.Threading.Tasks.Task TestCancelPostVote()
        {
            // Will need to call upvotePostFunction beforehand after it is pushed
            /*HttpRequest req = TestUtils.CreateHttpRequest(
                requestBody: new Dictionary<string, object> {
                    { "post_id", 19 },
                    { "user_id", 0 },
                    { "is_upvote", false }
                }
            );
            var result = (OkObjectResult)await CancelPostVoteFunction.Run(req, logger);
            Assert.Equal(200, result.StatusCode);

            object body = result.Value;
            Assert.Equal(true, TestUtils.GetField(body, "success"));*/
        }

        [Fact]
        public async System.Threading.Tasks.Task TestCancelNonExistingPostVote()
        {
            HttpRequest req = TestUtils.CreateHttpRequest(
                requestBody: new Dictionary<string, object> {
                    { "post_id", 0 },
                    { "user_id", 0 },
                    { "is_upvote", true }
                },
                user_id: user_id
            );
            var result = (OkObjectResult)await CancelPostVoteFunction.Run(req, logger);
            Assert.Equal(200, result.StatusCode);

            object body = result.Value;
            Assert.Equal(false, TestUtils.GetField(body, "success"));
        }

        [Fact]
        public async System.Threading.Tasks.Task TestCancelPostVoteFailed()
        {
            // post_id is null
            HttpRequest req = TestUtils.CreateHttpRequest(
                requestBody: new Dictionary<string, object> {
                    { "user_id", 0 },
                    { "is_upvote", true }
                },
                user_id: user_id
            );
            var result = (BadRequestResult)await CancelPostVoteFunction.Run(req, logger);
            Assert.Equal(400, result.StatusCode);

            // user_id is null
            req = TestUtils.CreateHttpRequest(
                requestBody: new Dictionary<string, object> {
                    { "post_id", 19 },
                    { "is_upvote", true }
                }
            );
            var unauth = (UnauthorizedResult)await CancelPostVoteFunction.Run(req, logger);
            Assert.Equal(401, unauth.StatusCode);

            // upvote is null
            req = TestUtils.CreateHttpRequest(
                requestBody: new Dictionary<string, object> {
                    { "post_id", 19 },
                    { "user_id", 0 }
                },
                user_id: user_id
            );
            result = (BadRequestResult)await CancelPostVoteFunction.Run(req, logger);
            Assert.Equal(400, result.StatusCode);
        }
    }
}
