using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;
using Authentication;
using Moq;
using System;
using System.Threading.Tasks;

// Contains tests against the Authenticate HTTP trigger.
// Based on https://docs.microsoft.com/en-us/azure/azure-functions/functions-test-a-function
namespace Authentication.Tests
{
    public class RegisterFunctionTests
    {
        public RegisterFunctionTests()
        {
            var mock = new Mock<IDbAdapter>();

            // The user "Bill" exists in the database.
            mock.Setup(_ => _.GetIDByUsername("Bill")).Returns(1);
            // The user "Bill" exists in the database.
            mock.Setup(_ => _.GetIDByUsername("Joe")).Returns(2);
            // The user "Molly" does not exist.
            mock.Setup(_ => _.GetIDByUsername("Molly")).Returns(-1);

            // The user "Bill" does not have an entry in credentials yet.
            mock.Setup(_ => _.DoesIdExist(1)).Returns(false);
            // The user "Joe" already have an entry in credentials.
            mock.Setup(_ => _.DoesIdExist(2)).Returns(true);

            string password = PasswordEncryption.Encrypt("testpassword");
            mock.Setup(_ => _.CreateCredentialsEntry(1, password));
            RegisterFunction.db = mock.Object;
        }

        private readonly ILogger logger = TestFactory.CreateLogger();

        [Fact]
        public async void Http_trigger_should_return_success()
        {
            Dictionary<string, string> requestBody = new Dictionary<string, string>();
            requestBody["username"] = "Bill";
            requestBody["password"] = "testpassword";

            var request = TestFactory.CreateHttpPostRequestWithDictionary(requestBody);
            var response = (OkObjectResult)await RegisterFunction.Run(request, logger);
            Assert.Equal(200, response.StatusCode);
        }

        [Fact]
        public async void Http_trigger_should_return_409_status()
        {
            Dictionary<string, string> requestBody = new Dictionary<string, string>();
            requestBody["username"] = "Joe";
            requestBody["password"] = "testpassword";

            var request = TestFactory.CreateHttpPostRequestWithDictionary(requestBody);
            var response = (ConflictObjectResult)await RegisterFunction.Run(request, logger);
            Assert.Equal(409, response.StatusCode);
        }
    }
}
