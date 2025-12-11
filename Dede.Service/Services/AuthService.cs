// Dede.Service/Services/AuthService.cs

using System.Security.Claims;
using System.Security.Cryptography;
using Dede.Domain.Entities;
using Dede.Domain.Interfaces;
using Dede.Service.Dto;
using Dede.Service.Security;

namespace Dede.Service.Services;

public interface IAuthService
{
    Task<(bool ok, string? error, User? user)> RegisterAsync(RegisterDto dto);
    Task<(bool ok, string? error, User? user)> LoginAsync(LoginDto dto);
    
    Task<(bool ok, string? error, User? user)> LoginWithGoogleAsync(ClaimsPrincipal externalUser);
    Task<(bool ok, string? error, User? user)> ConfirmEmailAsync(int userId, string token);    
}

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;

    public AuthService(IUserRepository users)
    {
        _users = users;
    }
    
    public async Task<(bool ok, string? error, User? user)> LoginWithGoogleAsync(ClaimsPrincipal externalUser)
    {
        var email = externalUser.FindFirst(ClaimTypes.Email)?.Value;

        if (String.IsNullOrEmpty(email))
            return (false, "Не удалось получить email от Google", null);
        
        var name = externalUser.FindFirst(ClaimTypes.Name)?.Value ?? email;

        var user = await _users.GetByEmailAsync(email);

        if (user == null)
        {
            // создаём нового юзера
            user = new User
            {
                Name = name,
                Email = email,
                Phone = "",
                PasswordHash = PasswordHasher.Hash(Guid.NewGuid().ToString("N")), // просто рандом
                Role = "User",
                CreatedAt = DateTime.UtcNow,

                EmailConfirmed = true,
                EmailConfirmationToken = null,
                EmailConfirmationTokenExpiresAt = null
            };

            await _users.AddAsync(user);
            await _users.SaveChangesAsync();
        }
        else if (!user.EmailConfirmed)
        {
            // если был старый, но не подтверждён — считаем подтверждённым
            user.EmailConfirmed = true;
            user.EmailConfirmationToken = null;
            user.EmailConfirmationTokenExpiresAt = null;
            await _users.SaveChangesAsync();
        }

        return (true, null, user);
    }


    public async Task<(bool ok, string? error, User? user)> RegisterAsync(RegisterDto dto)
    {
        if (await _users.EmailExistsAsync(dto.Email))
            return (false, "Пользователь с таким email уже существует", null);

        var token = GenerateEmailToken();

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone,
            PasswordHash = PasswordHasher.Hash(dto.Password),
            Role = "User",
            CreatedAt = DateTime.UtcNow,

            EmailConfirmed = false,
            EmailConfirmationToken = token,
            EmailConfirmationTokenExpiresAt = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(2), DateTimeKind.Utc)

        };

        await _users.AddAsync(user);
        await _users.SaveChangesAsync();

        return (true, null, user);
    }

    public async Task<(bool ok, string? error, User? user)> LoginAsync(LoginDto dto)
    {
        var user = await _users.GetByEmailAsync(dto.Email);
        if (user is null)
            return (false, "Неверный email или пароль", null);

        if (!PasswordHasher.Check(user.PasswordHash, dto.Password))
            return (false, "Неверный email или пароль", null);
        
        if (!user.EmailConfirmed)
            return (false, "Email не подтверждён. Проверьте почту и перейдите по ссылке в письме.", null);
        


        return (true, null, user);
    }
    
    private static string GenerateEmailToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }
    
    public async Task<(bool ok, string? error, User? user)> ConfirmEmailAsync(int userId, string token)
    {
        var user = await _users.GetByIdAsync(userId);
        if (user == null)
            return (false, "Пользователь не найден", null);

        if (user.EmailConfirmed)
            return (true, null, user);

        if (string.IsNullOrEmpty(user.EmailConfirmationToken) ||
            !string.Equals(user.EmailConfirmationToken, token, StringComparison.Ordinal))
        {
            return (false, "Некорректная ссылка подтверждения", null);
        }

        if (user.EmailConfirmationTokenExpiresAt.HasValue &&
            user.EmailConfirmationTokenExpiresAt.Value < DateTime.UtcNow)
        {
            return (false, "Ссылка подтверждения устарела", null);
        }

        user.EmailConfirmed = true;
        user.EmailConfirmationToken = null;
        user.EmailConfirmationTokenExpiresAt = null;

        await _users.SaveChangesAsync();

        return (true, null, user);
    }


}