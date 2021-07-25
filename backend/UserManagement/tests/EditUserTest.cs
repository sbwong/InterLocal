using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using UserManagement;
using Xunit;
using Microsoft.Extensions.Logging;
using UserManagementTest;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace tests
{
    // To run these tests only, run `dotnet test --filter Put_User_Prefs`
    public class EditUserTest
    {
        private readonly ILogger logger = TestFactory.CreateLogger();

        public EditUserTest()
        {
            var mock = new Mock<IDbAdapter>();
            mock.Setup(_ => _.EditUserAsync(It.IsAny<int>(), new { })).Returns(new Task<int>(()=>1));
            EditUserFunction.setDBAdapter(mock.Object);
            EditUserFunction.logger = TestFactory.CreateLoggingAdapter();
        }

        [Fact]
        public async void Put_User_No_JWT()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            IStatusCodeActionResult response = (IStatusCodeActionResult) await EditUserFunction.Run(request, 0, logger);
            Assert.Equal(401, response.StatusCode);
        }

        [Fact]
        public async void Put_User_JWT_Only()
        {
            var request = TestFactory.CreateHttpPostRequest("phone", "1121234567");
            request.Headers.Add(Constants.TOKEN_KEY, TestFactory.USER_JWT);

            IStatusCodeActionResult response = (IStatusCodeActionResult)await EditUserFunction.Run(request, null, logger);
            Assert.Equal(200, response.StatusCode);
        }

        [Fact]
        public async void Put_User_Empty_Body()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers.Add(Constants.TOKEN_KEY, TestFactory.USER_JWT);

            IStatusCodeActionResult response = (IStatusCodeActionResult)await EditUserFunction.Run(request, 0, logger);
            Assert.Equal(400, response.StatusCode);
        }

        [Fact]
        public async void Put_User_Valid_JWT_Wrong_User()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers.Add(Constants.TOKEN_KEY, TestFactory.ALT_USER_JWT);

            IStatusCodeActionResult response = (IStatusCodeActionResult)await EditUserFunction.Run(request, 0, logger);
            Assert.Equal(403, response.StatusCode);
        }

        [Fact]
        public async void Put_User_Valid_JWT_User()
        {
            var request = TestFactory.CreateHttpPostRequest("phone", "1121234567");
            request.Headers.Add(Constants.TOKEN_KEY, TestFactory.USER_JWT);

            IStatusCodeActionResult response = (IStatusCodeActionResult) await EditUserPrefs.Run(request, 0, logger);
            Assert.Equal(200, response.StatusCode);
        }

        [Fact]
        public async void Put_User_Valid_JWT_Admin()
        {
            var request = TestFactory.CreateHttpPostRequest("phone", "1121234567");
            request.Headers.Add(Constants.TOKEN_KEY, TestFactory.USER_JWT);
            IStatusCodeActionResult response = (IStatusCodeActionResult) await EditUserPrefs.Run(request, 0, logger);
            Assert.Equal(200, response.StatusCode);
        }

        [Fact]
        public async void Put_User_Bad_Body()
        {
            var request = TestFactory.CreateHttpPostRequestWithString("heyyy");
            request.Headers.Add(Constants.TOKEN_KEY, TestFactory.ADMIN_JWT);
            IStatusCodeActionResult response = (IStatusCodeActionResult)await EditUserPrefs.Run(request, 0, logger);
            Assert.Equal(400, response.StatusCode);
        }
    }
}
