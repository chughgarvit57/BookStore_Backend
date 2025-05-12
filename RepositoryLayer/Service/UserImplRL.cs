using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelLayer.Entity;
using RepoLayer.DTO;
using RepoLayer.Helper;
using RepositoryLayer.Context;
using RepositoryLayer.DTO;
using RepositoryLayer.Helper;
using RepositoryLayer.Interface;
using StackExchange.Redis;

namespace RepositoryLayer.Service
{
    public class UserImplRL : IUserRL
    {
        private readonly UserContext _context;
        private readonly IDatabase _redisDb;
        private readonly IConnectionMultiplexer _redisConnection;
        private readonly PasswordHashService _passwordHashService;
        private readonly AuthService _service;
        private readonly EmailService _emailService;
        private readonly ILogger<UserImplRL> _logger;

        public UserImplRL(UserContext context, IConnectionMultiplexer redis,
            PasswordHashService passwordHashService, AuthService service,
            EmailService emailService, ILogger<UserImplRL> logger)
        {
            _context = context;
            _redisConnection = redis;
            _redisDb = redis.GetDatabase();
            _passwordHashService = passwordHashService;
            _service = service;
            _emailService = emailService;
            _logger = logger;
            _logger.LogDebug("UserImplRL initialized for UserContext, Redis, and services.");
        }

        private string GetUserCacheKey(string email) => $"user:{email}";
        private string GetAllUsersCacheKey() => "all_users";

        public async Task<ResponseDTO<UserEntity>> RegisterAsync(RegUserDTO request)
        {
            _logger.LogInformation("Attempting to register user with email: {Email}", request.Email);
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogDebug("Checking if user exists with email: {Email}", request.Email);
                if (await _context.Users.AnyAsync(x => x.Email == request.Email))
                {
                    _logger.LogWarning("User already exists with email: {Email}", request.Email);
                    return new ResponseDTO<UserEntity>
                    {
                        IsSuccess = false,
                        Message = "User already exists with this email!",
                    };
                }

                var newUser = new UserEntity
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Password = _passwordHashService.HashPassword(request.Password)
                };

                _logger.LogDebug("Adding new user to database with email: {Email}", request.Email);
                await _context.Users.AddAsync(newUser);
                await _context.SaveChangesAsync();

                _logger.LogDebug("Caching new user with email: {Email}", newUser.Email);
                var cacheKey = GetUserCacheKey(newUser.Email);
                await _redisDb.StringSetAsync(cacheKey, JsonSerializer.Serialize(newUser), TimeSpan.FromMinutes(30));
                await _redisDb.KeyDeleteAsync(GetAllUsersCacheKey());

                _logger.LogDebug("Sending welcome email to: {Email}", request.Email);
                var message = new EmailMessageDTO
                {
                    To = request.Email,
                    Subject = "🎉 Welcome to Our Platform!",
                    Body = $"Hey {request.FirstName} {request.LastName}! 👋<br/><br/>" +
                           "We're super excited to have you on board! 🚀<br/>" +
                           "Get ready to explore awesome features and make your journey with us amazing. 💫<br/><br/>" +
                           "Cheers,<br/>The BookStore Team ❤️"
                };
                try
                {
                    _emailService.SendEmail(message.To, message.Subject, message.Body);
                    _logger.LogInformation("Welcome email sent successfully to: {Email}", request.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send welcome email to: {Email}", request.Email);
                }

                await transaction.CommitAsync();
                _logger.LogInformation("User successfully registered with email: {Email}", request.Email);

                return new ResponseDTO<UserEntity>
                {
                    IsSuccess = true,
                    Message = "User registered successfully!",
                    Data = newUser
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during registration for email: {Email}", request.Email);
                await transaction.RollbackAsync();
                return new ResponseDTO<UserEntity>
                {
                    IsSuccess = false,
                    Message = ex.Message,
                };
            }
        }

        public async Task<ResponseDTO<LoginResponseDTO>> LoginAsync(string email, string password)
        {
            _logger.LogInformation("Login attempt for email: {Email}", email);
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var cacheKey = GetUserCacheKey(email);
                _logger.LogDebug("Checking cache for user with email: {Email}", email);
                var cachedUser = await _redisDb.StringGetAsync(cacheKey);

                if (cachedUser.HasValue)
                {
                    try
                    {
                        var user = JsonSerializer.Deserialize<UserEntity>(cachedUser);
                        if (_passwordHashService.VerifyPassword(password, user.Password))
                        {
                            var token = _service.GenerateToken(user);
                            _logger.LogInformation("User authenticated from cache for email: {Email}", email);

                            return new ResponseDTO<LoginResponseDTO>
                            {
                                IsSuccess = true,
                                Message = "Login successful!",
                                Data = new LoginResponseDTO
                                {
                                    Email = user.Email,
                                    Token = token
                                }
                            };
                        }
                        _logger.LogWarning("Invalid password attempt for cached user with email: {Email}", email);
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.LogWarning(jsonEx, "Error deserializing cached user for email: {Email}. Clearing cache.", email);
                        await _redisDb.KeyDeleteAsync(cacheKey);
                    }
                }

                _logger.LogDebug("No cache found, querying database for user with email: {Email}", email);
                var dbUser = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
                if (dbUser == null)
                {
                    _logger.LogWarning("Login failed: user not found for email: {Email}", email);
                    return new ResponseDTO<LoginResponseDTO>
                    {
                        IsSuccess = false,
                        Message = "User not found! Please register first.",
                    };
                }

                if (!_passwordHashService.VerifyPassword(password, dbUser.Password))
                {
                    _logger.LogWarning("Invalid password attempt for email: {Email}", email);
                    return new ResponseDTO<LoginResponseDTO>
                    {
                        IsSuccess = false,
                        Message = "Invalid password!",
                    };
                }

                _logger.LogDebug("Caching user with email: {Email} after successful login", email);
                await _redisDb.StringSetAsync(cacheKey, JsonSerializer.Serialize(dbUser), TimeSpan.FromMinutes(30));

                var loginResponse = new LoginResponseDTO
                {
                    Email = dbUser.Email,
                    Token = _service.GenerateToken(dbUser),
                };

                await transaction.CommitAsync();
                _logger.LogInformation("User successfully logged in with email: {Email}", email);

                return new ResponseDTO<LoginResponseDTO>
                {
                    IsSuccess = true,
                    Message = "Login successful!",
                    Data = loginResponse
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login for email: {Email}", email);
                await transaction.RollbackAsync();
                return new ResponseDTO<LoginResponseDTO>
                {
                    IsSuccess = false,
                    Message = ex.Message,
                };
            }
        }

        public async Task<ResponseDTO<string>> ResetPasswordAsync(ResetPasswordDTO request, string email)
        {
            _logger.LogInformation("Password reset requested for email: {Email}", email);
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var cacheKey = GetUserCacheKey(email);
                _logger.LogDebug("Checking cache for user with email: {Email}", email);
                var cachedUser = await _redisDb.StringGetAsync(cacheKey);
                UserEntity user = null;

                if (cachedUser.HasValue)
                {
                    try
                    {
                        user = JsonSerializer.Deserialize<UserEntity>(cachedUser);
                        _logger.LogDebug("User found in cache for email: {Email}", email);
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.LogWarning(jsonEx, "Error deserializing cached user for email: {Email}. Clearing cache.", email);
                        await _redisDb.KeyDeleteAsync(cacheKey);
                    }
                }

                if (user == null)
                {
                    _logger.LogDebug("No cache found, querying database for user with email: {Email}", email);
                    user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
                }

                if (user == null)
                {
                    _logger.LogWarning("Password reset failed: user not found for email: {Email}", email);
                    return new ResponseDTO<string>
                    {
                        IsSuccess = false,
                        Message = "User not registered! Please register first."
                    };
                }

                if (!_passwordHashService.VerifyPassword(request.OldPasssword, user.Password))
                {
                    _logger.LogWarning("Incorrect old password entered for email: {Email}", email);
                    return new ResponseDTO<string>
                    {
                        IsSuccess = false,
                        Message = "Old password is incorrect!"
                    };
                }

                if (request.NewPassword != request.ConfirmPassword)
                {
                    _logger.LogWarning("New password and confirm password mismatch for email: {Email}", email);
                    return new ResponseDTO<string>
                    {
                        IsSuccess = false,
                        Message = "New password and confirm password do not match!"
                    };
                }

                user.Password = _passwordHashService.HashPassword(request.NewPassword);
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                _logger.LogDebug("Updated password for user with email: {Email}", email);

                _logger.LogDebug("Caching updated user with email: {Email}", email);
                await _redisDb.StringSetAsync(cacheKey, JsonSerializer.Serialize(user), TimeSpan.FromMinutes(30));
                await _redisDb.KeyDeleteAsync(GetAllUsersCacheKey());

                _logger.LogDebug("Sending password reset confirmation email to: {Email}", email);
                var message = new EmailMessageDTO
                {
                    To = email,
                    Subject = "Password Reset Confirmation",
                    Body = $"Hello {user.FirstName} {user.LastName},<br/><br/>" +
                           "Your password has been successfully reset!<br/>" +
                           "If you didn't request this change, please contact our support team immediately.<br/><br/>" +
                           "Best regards,<br/>The BookStore Team ❤️"
                };
                try
                {
                    _emailService.SendEmail(message.To, message.Subject, message.Body);
                    _logger.LogInformation("Password reset confirmation email sent successfully to: {Email}", email);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send password reset confirmation email to: {Email}", email);
                }

                await transaction.CommitAsync();
                _logger.LogInformation("Password reset successfully for email: {Email}", email);

                return new ResponseDTO<string>
                {
                    IsSuccess = true,
                    Message = "Password reset successfully!",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during password reset for email: {Email}", email);
                await transaction.RollbackAsync();
                return new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message,
                };
            }
        }

        public async Task<ResponseDTO<string>> ForgotPasswordAsync(string email)
        {
            _logger.LogInformation("Forgot password requested for email: {Email}", email);
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogDebug("Checking database for user with email: {Email}", email);
                var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);

                if (user == null)
                {
                    _logger.LogWarning("Forgot password failed: user not found for email: {Email}", email);
                    return new ResponseDTO<string>
                    {
                        IsSuccess = false,
                        Message = "User not registered! Please register first."
                    };
                }

                var token = _service.GenerateToken(user);
                _logger.LogDebug("Sending forgot password email to: {Email}", email);
                var message = new EmailMessageDTO
                {
                    To = email,
                    Subject = "Password Reset Request",
                    Body = $"Hello {user.FirstName} {user.LastName},<br/><br/>" +
                           "We received a request to reset your password. If you didn't make this request, please ignore this email.<br/>" +
                           $"<a href='https://example.com/reset-password/{email}&resetToken={token}'>Reset Password</a><br/><br/>" +
                           "Best regards,<br/>The BookStore Team ❤️"
                };
                try
                {
                    _emailService.SendEmail(message.To, message.Subject, message.Body);
                    _logger.LogInformation("Forgot password email sent successfully to: {Email}", email);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send forgot password email to: {Email}", email);
                }

                await transaction.CommitAsync();
                _logger.LogInformation("Forgot password process completed for email: {Email}", email);

                return new ResponseDTO<string>
                {
                    IsSuccess = true,
                    Message = "Password reset link sent to your email!",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during forgot password for email: {Email}", email);
                await transaction.RollbackAsync();
                return new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message,
                };
            }
        }

        public async Task<ResponseDTO<string>> DeleteUserAsync(string email)
        {
            _logger.LogInformation("Attempting to delete user with email: {Email}", email);
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var cacheKey = GetUserCacheKey(email);
                _logger.LogDebug("Checking database for user with email: {Email}", email);
                var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);

                if (user == null)
                {
                    _logger.LogWarning("Delete user failed: user not found for email: {Email}", email);
                    return new ResponseDTO<string>
                    {
                        IsSuccess = false,
                        Message = "User not registered! Please register first."
                    };
                }

                _logger.LogDebug("Removing user with email: {Email} from database", email);
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                _logger.LogDebug("Clearing user cache for email: {Email}", email);
                await _redisDb.KeyDeleteAsync(cacheKey);
                var allUsersCacheKey = GetAllUsersCacheKey();
                var cachedUsers = await _redisDb.StringGetAsync(allUsersCacheKey);

                if (cachedUsers.HasValue)
                {
                    try
                    {
                        _logger.LogDebug("Updating all users cache after deletion for email: {Email}", email);
                        var usersList = JsonSerializer.Deserialize<List<UserEntity>>(cachedUsers);
                        usersList.RemoveAll(u => u.Email == email);
                        await _redisDb.StringSetAsync(allUsersCacheKey, JsonSerializer.Serialize(usersList), TimeSpan.FromMinutes(30));
                        _logger.LogDebug("All users cache updated, {Count} users remaining", usersList.Count);
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.LogWarning(jsonEx, "Error deserializing all users cache during deletion for email: {Email}. Clearing cache.", email);
                        await _redisDb.KeyDeleteAsync(allUsersCacheKey);
                    }
                }

                _logger.LogDebug("Sending account deletion confirmation email to: {Email}", email);
                var message = new EmailMessageDTO
                {
                    To = email,
                    Subject = "Account Deletion Confirmation",
                    Body = $"Hello {user.FirstName} {user.LastName},<br/><br/>" +
                           "Your account has been successfully deleted.<br/>" +
                           "If you didn't request this, please contact our support team immediately.<br/><br/>" +
                           "Best regards,<br/>The BookStore Team ❤️"
                };
                try
                {
                    _emailService.SendEmail(message.To, message.Subject, message.Body);
                    _logger.LogInformation("Account deletion confirmation email sent successfully to: {Email}", email);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send account deletion confirmation email to: {Email}", email);
                }

                await transaction.CommitAsync();
                _logger.LogInformation("User account deleted successfully for email: {Email}", email);

                return new ResponseDTO<string>
                {
                    IsSuccess = true,
                    Message = "User account deleted successfully!",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during account deletion for email: {Email}", email);
                await transaction.RollbackAsync();
                return new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message,
                };
            }
        }

        public async Task<ResponseDTO<List<UserEntity>>> GetAllUsersAsync()
        {
            _logger.LogInformation("Attempting to fetch all users");
            try
            {
                var cacheKey = GetAllUsersCacheKey();
                _logger.LogDebug("Checking cache for all users with key: {CacheKey}", cacheKey);
                var cachedUsers = await _redisDb.StringGetAsync(cacheKey);

                if (cachedUsers.HasValue)
                {
                    try
                    {
                        var users = JsonSerializer.Deserialize<List<UserEntity>>(cachedUsers);
                        _logger.LogInformation("Retrieved {Count} users from cache", users?.Count ?? 0);
                        return new ResponseDTO<List<UserEntity>>
                        {
                            IsSuccess = true,
                            Message = "Users retrieved from cache!",
                            Data = users
                        };
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.LogWarning(jsonEx, "Error deserializing all users cache. Clearing cache.");
                        await _redisDb.KeyDeleteAsync(cacheKey);
                    }
                }

                _logger.LogDebug("No cache found, querying database for all users");
                var usersFromDb = await _context.Users.ToListAsync();
                if (usersFromDb == null || usersFromDb.Count == 0)
                {
                    _logger.LogWarning("No users found in the database");
                    return new ResponseDTO<List<UserEntity>>
                    {
                        IsSuccess = false,
                        Message = "No users found!",
                    };
                }

                _logger.LogDebug("Caching {Count} users", usersFromDb.Count);
                await _redisDb.StringSetAsync(cacheKey, JsonSerializer.Serialize(usersFromDb), TimeSpan.FromMinutes(30));

                _logger.LogInformation("Retrieved {Count} users from database", usersFromDb.Count);
                return new ResponseDTO<List<UserEntity>>
                {
                    IsSuccess = true,
                    Message = "Users retrieved from database!",
                    Data = usersFromDb
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all users");
                return new ResponseDTO<List<UserEntity>>
                {
                    IsSuccess = false,
                    Message = ex.Message,
                };
            }
        }
    }
}