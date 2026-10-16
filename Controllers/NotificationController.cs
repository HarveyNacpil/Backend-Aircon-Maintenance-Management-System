using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.AuthorizationModule;
using Api.NotificationModule;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Produces("application/json")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationRepository _repo;
        public NotificationController(INotificationRepository repo) => _repo = repo;

        [HttpGet("user/{userId:int}")]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> GetByUser(int userId)
        {
            try   { return Ok(await _repo.GetByUserIdAsync(userId)); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("user/{userId:int}/unread")]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> GetUnreadByUser(int userId)
        {
            try   { return Ok(await _repo.GetUnreadByUserIdAsync(userId)); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("user/{userId:int}/unread-count")]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> GetUnreadCount(int userId)
        {
            try   { return Ok(new { count = await _repo.GetUnreadCountAsync(userId) }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("{id:int}")]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var n = await _repo.GetByIdAsync(id);
                return n is null ? NotFound(new { message = "Notification not found." }) : Ok(n);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPost]
        [Authorize(Policy = AppPolicies.AdminOrTech)]
        public async Task<IActionResult> Create([FromBody] Notification body)
        {
            try
            {
                if (body.UserId <= 0)
                    return BadRequest(new { message = "user_id is required." });
                await _repo.AddAsync(body);
                return Ok(new { message = "Notification created." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPatch("{id:int}/read")]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var existing = await _repo.GetByIdAsync(id);
                if (existing is null) return NotFound(new { message = "Notification not found." });
                await _repo.MarkAsReadAsync(id);
                return Ok(new { message = "Notification marked as read." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPatch("user/{userId:int}/read-all")]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> MarkAllAsRead(int userId)
        {
            try
            {
                await _repo.MarkAllAsReadAsync(userId);
                return Ok(new { message = "All notifications marked as read." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existing = await _repo.GetByIdAsync(id);
                if (existing is null) return NotFound(new { message = "Notification not found." });
                await _repo.DeleteAsync(id);
                return Ok(new { message = "Notification deleted." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }
    }
}
