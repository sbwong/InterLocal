using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;
using Authentication;
using Moq;
using System;

// Contains tests against the Authenticate HTTP trigger.
// Based on https://docs.microsoft.com/en-us/azure/azure-functions/functions-test-a-function
namespace Authentication.Tests
{
    public class LoginFunctionTests
    {
        private int validUserId = 1;
        private string validUsername = "Bill";
        private string password = "alphabeta";
        private string invalidUsername = "Molly";
        private string userStatus = "user";

        public LoginFunctionTests()
        {
            var mock = new Mock<IDbAdapter>();

            // The user "Bill" exists in the database.
            mock.Setup(_ => _.GetIDByUsername(validUsername)).Returns(validUserId);

            // The user "Molly" does not exist.
            mock.Setup(_ => _.GetIDByUsername(invalidUsername)).Returns(-1);

            // The user "Bill" has password "alphabeta" in the database.
            var encryptedPassword = PasswordEncryption.Encrypt(password);
            mock.Setup(_ => _.LookupPasswordByUserId(validUserId)).Returns(encryptedPassword);

            // The user "Bill" has typical user permissions i.e. is not an admin
            mock.Setup(_ => _.GetStatusByID(validUserId)).Returns(userStatus);

            // Mock out LoginFunction's database.
            LoginFunction.db = mock.Object;
        }

        private readonly ILogger logger = TestFactory.CreateLogger();

        /// This test creates a request using the valid credentials username=Bill password=alphabeta
        /// to an HTTP function and checks that the expected response is returned.
        [Fact]
        public async void Valid_login_should_return_user_status()
        {

            Dictionary<string, string> requestBody = new Dictionary<string, string> {
                ["username"] = validUsername,
                ["password"] = password,
            };
            var request = TestFactory.CreateHttpPostRequestWithDictionary(requestBody);
            var response = (OkObjectResult)await LoginFunction.Run(request, logger);
            Assert.Equal(200, response.StatusCode);

            // https://stackoverflow.com/a/18191592
            string status = (string)response.Value.GetType().GetProperty("status").GetValue(response.Value);
            Assert.Equal(userStatus, status);
        }

        /// Tests that attempting to login as a existing user with an incorrect password fails as expected.
        [Fact]
        public async void Incorrect_password_should_return_unauthorized_status()
        {
            Dictionary<string, string> requestBody = new Dictionary<string, string>
            {
                ["username"] = validUsername,
                ["password"] = password + "123",
            };
            var request = TestFactory.CreateHttpPostRequestWithDictionary(requestBody);
            var response = (UnauthorizedResult)await LoginFunction.Run(request, logger);
            Assert.Equal(401, response.StatusCode);
        }

        // This test creates requests that are missing a field and verifies that the expected
        // response (401, Uauthorized) is returned.
        [Fact]
        public async void Missing_request_body_fields_should_return_unauthorized()
        {
            var usernameRequest = TestFactory.CreateHttpPostRequest("username", validUsername);
            var passwordRequest = TestFactory.CreateHttpPostRequest("password", password);

            var response = (BadRequestObjectResult)await LoginFunction.Run(usernameRequest, logger);
            Assert.Equal(400, response.StatusCode);
            response = (BadRequestObjectResult)await LoginFunction.Run(passwordRequest, logger);
            Assert.Equal(400, response.StatusCode);
        }


        // This test creates requests with correct data but malformed names and verifies that the expected
        // response (401, Uauthorized) is returned.
        [Fact]
        public async void Http_trigger_should_return_unauthorized_status_wrong_key()
        {
            var request = TestFactory.CreateHttpPostRequestWithDictionary(new Dictionary<string, string>
            {
                ["user"] = validUsername,
                ["password"] = password,
            });

            var response = (BadRequestObjectResult)await LoginFunction.Run(request, logger);
            Assert.Equal(400, response.StatusCode);
        }

        // This test creates a request with the request body with username=Molly to an HTTP
        // function and checks that the expected response is returned (user "Molly" doesn't exist).
        [Fact]
        public async void Http_trigger_should_return_unauthorized_status_incorrect_username()
        {
            var request = TestFactory.CreateHttpPostRequestWithDictionary(new Dictionary<string, string>
            {
                ["username"] = invalidUsername,
                ["password"] = password,
            });
            var response = (UnauthorizedResult)await LoginFunction.Run(request, logger);
            Assert.Equal(401, response.StatusCode);
        }
    }
}
