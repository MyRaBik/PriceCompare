using Application.Interfaces.Services;
using Domain.DTOs.Subscriptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(SubscriptionDto), 201)]
    [ProducesResponseType(typeof(string), 400)]  // Запрос не найден
    [ProducesResponseType(typeof(string), 409)]  // Повторная подписка
    public async Task<IActionResult> Subscribe(CreateSubscriptionDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var subscription = await _subscriptionService.AddAsync(dto, userId);
            return CreatedAtAction(nameof(GetAll), null, subscription);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }


    [HttpGet]
    public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetAll()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var subscriptions = await _subscriptionService.GetAllByUserAsync(userId);
        return Ok(subscriptions);
    }

    [HttpGet("all")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<IEnumerable<AdminSubscriptionGroupDto>>> GetAllSubscriptions()
    {
        var result = await _subscriptionService.GetAllGroupedAsync();
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Unsubscribe(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _subscriptionService.DeleteAsync(id, userId);
        return NoContent();
    }

    [HttpPost("refresh")]
    [Authorize(Roles = "admin")] // чтобы только админ мог обновлять
    [ProducesResponseType(200)]
    public async Task<IActionResult> RefreshSubscriptions([FromServices] ISubscriptionRefresherService refresher)
    {
        await refresher.RefreshAllAsync();
        return Ok("Подписки успешно обновлены");
    }
}
