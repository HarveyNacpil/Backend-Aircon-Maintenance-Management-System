using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.AuthorizationModule;
using Api.DTOs;
using Api.Security;
using Api.UserModules;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Produces("application/json")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _repo;

        public UserController(IUserRepository repo)
        {
            _repo = repo;
        }

        // ── CRUD ──────────────────────────────────────────────────────────────

        /// <summary>Get all users. Admin only.</summary>
        [HttpGet]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<ActionResult<IEnumerable<object>>> GetAll()
        {
            try
            {
                var users = await _repo.GetAllAsync();
                return Ok(users.Select(u => new {
                    u.UserId, u.FullName, u.Username, u.Role, u.Phone, u.Email, u.Status, u.CreatedAt
                }));
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        /// <summary>Get user by ID. Admin only.</summary>
        [HttpGet("{id:int}")]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var user = await _repo.GetByIdAsync(id);
                if (user is null) return NotFound(new { message = "User not found." });
                return Ok(new {
                    user.UserId, user.FullName, user.Username, user.Role,
                    user.Phone, user.Email, user.Status, user.CreatedAt
                });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        /// <summary>Search users. Admin only.</summary>
        [HttpGet("search")]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            try
            {
                var users = await _repo.SearchAsync(query);
                return Ok(users.Select(u => new {
                    u.UserId, u.FullName, u.Username, u.Role, u.Phone, u.Email, u.Status, u.CreatedAt
                }));
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        /// <summary>Get paginated users. Admin only.</summary>
        [HttpGet("paginated")]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<IActionResult> GetPaginated(int pageNumber = 1, int pageSize = 25)
        {
            try   { return Ok(await _repo.GetPaginatedAsync(pageNumber, pageSize)); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        /// <summary>Create a new user. Admin only.</summary>
        [HttpPost]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest req)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
                    return BadRequest(new { message = "Username and password are required." });

                var existing = await _repo.GetByUsernameAsync(req.Username);
                if (existing is not null)
                    return Conflict(new { message = "Username already exists." });

                var user = new User
                {
                    FullName = req.FullName,
                    Username = req.Username,
                    Password = PasswordHasher.Hash(req.Password),
                    Role     = req.Role,
                    Phone    = req.Phone,
                    Email    = req.Email,
                    Status   = req.Status,
                };
                await _repo.AddAsync(user);
                return Ok(new { message = "User created successfully." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        /// <summary>Update user. Admin only.</summary>
        [HttpPut("{id:int}")]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest req)
        {
            try
            {
                var existing = await _repo.GetByIdAsync(id);
                if (existing is null) return NotFound(new { message = "User not found." });

                existing.FullName = req.FullName;
                existing.Username = req.Username;
                existing.Role     = req.Role;
                existing.Phone    = req.Phone;
                existing.Email    = req.Email;
                existing.Status   = req.Status;
                if (!string.IsNullOrWhiteSpace(req.NewPassword))
                    existing.Password = PasswordHasher.Hash(req.NewPassword);

                await _repo.UpdateAsync(existing);
                return Ok(new { message = "User updated successfully." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        /// <summary>Delete user. Admin only.</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existing = await _repo.GetByIdAsync(id);
                if (existing is null) return NotFound(new { message = "User not found." });
                await _repo.DeleteAsync(id);
                return Ok(new { message = "User deleted successfully." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }
    }
}
