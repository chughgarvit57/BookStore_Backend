using BusinessLayer.Interface;
using ModelLayer.Entity;
using RepositoryLayer.DTO;
using RepositoryLayer.Interface;
using Microsoft.Extensions.Logging;

namespace BusinessLayer.Service
{
    public class UserImplBL : IUserBL
    {
        private readonly IUserRL _userRL;
        private readonly ILogger<UserImplBL> _logger;

        public UserImplBL(IUserRL userRL, ILogger<UserImplBL> logger)
        {
            _userRL = userRL;
            _logger = logger;
        }

        public async Task<ResponseDTO<UserEntity>> RegisterAsync(RegUserDTO request)
        {
            try
            {
                _logger.LogInformation("Registering user with email: {Email}", request.Email);
                return await _userRL.RegisterAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed for email: {Email}", request.Email);
                return new ResponseDTO<UserEntity>
                {
                    IsSuccess = false,
                    Message = ex.Message,
                };
            }
        }

        public async Task<ResponseDTO<LoginResponseDTO>> LoginAsync(string email, string password)
        {
            try
            {
                _logger.LogInformation("User login attempt for email: {Email}", email);
                return await _userRL.LoginAsync(email, password);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for email: {Email}", email);
                return new ResponseDTO<LoginResponseDTO>
                {
                    IsSuccess = false,
                    Message = ex.Message,
                };
            }
        }

        public async Task<ResponseDTO<string>> ResetPasswordAsync(ResetPasswordDTO request, string email)
        {
            try
            {
                _logger.LogInformation("Password reset attempt for email: {Email}", email);
                return await _userRL.ResetPasswordAsync(request, email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Password reset failed for email: {Email}", email);
                return new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message,
                };
            }
        }

        public async Task<ResponseDTO<string>> ForgotPasswordAsync(string email)
        {
            try
            {
                _logger.LogInformation("Forgot password requested for email: {Email}", email);
                return await _userRL.ForgotPasswordAsync(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Forgot password failed for email: {Email}", email);
                return new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message,
                };
            }
        }

        public async Task<ResponseDTO<string>> DeleteUserAsync(string email)
        {
            try
            {
                _logger.LogInformation("Delete user requested for email: {Email}", email);
                return await _userRL.DeleteUserAsync(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete user failed for email: {Email}", email);
                return new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message,
                };
            }
        }

        public async Task<ResponseDTO<List<UserEntity>>> GetAllUsersAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all users.");
                return await _userRL.GetAllUsersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fetching all users failed.");
                return new ResponseDTO<List<UserEntity>>
                {
                    IsSuccess = false,
                    Message = ex.Message,
                };
            }
        }
    }
}
