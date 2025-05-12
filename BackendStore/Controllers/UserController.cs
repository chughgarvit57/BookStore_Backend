using BusinessLayer.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModelLayer.Entity;
using RepositoryLayer.DTO;
using System.Text.Json;

namespace BackendStore.Controllers
{
    /// <summary>
    /// Controller for handling all user-related operations including registration,
    /// authentication, password management, and user data retrieval.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserBL _userBL;
        private readonly ILogger<UserController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserController"/> class.
        /// </summary>
        /// <param name="userBL">Business layer service for user operations.</param>
        /// <param name="logger">Logger service for logging operations.</param>
        public UserController(IUserBL userBL, ILogger<UserController> logger)
        {
            _userBL = userBL;
            _logger = logger;
            _logger.LogDebug("UserController initialized with UserBL.");
        }

        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        /// <param name="request">User registration data transfer object containing user details.</param>
        /// <returns>
        /// Returns the newly created user if registration is successful, or an error message if it fails.
        /// </returns>
        /// <response code="200">Returns the newly created user.</response>
        /// <response code="400">If the user already exists or invalid data is provided.</response>
        [HttpPost("Register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register(RegUserDTO request)
        {
            _logger.LogInformation("Attempting to register user with email: {Email}", request.Email);
            try
            {
                _logger.LogDebug("Registering user with details: {Request}", JsonSerializer.Serialize(request));
                var result = await _userBL.RegisterAsync(request);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("User registered successfully with email: {Email}", request.Email);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Failed to register user with email: {Email}. Message: {Message}", request.Email, result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user with email: {Email}", request.Email);
                return BadRequest(new ResponseDTO<RegUserDTO>
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Authenticates a user and generates an access token.
        /// </summary>
        /// <param name="email">User's email address.</param>
        /// <param name="password">User's password.</param>
        /// <returns>
        /// Returns the user details and authentication token if successful, or an error message if authentication fails.
        /// </returns>
        /// <response code="200">Returns the user details and authentication token.</response>
        /// <response code="400">If credentials are invalid or the user doesn't exist.</response>
        [HttpPost("Login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login(string email, string password)
        {
            _logger.LogInformation("Attempting to log in user with email: {Email}", email);
            try
            {
                _logger.LogDebug("Logging in with email: {Email}", email);
                var result = await _userBL.LoginAsync(email, password);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("User logged in successfully with email: {Email}", email);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Failed to log in user with email: {Email}. Message: {Message}", email, result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging in user with email: {Email}", email);
                return BadRequest(new ResponseDTO<UserEntity>
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Allows an authenticated user to reset their password.
        /// </summary>
        /// <param name="request">Password reset data transfer object containing old and new passwords.</param>
        /// <returns>
        /// Returns a success message if the password reset is successful, or an error message if it fails.
        /// </returns>
        /// <response code="200">Returns a success message if the password was reset.</response>
        /// <response code="400">If the old password is incorrect or new passwords don't match.</response>
        [HttpPatch("ResetPassword")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO request)
        {
            _logger.LogInformation("Attempting to reset password for user with email: {Email}", User.Claims.FirstOrDefault(x => x.Type == "Email")?.Value);
            try
            {
                string loggedInEmail = User.Claims.FirstOrDefault(x => x.Type == "Email")?.Value;
                _logger.LogDebug("Resetting password with details: {Request}", JsonSerializer.Serialize(request));
                var result = await _userBL.ResetPasswordAsync(request, loggedInEmail);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Password reset successfully for email: {Email}", loggedInEmail);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Failed to reset password for email: {Email}. Message: {Message}", loggedInEmail, result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for email: {Email}", User.Claims.FirstOrDefault(x => x.Type == "Email")?.Value);
                return BadRequest(new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a user account from the system.
        /// </summary>
        /// <param name="email">Email of the user to be deleted.</param>
        /// <returns>
        /// Returns a success message if deletion is successful, or an error message if it fails.
        /// </returns>
        /// <response code="200">Returns a success message if the user was deleted.</response>
        /// <response code="400">If the user doesn't exist or deletion fails.</response>
        [HttpDelete("DeleteUser")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteUser(string email)
        {
            _logger.LogInformation("Attempting to delete user with email: {Email}", email);
            try
            {
                var result = await _userBL.DeleteUserAsync(email);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("User deleted successfully with email: {Email}", email);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Failed to delete user with email: {Email}. Message: {Message}", email, result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with email: {Email}", email);
                return BadRequest(new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Initiates a password reset process by sending a reset link to the user's email.
        /// </summary>
        /// <param name="email">Email of the user requesting a password reset.</param>
        /// <returns>
        /// Returns a success message if the reset email is sent, or an error message if it fails.
        /// </returns>
        /// <response code="200">Returns a success message if the reset email was sent.</response>
        /// <response code="400">If the email doesn't exist or sending fails.</response>
        [HttpPost("ForgotPassword")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            _logger.LogInformation("Attempting to process forgot password for email: {Email}", email);
            try
            {
                var result = await _userBL.ForgotPasswordAsync(email);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Forgot password processed successfully for email: {Email}", email);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Failed to process forgot password for email: {Email}. Message: {Message}", email, result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing forgot password for email: {Email}", email);
                return BadRequest(new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Retrieves all users from the system (requires authentication).
        /// </summary>
        /// <returns>
        /// Returns a list of all users if successful, or an error message if retrieval fails.
        /// </returns>
        /// <response code="200">Returns the list of all users.</response>
        /// <response code="400">If retrieval fails.</response>
        [HttpGet("GetAllUsers")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllUsers()
        {
            _logger.LogInformation("Attempting to fetch all users");
            try
            {
                var result = await _userBL.GetAllUsersAsync();
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Retrieved {Count} users successfully", result.Data?.Count ?? 0);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Failed to fetch all users. Message: {Message}", result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all users");
                return BadRequest(new ResponseDTO<List<UserEntity>>
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }
    }
}