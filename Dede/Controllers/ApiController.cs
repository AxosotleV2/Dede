// Controllers/ApiController.cs

using System.Net;
using System.Security.Claims;
using Dede.Domain.Entities;
using Dede.Domain.Interfaces;
using Dede.Domain.Options;
using Dede.Models;
using Dede.Service.Dto;
using Dede.Service.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace Dede.Controllers;

[Route("api")]
[ApiController]
public class ApiController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IOrderService _orderService;
    private readonly IServiceCatalogService _serviceCatalogService;
    private readonly IEmailSender _emailSender;

    public ApiController(
        IAuthService authService,
        IServiceCatalogService serviceCatalogService,
        IOrderService orderService,
        IEmailSender emailSender)
    {
        _authService = authService;
        _serviceCatalogService = serviceCatalogService;
        _orderService = orderService;
        _emailSender = emailSender;
    }

    // ============= AUTH =============

    [HttpPost("auth/register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var (ok, error, user) = await _authService.RegisterAsync(dto);
        if (!ok || user == null)
            return BadRequest(new { success = false, message = error });

        var token = user.EmailConfirmationToken;
        if (!string.IsNullOrEmpty(token))
        {
            var scheme = Request.Scheme;               // http / https
            var host = Request.Host.Value;             // localhost:7114
            var callbackUrl =
                $"{scheme}://{host}/api/auth/confirm-email?userId={user.Id}&token={WebUtility.UrlEncode(token)}";

            var body = $@"
            <p>Здравствуйте, {WebUtility.HtmlEncode(user.Name)}!</p>
            <p>Спасибо за регистрацию в сервисе <b>ДомМастер</b>.</p>
            <p>Чтобы подтвердить email, перейдите по ссылке:</p>
            <p><a href=""{callbackUrl}"">Подтвердить email</a></p>
            <p>Если вы не регистрировались на сайте, просто проигнорируйте это письмо.</p>";

            await _emailSender.SendEmailAsync(
                user.Email,
                "Подтверждение email — ДомМастер",
                body);
        }

        return Ok(new
        {
            success = true,
            message = "Регистрация успешна! Проверьте почту и подтвердите email.",
            data = new { id = user.Id, name = user.Name, email = user.Email }
        });
    }

    [HttpPost("auth/login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var (ok, error, user) = await _authService.LoginAsync(dto); 
        if (!ok || user == null) 
            return Unauthorized(new { success = false, message = error }); 
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()), 
            new(ClaimTypes.Name, user.Name), 
            new(ClaimTypes.Email, user.Email), 
            new(ClaimTypes.Role, user.Role)
        }; 
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme); 
        var principal = new ClaimsPrincipal(identity); 
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal); 
        return Ok(new
        {
            success = true, 
            message = "Вход выполнен успешно!", 
            data = new
            {
                id = user.Id, 
                name = user.Name, 
                email = user.Email, 
                role = user.Role
            }
        });
    }

    [HttpPost("auth/logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new { success = true, message = "Вы вышли из системы" });
    }
    

    [HttpGet("auth/confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] int userId, [FromQuery] string token)
    {
        var (ok, error, user) = await _authService.ConfirmEmailAsync(userId, token);

        // Куда вернуть после подтверждения – твоя главная страница
        var baseUrl = Url.Action("SiteInformation", "Home") ?? "/";

        if (!ok || user == null)
        {
            var errorText = string.IsNullOrWhiteSpace(error)
                ? "Не удалось подтвердить почту"
                : error;

            var errorUrl = QueryHelpers.AddQueryString(baseUrl, "emailConfirmError", errorText);
            return Redirect(errorUrl);
        }

        // ---- АВТО-ЛОГИН ----
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        // ---- РЕДИРЕКТ С ФЛАГОМ ДЛЯ ТОСТА ----
        var okUrl = QueryHelpers.AddQueryString(baseUrl, "emailConfirmed", "1");
        return Redirect(okUrl);
    }



    
    [HttpGet("auth/check")]
    public IActionResult CheckAuth()
    {
        if (User.Identity?.IsAuthenticated == true)
            return Ok(new
            {
                success = true,
                authenticated = true,
                data = new
                {
                    id = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    name = User.Identity!.Name,
                    email = User.FindFirstValue(ClaimTypes.Email),
                    role = User.FindFirstValue(ClaimTypes.Role)
                }
            });

        return Ok(new { success = true, authenticated = false });
    }

    // ============= SERVICES =============

    [HttpGet("services")]
    public async Task<IActionResult> GetServices([FromQuery] string? sort = null)
    {
        var services = await _serviceCatalogService.GetServicesAsync(sort);

        if (!User.IsInRole("Admin"))
        {
            services = services
                .Where(s => s.IsActive)
                .ToList();
        }

        var data = services.Select(s => new
        {
            id = s.Id,
            name = s.Name,
            description = s.Description,
            minPrice = s.MinPrice,
            category = s.Category,
            icon = s.Icon,
            isActive = s.IsActive
        });

        return Ok(new { success = true, data });
    }


    [HttpGet("services/{id:int}")]
    public async Task<IActionResult> GetService(int id)
    {
        var s = await _serviceCatalogService.GetByIdAsync(id);
        if (s == null)
            return NotFound(new { success = false, message = "Услуга не найдена" });

        var data = new
        {
            id = s.Id,
            name = s.Name,
            description = s.Description,
            minPrice = s.MinPrice,
            category = s.Category,
            icon = s.Icon,
            isActive = s.IsActive
        };

        return Ok(new { success = true, data });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("services")]
    public async Task<IActionResult> CreateService([FromBody] ServiceEditModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, message = "Некорректные данные" });

        var entity = new ServiceItem
        {
            Name = model.Name,
            Description = model.Description,
            MinPrice = model.MinPrice,
            Category = model.Category,
            Icon = model.Icon,
            IsActive = model.IsActive
        };

        await _serviceCatalogService.CreateAsync(entity);

        return Ok(new
        {
            success = true,
            message = "Услуга создана",
            data = new
            {
                id = entity.Id,
                name = entity.Name,
                description = entity.Description,
                minPrice = entity.MinPrice,
                category = entity.Category,
                icon = entity.Icon,
                isActive = entity.IsActive
            }
        });
    }
    
    [HttpPost("contact")]
    public async Task<IActionResult> SendContact([FromBody] ContactDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) ||
            string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Subject) ||
            string.IsNullOrWhiteSpace(dto.Message))
        {
            return BadRequest(new { success = false, message = "Заполните все поля" });
        }
        

        // --- автоответ пользователю ---
        var userBody = $"""
                        Здравствуйте, {dto.Name}!

                        Спасибо за ваш отзыв и обращение в сервис «ДомМастер».
                        Мы получили ваше сообщение и ответим вам в ближайшее время.

                        С уважением,
                        команда ДомМастер
                        """;

        await _emailSender.SendEmailAsync(
            dto.Email,
            "ДомМастер: спасибо за ваш отзыв",
            userBody);

        return Ok(new { success = true, message = $"Спасибо за ваш отзыв, {dto.Name}!" });
    }

    [HttpGet("auth/google-login")]
    public IActionResult GoogleLogin()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(GoogleResponse))
        };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("auth/google-response")]
    public async Task<IActionResult> GoogleResponse()
    {
        // Аутентифицируемся по схеме Google
        var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

        if (!authenticateResult.Succeeded)
        {
            return Redirect("/?googleLoginError=auth_failed");
        }

        var (ok, error, user) = await _authService.LoginWithGoogleAsync(authenticateResult.Principal!);
    
        if (!ok || user == null)
        {
            var err = Uri.EscapeDataString(error ?? "Ошибка входа");
            return Redirect($"/?googleLoginError={err}");
        }

        // Создаём свою cookie-сессию
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        
        return Redirect("/?googleLogin=1");
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("services/{id:int}")]
    public async Task<IActionResult> UpdateService(int id, [FromBody] ServiceEditModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, message = "Некорректные данные" });

        var entity = new ServiceItem
        {
            Id = id,
            Name = model.Name,
            Description = model.Description,
            MinPrice = model.MinPrice,
            Category = model.Category,
            Icon = model.Icon,
            IsActive = model.IsActive
        };

        var updated = await _serviceCatalogService.UpdateAsync(entity);
        if (updated == null)
            return NotFound(new { success = false, message = "Услуга не найдена" });

        return Ok(new { success = true, message = "Услуга обновлена" });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("services/{id:int}")]
    public async Task<IActionResult> DeleteService(int id)
    {
        var ok = await _serviceCatalogService.DeleteAsync(id);
        if (!ok)
            return NotFound(new { success = false, message = "Услуга не найдена" });

        return Ok(new { success = true, message = "Услуга удалена" });
    }

    // ============= ORDERS (только авторизованный) =============
    
    [HttpPost("orders")]
    public async Task<IActionResult> CreateOrder([FromBody] OrderCreateDto dto)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return Unauthorized(new { success = false, message = "Требуется вход в систему" });
    
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, message = "Некорректные данные" });
    
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    
        try
        {
            // Сервис не знает про DTO — только про «голые» данные
            var order = await _orderService.CreateOrderAsync(
                userId,
                dto.ServiceItemId,
                dto.Quantity,
                dto.Phone,
                dto.Address,
                dto.Note
            );
    
            return Ok(new
            {
                success = true,
                message = "Заказ создан",
                data = new { orderId = order.Id }
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new { success = false, message = "Ошибка при создании заказа" });
        }
    }
    
    [HttpGet("orders/my")]
    public async Task<IActionResult> GetMyOrders()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return Unauthorized(new { success = false, message = "Требуется вход в систему" });
    
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    
        try
        {
            var orders = await _orderService.GetUserOrdersAsync(userId);
    
            // ВАЖНО: проекция -> нет циклических ссылок, фронту удобнее
            var data = orders.Select(o => new
            {
                id = o.Id,
                status = (int)o.Status,
                address = o.Address,
                phone = o.Phone,
                note = o.Note,
                createdAt = o.CreatedAt,
                items = o.Items.Select(i => new
                {
                    id = i.Id,
                    quantity = i.Quantity,
                    serviceItem = i.ServiceItem == null
                        ? null
                        : new
                        {
                            id = i.ServiceItem.Id,
                            name = i.ServiceItem.Name
                        }
                }).ToList()
            });
    
            return Ok(new { success = true, data });
        }
        catch (Exception)
        {
            return StatusCode(500, new { success = false, message = "Ошибка при получении заказов" });
        }
    }
    
    [HttpPost("orders/{id:int}/cancel")]
    public async Task<IActionResult> CancelOrder(int id)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return Unauthorized(new { success = false, message = "Требуется вход в систему" });
    
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    
        try
        {
            await _orderService.CancelOrderAsync(userId, id);
            return Ok(new { success = true, message = "Заказ отменён" });
        }
        catch (UnauthorizedAccessException)
        {
            // Пытается отменить чужой заказ
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            // Например: "Заказ не найден"
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { success = false, message = "Ошибка при отмене заказа" });
        }
    }

}