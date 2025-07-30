using Domain.DTOs.Users;
using Application.Interfaces;
using Domain.Entities;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;

namespace Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public UserService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task RegisterAsync(UserRegisterDto dto)
    {
        var existing = await _userRepository.GetByEmailAsync(dto.Email);
        if (existing != null)
            throw new Exception("Пользователь с таким Email уже существует");

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            Password = _passwordHasher.Hash(dto.Password),
            Role = "user"
        };

        await _userRepository.AddAsync(user);
    }

    public async Task<string> LoginAsync(UserLoginDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user == null || !_passwordHasher.Verify(dto.Password, user.Password))
            throw new Exception("Неверный Email или пароль");

        return _jwtTokenGenerator.GenerateToken(user);
    }

    public async Task<UserDto> GetByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id)
            ?? throw new Exception("Пользователь не найден");

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role
        };
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(user => new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role
        });
    }

    public async Task UpdateAsync(int id, UserUpdateDto dto)
    {
        var user = await _userRepository.GetByIdAsync(id)
            ?? throw new Exception("Пользователь не найден");

        if (!string.IsNullOrEmpty(dto.Username))
            user.Username = dto.Username;

        if (!string.IsNullOrEmpty(dto.Password))
            user.Password = _passwordHasher.Hash(dto.Password);

        await _userRepository.UpdateAsync(user);
    }

    public async Task DeleteAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id)
            ?? throw new Exception("Пользователь не найден");

        await _userRepository.DeleteAsync(user);
    }
}