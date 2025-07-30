using Domain.DTOs.Users;

namespace Application.Interfaces.Services
{
    public interface IUserService
    {
        Task RegisterAsync(UserRegisterDto dto);
        Task<string> LoginAsync(UserLoginDto dto);

        Task<UserDto> GetByIdAsync(int id);
        Task<IEnumerable<UserDto>> GetAllAsync();

        Task UpdateAsync(int id, UserUpdateDto dto);
        Task DeleteAsync(int id);
    }
}
