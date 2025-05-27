using System.Security.Claims;
using BackendStore.Controllers;
using BusinessLayer.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModelLayer.Entity;
using Moq;
using RepositoryLayer.DTO;

namespace BookStore.Tests
{
    [TestFixture]
    public class UserControllerTests
    {
        private Mock<IUserBL> _mockUserBL;
        private Mock<ILogger<UserController>> _mockLogger;
        private UserController _userController;

        [SetUp]
        public void Setup()
        {
            _mockUserBL = new Mock<IUserBL>();
            _mockLogger = new Mock<ILogger<UserController>>();
            _userController = new UserController(_mockUserBL.Object, _mockLogger.Object);
        }

        [Test]
        public async Task Register_ShouldReturnOk_WhenRegistrationIsSuccessfull()
        {
            var request = new RegUserDTO
            {
                FirstName = "Garvit",
                LastName = "Chugh",
                Email = "gavi.chugh@gmail.com",
                Password = "password",
            };

            var expectedResponse = new ResponseDTO<UserEntity>
            {
                IsSuccess = true
            };

            _mockUserBL.Setup(x => x.RegisterAsync(request)).ReturnsAsync(expectedResponse);

            var result = await _userController.Register(request);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsTrue((okResult?.Value as ResponseDTO<UserEntity>)?.IsSuccess);
        }
        [Test]
        public async Task Register_ShouldReturnBadRequest_WhenExceptionIsThrown()
        {
            var request = new RegUserDTO
            {
                FirstName = "Garvit",
                LastName = "Chugh",
                Email = "gavi.chugh@gmail.com",
                Password = "password"
            };
            _mockUserBL.Setup(x => x.RegisterAsync(request)).ThrowsAsync(new Exception());
            var result = await _userController.Register(request);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badResult = result as BadRequestObjectResult;
            var response = badResult?.Value as ResponseDTO<RegUserDTO>;
            Assert.IsFalse(response?.IsSuccess);
        }

        [Test]
        public async Task Register_ShouldReturnBadRequest_WhenRegistrationFails()
        {
            var request = new RegUserDTO
            {
                FirstName = "Test",
                LastName = "Fail",
                Email = "fail@example.com",
                Password = "failpass",
            };

            var failedResponse = new ResponseDTO<UserEntity>
            {
                IsSuccess = false
            };

            _mockUserBL.Setup(x => x.RegisterAsync(request)).ReturnsAsync(failedResponse);

            var result = await _userController.Register(request);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badResult = result as BadRequestObjectResult;
            Assert.IsFalse((badResult?.Value as ResponseDTO<UserEntity>)?.IsSuccess);
        }

        [Test]
        public async Task Login_ShouldReturnOk_WhenLoginIsSuccessful()
        {
            var request = new LoginRequestDTO
            {
                Email = "gavi.chugh@gmail.com",
                Password = "password",
            };

            var expectedResponse = new ResponseDTO<LoginResponseDTO>
            {
                IsSuccess = true
            };

            _mockUserBL.Setup(x => x.LoginAsync(request)).ReturnsAsync(expectedResponse);

            var result = await _userController.Login(request);

            Assert.IsInstanceOf<OkObjectResult>(result);
            Assert.IsTrue(((result as OkObjectResult)?.Value as ResponseDTO<LoginResponseDTO>)?.IsSuccess);
        }

        [Test]
        public async Task Login_ShouldReturnBadRequest_WhenLoginFails()
        {
            var request = new LoginRequestDTO
            {
                Email = "wrong@example.com",
                Password = "wrongpass",
            };

            var failedResponse = new ResponseDTO<LoginResponseDTO>
            {
                IsSuccess = false
            };

            _mockUserBL.Setup(x => x.LoginAsync(request)).ReturnsAsync(failedResponse);

            var result = await _userController.Login(request);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            Assert.IsFalse(((result as BadRequestObjectResult)?.Value as ResponseDTO<LoginResponseDTO>)?.IsSuccess);
        }

        [Test]
        public async Task ResetPassword_ShouldReturnOk_WhenResetIsSuccessful()
        {
            var request = new ResetPasswordDTO
            {
                OldPasssword = "Old@1234",
                NewPassword = "New@1234",
                ConfirmPassword = "New@1234"
            };
            var email = "gavi.chugh@gmail.com";

            var expectedResponse = new ResponseDTO<string>
            {
                IsSuccess = true
            };

            _mockUserBL.Setup(x => x.ResetPasswordAsync(request, email)).ReturnsAsync(expectedResponse);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("Email", email)
            }, "mock"));

            _userController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var result = await _userController.ResetPassword(request);

            Assert.IsInstanceOf<OkObjectResult>(result);
            Assert.IsTrue(((result as OkObjectResult)?.Value as ResponseDTO<string>)?.IsSuccess);
        }

        [Test]
        public async Task ResetPassword_ShouldReturnBadRequest_WhenResetFails()
        {
            var request = new ResetPasswordDTO
            {
                OldPasssword = "wrong",
                NewPassword = "wrong",
                ConfirmPassword = "wrong"
            };
            var email = "gavi.chugh@gmail.com";

            var failedResponse = new ResponseDTO<string>
            {
                IsSuccess = false
            };

            _mockUserBL.Setup(x => x.ResetPasswordAsync(request, email)).ReturnsAsync(failedResponse);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("Email", email)
            }, "mock"));

            _userController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var result = await _userController.ResetPassword(request);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            Assert.IsFalse(((result as BadRequestObjectResult)?.Value as ResponseDTO<string>)?.IsSuccess);
        }

        [Test]
        public async Task DeleteUser_ShouldReturnOk_WhenDeletionIsSuccessful()
        {
            var email = "test@example.com";

            var expectedResponse = new ResponseDTO<string>
            {
                IsSuccess = true
            };

            _mockUserBL.Setup(x => x.DeleteUserAsync(email)).ReturnsAsync(expectedResponse);

            var result = await _userController.DeleteUser(email);

            Assert.IsInstanceOf<OkObjectResult>(result);
            Assert.IsTrue(((result as OkObjectResult)?.Value as ResponseDTO<string>)?.IsSuccess);
        }

        [Test]
        public async Task DeleteUser_ShouldReturnBadRequest_WhenDeletionFails()
        {
            var email = "nonexistent@example.com";

            var failedResponse = new ResponseDTO<string>
            {
                IsSuccess = false
            };

            _mockUserBL.Setup(x => x.DeleteUserAsync(email)).ReturnsAsync(failedResponse);

            var result = await _userController.DeleteUser(email);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            Assert.IsFalse(((result as BadRequestObjectResult)?.Value as ResponseDTO<string>)?.IsSuccess);
        }

        [Test]
        public async Task ForgotPassword_ShouldReturnOk_WhenEmailIsValid()
        {
            var email = "gavi.chugh@gmail.com";

            var expectedResponse = new ResponseDTO<string>
            {
                IsSuccess = true
            };

            _mockUserBL.Setup(x => x.ForgotPasswordAsync(email)).ReturnsAsync(expectedResponse);

            var result = await _userController.ForgotPassword(email);

            Assert.IsInstanceOf<OkObjectResult>(result);
            Assert.IsTrue(((result as OkObjectResult)?.Value as ResponseDTO<string>)?.IsSuccess);
        }

        [Test]
        public async Task ForgotPassword_ShouldReturnBadRequest_WhenEmailIsInvalid()
        {
            var email = "invalid@example.com";

            var failedResponse = new ResponseDTO<string>
            {
                IsSuccess = false
            };

            _mockUserBL.Setup(x => x.ForgotPasswordAsync(email)).ReturnsAsync(failedResponse);

            var result = await _userController.ForgotPassword(email);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            Assert.IsFalse(((result as BadRequestObjectResult)?.Value as ResponseDTO<string>)?.IsSuccess);
        }

        [Test]
        public async Task GetAllUsers_ShouldReturnOk_WhenUsersExist()
        {
            var expectedResponse = new ResponseDTO<List<UserEntity>>
            {
                IsSuccess = true,
                Data = new List<UserEntity> { new UserEntity(), new UserEntity() }
            };

            _mockUserBL.Setup(x => x.GetAllUsersAsync()).ReturnsAsync(expectedResponse);

            var result = await _userController.GetAllUsers();

            Assert.IsInstanceOf<OkObjectResult>(result);
            Assert.IsTrue(((result as OkObjectResult)?.Value as ResponseDTO<List<UserEntity>>)?.IsSuccess);
        }

        [Test]
        public async Task GetAllUsers_ShouldReturnBadRequest_WhenNoUsersFound()
        {
            var failedResponse = new ResponseDTO<List<UserEntity>>
            {
                IsSuccess = false
            };

            _mockUserBL.Setup(x => x.GetAllUsersAsync()).ReturnsAsync(failedResponse);

            var result = await _userController.GetAllUsers();

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            Assert.IsFalse(((result as BadRequestObjectResult)?.Value as ResponseDTO<List<UserEntity>>)?.IsSuccess);
        }
    }
}
