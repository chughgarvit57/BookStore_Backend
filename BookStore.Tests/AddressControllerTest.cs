using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using BackendStore.Controllers;
using BusinessLayer.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModelLayer.Entity;
using Moq;
using RepositoryLayer.DTO;
using NUnit.Framework;

namespace BookStore.Tests
{
    [TestFixture]
    public class AddressControllerTest
    {
        private Mock<IAddressBL> _mockAddressBL;
        private Mock<ILogger<AddressController>> _mockLogger;
        private AddressController _addressController;

        [SetUp]
        public void Setup()
        {
            _mockAddressBL = new Mock<IAddressBL>();
            _mockLogger = new Mock<ILogger<AddressController>>();
            _addressController = new AddressController(_mockAddressBL.Object, _mockLogger.Object);
        }

        [Test]
        public async Task AddAddress_ShouldReturnOk_WhenAddressIsAddedSuccessfully()
        {
            var request = new AddAddressRequestDTO
            {
                Address = "123 Main St",
                AddressType = 0,
                City = "New York",
                Locality = "Manhattan",
                State = "NY",
                PhoneNumber = 1234567890
            };
            var userId = 1;
            var expectedResponse = new ResponseDTO<AddressEntity>
            {
                IsSuccess = true
            };
            _mockAddressBL.Setup(x => x.AddAddressAsync(request, userId)).ReturnsAsync(expectedResponse);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("Id", userId.ToString())
            }, "mock"));
            _addressController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            var result = await _addressController.AddAddress(request);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ResponseDTO<AddressEntity>;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess);
        }

        [Test]
        public async Task AddAddress_ShouldReturnBadRequest_WhenAddressIsNotAdded()
        {
            var request = new AddAddressRequestDTO
            {
                Address = "",
                AddressType = 0,
                City = "",
                Locality = "",
                State = "",
                PhoneNumber = 1234567890
            };
            var userId = 1;
            var expectedResponse = new ResponseDTO<AddressEntity>
            {
                IsSuccess = false
            };
            _mockAddressBL.Setup(x => x.AddAddressAsync(request, userId)).ReturnsAsync(expectedResponse);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("Id", userId.ToString())
            }, "mock"));
            _addressController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            var result = await _addressController.AddAddress(request);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            var response = badRequestResult.Value as ResponseDTO<AddressEntity>;
            Assert.IsNotNull(response);
            Assert.IsFalse(response.IsSuccess);
        }

        [Test]
        public async Task AddAddress_ShouldReturnBadRequest_WhenExceptionIsThrown()
        {
            var request = new AddAddressRequestDTO
            {
                Address = "123 Main St",
                AddressType = 0,
                City = "New York",
                Locality = "Manhattan",
                State = "NY",
                PhoneNumber = 1234567890
            };
            var userId = 1;
            _mockAddressBL.Setup(x => x.AddAddressAsync(request, userId)).ThrowsAsync(new Exception("Database error"));
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("Id", userId.ToString())
            }, "mock"));
            _addressController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            var result = await _addressController.AddAddress(request);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            var response = badRequestResult.Value as ResponseDTO<string>;
            Assert.IsNotNull(response);
            Assert.IsFalse(response.IsSuccess);
        }

        [Test]
        public async Task DeleteAddress_ShouldReturnOk_WhenAddressIsDeletedSuccessfully()
        {
            var addressId = 1;
            var expectedResponse = new ResponseDTO<AddressEntity>
            {
                IsSuccess = true
            };
            var userId = 1;
            _mockAddressBL.Setup(x => x.DeleteAddressAsync(addressId, userId)).ReturnsAsync(expectedResponse);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("Id", userId.ToString())
            }, "mock"));
            _addressController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            var result = await _addressController.DeleteAddressAsync(addressId);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ResponseDTO<AddressEntity>;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess);
        }

        [Test]
        public async Task DeleteAddress_ShouldReturnBadRequest_WhenExceptionIsThrown()
        {
            var addressId = 1;
            var userId = 1;
            var expectedResponse = new ResponseDTO<AddressEntity>
            {
                IsSuccess = false
            };
            _mockAddressBL.Setup(x => x.DeleteAddressAsync(addressId, userId)).ReturnsAsync(expectedResponse);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("Id", userId.ToString())
            }, "mock"));
            _addressController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            var result = await _addressController.DeleteAddressAsync(addressId);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            var response = badRequestResult.Value as ResponseDTO<AddressEntity>;
            Assert.IsNotNull(response);
            Assert.IsFalse(response.IsSuccess);
        }

        [Test]
        public async Task GetAllAddresses_ShouldReturnOk_WhenAddressesAreRetrieved()
        {
            var userId = 1;
            var addresses = new List<AddressEntity>
            {
                new AddressEntity
                {
                    UserId = userId,
                    Address = "Dummy Address",
                    AddressType = 0,
                    City = "Chd",
                    Locality = "Progressive Society",
                    State = "Chd",
                    PhoneNumber = 916283625475
                },
                new AddressEntity
                {
                    UserId = userId,
                    Address = "Dummy Address 2",
                    AddressType = 0,
                    City = "Chd",
                    Locality = "Victoria Enclave",
                    State = "Chd",
                    PhoneNumber = 916283625547
                }
            };
            var expectedResponse = new ResponseDTO<List<AddressEntity>>
            {
                IsSuccess = true,
            };
            _mockAddressBL.Setup(x => x.GetAllAddressesAsync(userId)).ReturnsAsync(expectedResponse);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("Id", userId.ToString())
            }, "mock"));
            _addressController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            var result = await _addressController.GetAllAddresses();
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ResponseDTO<List<AddressEntity>>;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess);
        }

        [Test]
        public async Task GetAllAddresses_ShouldReturnOk_WhenNoAddressesExist()
        {
            var userId = 1;
            var expectedResponse = new ResponseDTO<List<AddressEntity>>
            {
                IsSuccess = true
            };
            _mockAddressBL.Setup(x => x.GetAllAddressesAsync(userId)).ReturnsAsync(expectedResponse);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("Id", userId.ToString())
            }, "mock"));
            _addressController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            var result = await _addressController.GetAllAddresses();
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ResponseDTO<List<AddressEntity>>;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess);
        }

        [Test]
        public async Task GetAllAddresses_ShouldReturnBadRequest_WhenExceptionIsThrown()
        {
            var userId = 1;
            _mockAddressBL.Setup(x => x.GetAllAddressesAsync(userId)).ThrowsAsync(new Exception("Database error"));
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("Id", userId.ToString())
            }, "mock"));
            _addressController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            var result = await _addressController.GetAllAddresses();
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            var response = badRequestResult.Value as ResponseDTO<string>;
            Assert.IsNotNull(response);
            Assert.IsFalse(response.IsSuccess);
        }

        [Test]
        public async Task UpdateAddress_ShouldReturnOk_WhenAddressIsUpdatedSuccessfully()
        {
            var request = new UpdateAddressRequestDTO
            {
                AddressId = 1,
                AddressType = 0,
                City = "Boston",
                State = "MA"
            };
            var userId = 1;
            var expectedResponse = new ResponseDTO<AddressEntity>
            {
                IsSuccess = true
            };
            _mockAddressBL.Setup(x => x.UpdateAddressAsync(request, userId)).ReturnsAsync(expectedResponse);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("Id", userId.ToString())
            }, "mock"));
            _addressController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            var result = await _addressController.UpdateAddress(request);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ResponseDTO<AddressEntity>;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess);
        }

        [Test]
        public async Task UpdateAddress_ShouldReturnBadRequest_WhenAddressIsNotFound()
        {
            var request = new UpdateAddressRequestDTO
            {
                AddressId = 999,
                AddressType = 0,
                City = "Boston",
                State = "MA",
            };
            var userId = 1;
            var expectedResponse = new ResponseDTO<AddressEntity>
            {
                IsSuccess = false,
            };
            _mockAddressBL.Setup(x => x.UpdateAddressAsync(request, userId)).ReturnsAsync(expectedResponse);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("Id", userId.ToString())
            }, "mock"));
            _addressController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            var result = await _addressController.UpdateAddress(request);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            var response = badRequestResult.Value as ResponseDTO<AddressEntity>;
            Assert.IsNotNull(response);
            Assert.IsFalse(response.IsSuccess);
        }

        [Test]
        public async Task UpdateAddress_ShouldReturnBadRequest_WhenExceptionIsThrown()
        {
            var request = new UpdateAddressRequestDTO
            {
                AddressId = 1,
                AddressType = 0,
                City = "Boston",
                State = "MA"
            };
            var userId = 1;
            _mockAddressBL.Setup(x => x.UpdateAddressAsync(request, userId)).ThrowsAsync(new Exception("Database error"));
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("Id", userId.ToString())
            }, "mock"));
            _addressController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            var result = await _addressController.UpdateAddress(request);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            var response = badRequestResult.Value as ResponseDTO<string>;
            Assert.IsNotNull(response);
            Assert.IsFalse(response.IsSuccess);
        }
    }
}