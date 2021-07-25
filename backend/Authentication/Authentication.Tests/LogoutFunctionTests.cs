using Authentication;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;

// Contains tests against the Authenticate HTTP trigger.
// Based on https://docs.microsoft.com/en-us/azure/azure-functions/functions-test-a-function
namespace Authentication.Tests
{
    public class LogoutFunctionTests
    {
        private readonly ILogger logger = TestFactory.CreateLogger();
        private Credentials user = new Credentials
        {
            status = "user",
            username = "Wexler",
            user_id = 1,
        };
        private string userJwt = "";

        public LogoutFunctionTests()
        {
            // Create a valid JWT for the user.
            userJwt = new TokenIssuer().IssueTokenForUser(user);
        }

        // This test simply verifies that the logout function can be executed and returns the 200 status code.
        [Fact]
        public async void Http_trigger_should_execute()
        {
            var request = TestFactory.CreateHttpPostRequestWithDictionary(new Dictionary<string, string>());
            request.Headers[Constants.TOKEN_KEY] = userJwt;
            var response = (OkObjectResult)await LogoutFunction.Run(request, logger);
            Assert.Equal(200, response.StatusCode);
            Assert.Equal("OK", response.Value);
        }

        // This test simply verifies that the logout function returns 401 if no JWT is provided.
        [Fact]
        public async void Http_trigger_should_return_unauthorized()
        {
            var request = TestFactory.CreateHttpPostRequestWithDictionary(new Dictionary<string, string>());
            var response = (UnauthorizedResult)await LogoutFunction.Run(request, logger);
            Assert.Equal(401, response.StatusCode);
        }
    }
}
