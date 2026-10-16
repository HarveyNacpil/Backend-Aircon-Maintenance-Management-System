using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.AuthorizationModule;
using Api.ServiceRequestModule;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/service-requests")]
    [Produces("application/json")]
    [Authorize]
    public class ServiceRequestController : ControllerBase
    {
        private readonly IServiceRequestRepository _repo;
        public ServiceRequestController(IServiceRequestRepository repo) => _repo = repo;

        [HttpGet]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> GetAll()
        {
            try   { return Ok(await _repo.GetAllAsync()); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("{id:int}")]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var sr = await _repo.GetByIdAsync(id);
                return sr is null ? NotFound(new { message = "Service request not found." }) : Ok(sr);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("by-customer/{customerId:int}")]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> GetByCustomer(int customerId)
        {
            try   { return Ok(await _repo.GetByCustomerAsync(customerId)); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("by-technician/{technicianId:int}")]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> GetByTechnician(int technicianId)
        {
            try   { return Ok(await _repo.GetByTechnicianAsync(technicianId)); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("by-status")]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> GetByStatus([FromQuery] string status)
        {
            try   { return Ok(await _repo.GetByStatusAsync(status)); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("paginated")]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> GetPaginated(int pageNumber = 1, int pageSize = 25)
        {
            try   { return Ok(await _repo.GetPaginatedAsync(pageNumber, pageSize)); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPost]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> Create([FromBody] ServiceRequest body)
        {
            try
            {
                if (body.CustomerId <= 0 || body.UnitId <= 0 || string.IsNullOrWhiteSpace(body.IssueDescription))
                    return BadRequest(new { message = "customer_id, unit_id, and issue_description are required." });
                await _repo.AddAsync(body);
                return Ok(new { message = "Service request created successfully." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = AppPolicies.AdminOrTech)]
        public async Task<IActionResult> Update(int id, [FromBody] ServiceRequest body)
        {
            try
            {
                var existing = await _repo.GetByIdAsync(id);
                if (existing is null) return NotFound(new { message = "Service request not found." });
                body.RequestId = id;
                await _repo.UpdateAsync(body);
                return Ok(new { message = "Service request updated successfully." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        /// <summary>Assign a technician to the request.</summary>
        [HttpPatch("{id:int}/assign")]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<IActionResult> AssignTechnician(int id, [FromQuery] int technicianId)
        {
            try
            {
                var existing = await _repo.GetByIdAsync(id);
                if (existing is null) return NotFound(new { message = "Service request not found." });
                await _repo.AssignTechnicianAsync(id, technicianId);
                return Ok(new { message = "Technician assigned." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        /// <summary>Update just the status of a request.</summary>
        [HttpPatch("{id:int}/status")]
        [Authorize(Policy = AppPolicies.AdminOrTech)]
        public async Task<IActionResult> UpdateStatus(int id, [FromQuery] string status)
        {
            try
            {
                var existing = await _repo.GetByIdAsync(id);
                if (existing is null) return NotFound(new { message = "Service request not found." });
                await _repo.UpdateStatusAsync(id, status);
                return Ok(new { message = "Status updated." });
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
                if (existing is null) return NotFound(new { message = "Service request not found." });
                await _repo.DeleteAsync(id);
                return Ok(new { message = "Service request deleted." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }
    }
}
