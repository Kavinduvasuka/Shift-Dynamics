using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftDynamics.API.Common;
using ShiftDynamics.API.Interfaces;

namespace ShiftDynamics.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notifications;
    public NotificationsController(INotificationService notifications) => _notifications = notifications;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> List([FromQuery] bool unreadOnly = false) =>
        Ok(ApiResponse<object>.Ok(await _notifications.GetForUserAsync(User.RequireUserId(), unreadOnly)));

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id) =>
        await _notifications.MarkReadAsync(id, User.RequireUserId()) ? NoContent() : NotFound();
}
