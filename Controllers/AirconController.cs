using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.AuthorizationModule;
using Api.Airconmodule;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/aircon-units")]
    [Produces("application/json")]
    [Authorize]
    public class AirconController : ControllerBase
    {
        private readonly IAirconRepository _repo;
        public AirconController(IAirconRepository repo) => _repo = repo;

        /// <summary>Get all aircon units.</summary>
        [HttpGet]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> GetAll()
        {
            try   { return Ok(await _repo.GetAllAsync()); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        /// <summary>Get aircon unit by ID.</summary>
        [HttpGet("{id:int}")]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var unit = await _repo.GetByIdAsync(id);
                return unit is null ? NotFound(new { message = "Aircon unit not found." }) : Ok(unit);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        /// <summary>Get all aircon units for a customer.</summary>
        [HttpGet("by-customer/{customerId:int}")]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> GetByCustomer(int customerId)
        {
            try   { return Ok(await _repo.GetByCustomerIdAsync(customerId)); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        /// <summary>Search aircon units.</summary>
        [HttpGet("search")]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            try   { return Ok(await _repo.SearchAsync(query)); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        /// <summary>Get paginated aircon units.</summary>
        [HttpGet("paginated")]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> GetPaginated(int pageNumber = 1, int pageSize = 25)
        {
            try   { return Ok(await _repo.GetPaginatedAsync(pageNumber, pageSize)); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        /// <summary>Register a new aircon unit.</summary>
        [HttpPost]
        [Authorize(Policy = AppPolicies.AdminOrTech)]
        public async Task<IActionResult> Create([FromBody] AirconUnit unit)
        {
            try
            {
                if (unit.CustomerId <= 0)
                    return BadRequest(new { message = "customer_id is required." });
                await _repo.AddAsync(unit);
                return Ok(new { message = "Aircon unit created successfully." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        /// <summary>Update aircon unit.</summary>
        [HttpPut("{id:int}")]
        [Authorize(Policy = AppPolicies.AdminOrTech)]
        public async Task<IActionResult> Update(int id, [FromBody] AirconUnit unit)
        {
            try
            {
                if (unit.UnitId == 0) unit.UnitId = id;
                if (id != unit.UnitId) return BadRequest(new { message = "Route id does not match unit_id." });

                var existing = await _repo.GetByIdAsync(id);
                if (existing is null) return NotFound(new { message = "Aircon unit not found." });

                await _repo.UpdateAsync(unit);
                return Ok(new { message = "Aircon unit updated successfully." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        /// <summary>Delete aircon unit. Admin only.</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existing = await _repo.GetByIdAsync(id);
                if (existing is null) return NotFound(new { message = "Aircon unit not found." });
                await _repo.DeleteAsync(id);
                return Ok(new { message = "Aircon unit deleted successfully." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }
    }
}
