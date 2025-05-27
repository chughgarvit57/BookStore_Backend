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
    public class BookControllerTest
    {
        private Mock<IBookBL> _mockBookBL;
        private Mock<ILogger<BookController>> _mockBookLogger;
        private BookController _bookController;

        [SetUp]
        public void Setup()
        {
            _mockBookBL = new Mock<IBookBL>();
            _mockBookLogger = new Mock<ILogger<BookController>>();
            _bookController = new BookController(_mockBookBL.Object, _mockBookLogger.Object);
        }

        [Test]
        public async Task AddBook_ShouldReturnOk_WhenAddingIsSuccessful()
        {
            var request = new AddBookRequestDTO
            {
                BookName = "Test Book",
                AuthorName = "Test Author",
                Description = "Test Description",
                Price = 450,
                Quantity = 5,
            };
            var userId = 1;
            var expectedResponse = new ResponseDTO<BookEntity>
            {
                IsSuccess = true,
            };
            _mockBookBL.Setup(x => x.AddBookAsync(request, userId)).ReturnsAsync(expectedResponse);
            var book = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("Id", userId.ToString())
            }, "mock"));

            _bookController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = book }
            };
            var result = await _bookController.AddBook(request);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ResponseDTO<BookEntity>;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess);
        }

        [Test]
        public async Task GetBookById_ShouldReturnOk_WhenGetBookByIdIsSuccessful()
        {
            int bookId = 1;
            var expectedResponse = new ResponseDTO<BookEntity>
            {
                IsSuccess = true,
            };
            _mockBookBL.Setup(x => x.GetBookAsync(bookId)).ReturnsAsync(expectedResponse);
            var result = await _bookController.GetBook(bookId);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ResponseDTO<BookEntity>;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess);
        }

        [Test]
        public async Task GetAllBooks_ShouldReturnOk_WhenGetAllBooksIsSuccessful()
        {
            var books = new List<BookEntity>
            {
                new BookEntity { BookId = 1, BookName = "Book A" },
                new BookEntity { BookId = 2, BookName = "Book B" }
            };

            var expectedResponse = new ResponseDTO<List<BookEntity>>
            {
                IsSuccess = true,
            };
            _mockBookBL.Setup(x => x.GetAllBooksAsync()).ReturnsAsync(expectedResponse);
            var result = await _bookController.GetAllBooks();
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ResponseDTO<List<BookEntity>>;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess);
        }

        [Test]
        public async Task AddBook_ShouldReturnBadRequest_WhenRequestIsInvalid()
        {
            var request = new AddBookRequestDTO
            {
                BookName = "", // Invalid: empty book name
                AuthorName = "Test Author",
                Description = "Test Description",
                Price = -100, // Invalid: negative price
                Quantity = 0
            };
            var userId = 1;
            var expectedResponse = new ResponseDTO<BookEntity>
            {
                IsSuccess = false,
                Message = "Invalid book data"
            };
            _mockBookBL.Setup(x => x.AddBookAsync(request, userId)).ReturnsAsync(expectedResponse);
            var book = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("Id", userId.ToString())
            }, "mock"));

            _bookController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = book }
            };
            var result = await _bookController.AddBook(request);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            var response = badRequestResult.Value as ResponseDTO<BookEntity>;
            Assert.IsNotNull(response);
            Assert.IsFalse(response.IsSuccess);
        }

        [Test]
        public async Task AddBook_ShouldReturnUnauthorized_WhenUserIdIsMissing()
        {
            var request = new AddBookRequestDTO
            {
                BookName = "Test Book",
                AuthorName = "Test Author",
                Description = "Test Description",
                Price = 450,
                Quantity = 5
            };
            // No user ID in claims
            var book = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { }, "mock"));

            _bookController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = book }
            };
            var result = await _bookController.AddBook(request);
            var unauthorizedResult = result as UnauthorizedResult;
            Assert.IsNull(unauthorizedResult);
        }

        [Test]
        public async Task GetBookById_ShouldReturnNotFound_WhenBookDoesNotExist()
        {
            int bookId = 99900;
            var expectedResponse = new ResponseDTO<BookEntity>
            {
                IsSuccess = false,
            };
            _mockBookBL.Setup(x => x.GetBookAsync(bookId)).ReturnsAsync(expectedResponse);

            var result = await _bookController.GetBook(bookId);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            var response = notFoundResult.Value as ResponseDTO<BookEntity>;
            Assert.IsNotNull(response);
            Assert.IsFalse(response.IsSuccess);
        }

        [Test]
        public async Task GetAllBooks_ShouldReturnBadRequest_WhenNoBooksExist()
        {
            var expectedResponse = new ResponseDTO<List<BookEntity>>
            {
                IsSuccess = false,
                Message = "No books available"
            };
            _mockBookBL.Setup(x => x.GetAllBooksAsync()).ReturnsAsync(expectedResponse);

            var result = await _bookController.GetAllBooks();
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            var response = badRequestResult.Value as ResponseDTO<List<BookEntity>>;
            Assert.IsNotNull(response);
            Assert.IsFalse(response.IsSuccess);
        }
    }
}