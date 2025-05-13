using ModelLayer.Entity;
using RepositoryLayer.DTO;

namespace RepositoryLayer.Interface
{
    public interface IUserRL
    {
        Task<ResponseDTO<UserEntity>> RegisterAsync(RegUserDTO request);
        Task<ResponseDTO<LoginResponseDTO>> LoginAsync(LoginRequestDTO request);
        Task<ResponseDTO<string>> ResetPasswordAsync(ResetPasswordDTO request, string email);
        Task<ResponseDTO<string>> ForgotPasswordAsync(string email);
        Task<ResponseDTO<string>> DeleteUserAsync(string email);
        Task<ResponseDTO<List<UserEntity>>> GetAllUsersAsync();
    }
}
