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
    public class CartControllerTest
    {
        private Mock<ICartBL> _mockCartBL;
        private Mock<ILogger<CartController>> _mockLogger;
        private CartController _cartController;

        [SetUp]
        public void Setup()
        {
            _mockCartBL = new Mock<ICartBL>();
            _mockLogger = new Mock<ILogger<CartController>>();
            _cartController = new CartController(_mockCartBL.Object, _mockLogger.Object);
        }

        [Test]
        public async Task AddToCart_ShouldReturnOk_WhenAddToCartIsSuccessfull()
        {
            var request = new AddCartRequestDTO
            {
                BookId = 1,
                Quantity = 2
            };
            var expectedResponse = new ResponseDTO<CartEntity>
            {
                IsSuccess = true
            };
            var userId = 1;
            _mockCartBL.Setup(x => x.AddInCartAsync(request, userId)).ReturnsAsync(expectedResponse);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
    new Claim("Id", userId.ToString())
            }, "mock"));
            _cartController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            var result = await _cartController.AddToCart(request);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ResponseDTO<CartEntity>;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess);
        }
    }
}
