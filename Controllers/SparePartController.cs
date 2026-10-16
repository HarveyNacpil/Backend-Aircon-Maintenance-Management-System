using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.AuthorizationModule;
using Api.SparePartsModule;

namespace Api.Controllers
{
    [ApiController]
    [Route("api")]
    [Produces("application/json")]
    [Authorize]
    public class SparePartController : ControllerBase
    {
        private readonly ISparePartRepository _repo;
        public SparePartController(ISparePartRepository repo) => _repo = repo;

        // ── Spare Parts ───────────────────────────────────────────────────────

        [HttpGet("spare-parts")]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> GetAll()
        {
            try   { return Ok(await _repo.GetAllPartsAsync()); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("spare-parts/{id:int}")]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var p = await _repo.GetPartByIdAsync(id);
                return p is null ? NotFound(new { message = "Part not found." }) : Ok(p);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("spare-parts/search")]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            try   { return Ok(await _repo.SearchPartsAsync(query)); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("spare-parts/paginated")]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> GetPaginated(int pageNumber = 1, int pageSize = 25)
        {
            try   { return Ok(await _repo.GetPartsPaginatedAsync(pageNumber, pageSize)); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPost("spare-parts")]
        [Authorize(Policy = AppPolicies.AdminOrTech)]
        public async Task<IActionResult> Create([FromBody] SparePart part)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(part.PartName))
                    return BadRequest(new { message = "part_name is required." });
                await _repo.AddPartAsync(part);
                return Ok(new { message = "Spare part created successfully." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPut("spare-parts/{id:int}")]
        [Authorize(Policy = AppPolicies.AdminOrTech)]
        public async Task<IActionResult> Update(int id, [FromBody] SparePart part)
        {
            try
            {
                var existing = await _repo.GetPartByIdAsync(id);
                if (existing is null) return NotFound(new { message = "Part not found." });
                part.PartId = id;
                await _repo.UpdatePartAsync(part);
                return Ok(new { message = "Spare part updated successfully." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpDelete("spare-parts/{id:int}")]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existing = await _repo.GetPartByIdAsync(id);
                if (existing is null) return NotFound(new { message = "Part not found." });
                await _repo.DeletePartAsync(id);
                return Ok(new { message = "Spare part deleted successfully." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── Parts Usage ───────────────────────────────────────────────────────

        [HttpGet("parts-usage/by-record/{recordId:int}")]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> GetUsageByRecord(int recordId)
        {
            try   { return Ok(await _repo.GetUsageByRecordAsync(recordId)); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPost("parts-usage")]
        [Authorize(Policy = AppPolicies.AdminOrTech)]
        public async Task<IActionResult> AddUsage([FromBody] PartUsage usage)
        {
            try
            {
                if (usage.RecordId <= 0 || usage.PartId <= 0 || usage.QuantityUsed <= 0)
                    return BadRequest(new { message = "record_id, part_id, and quantity_used are required." });
                await _repo.AddUsageAsync(usage);
                return Ok(new { message = "Usage recorded successfully." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpDelete("parts-usage/{id:int}")]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<IActionResult> DeleteUsage(int id)
        {
            try
            {
                await _repo.DeleteUsageAsync(id);
                return Ok(new { message = "Usage deleted." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }
    }
}
