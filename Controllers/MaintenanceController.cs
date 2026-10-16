using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.AuthorizationModule;
using Api.Maintenance;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/maintenance")]
    [Produces("application/json")]
    [Authorize]
    public class MaintenanceController : ControllerBase
    {
        private readonly IMaintenanceRepository _repo;
        public MaintenanceController(IMaintenanceRepository repo) => _repo = repo;

        // ── Schedules ─────────────────────────────────────────────────────────

        [HttpGet("schedules")]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> GetAllSchedules()
        {
            try   { return Ok(await _repo.GetAllSchedulesAsync()); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("schedules/{id:int}")]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> GetScheduleById(int id)
        {
            try
            {
                var s = await _repo.GetScheduleByIdAsync(id);
                return s is null ? NotFound(new { message = "Schedule not found." }) : Ok(s);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("schedules/by-customer/{customerId:int}")]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> GetSchedulesByCustomer(int customerId)
        {
            try   { return Ok(await _repo.GetSchedulesByCustomerAsync(customerId)); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("schedules/by-technician/{technicianId:int}")]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> GetSchedulesByTechnician(int technicianId)
        {
            try   { return Ok(await _repo.GetSchedulesByTechnicianAsync(technicianId)); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("schedules/paginated")]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> GetSchedulesPaginated(int pageNumber = 1, int pageSize = 25)
        {
            try   { return Ok(await _repo.GetSchedulesPaginatedAsync(pageNumber, pageSize)); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPost("schedules")]
        [Authorize(Policy = AppPolicies.AdminOrTech)]
        public async Task<IActionResult> CreateSchedule([FromBody] MaintenanceSchedule body)
        {
            try
            {
                if (body.CustomerId <= 0 || body.UnitId <= 0 || body.TechnicianId <= 0)
                    return BadRequest(new { message = "customer_id, unit_id, and technician_id are required." });
                await _repo.AddScheduleAsync(body);
                return Ok(new { message = "Schedule created successfully." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPut("schedules/{id:int}")]
        [Authorize(Policy = AppPolicies.AdminOrTech)]
        public async Task<IActionResult> UpdateSchedule(int id, [FromBody] MaintenanceSchedule body)
        {
            try
            {
                var existing = await _repo.GetScheduleByIdAsync(id);
                if (existing is null) return NotFound(new { message = "Schedule not found." });
                body.ScheduleId = id;
                await _repo.UpdateScheduleAsync(body);
                return Ok(new { message = "Schedule updated successfully." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpDelete("schedules/{id:int}")]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            try
            {
                var existing = await _repo.GetScheduleByIdAsync(id);
                if (existing is null) return NotFound(new { message = "Schedule not found." });
                await _repo.DeleteScheduleAsync(id);
                return Ok(new { message = "Schedule deleted successfully." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── Records ───────────────────────────────────────────────────────────

        [HttpGet("records")]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> GetAllRecords()
        {
            try   { return Ok(await _repo.GetAllRecordsAsync()); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("records/{id:int}")]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> GetRecordById(int id)
        {
            try
            {
                var r = await _repo.GetRecordByIdAsync(id);
                return r is null ? NotFound(new { message = "Record not found." }) : Ok(r);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("records/by-schedule/{scheduleId:int}")]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> GetRecordBySchedule(int scheduleId)
        {
            try
            {
                var r = await _repo.GetRecordByScheduleIdAsync(scheduleId);
                return r is null ? NotFound(new { message = "No record for that schedule." }) : Ok(r);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("records/paginated")]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> GetRecordsPaginated(int pageNumber = 1, int pageSize = 25)
        {
            try   { return Ok(await _repo.GetRecordsPaginatedAsync(pageNumber, pageSize)); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPost("records")]
        [Authorize(Policy = AppPolicies.AdminOrTech)]
        public async Task<IActionResult> CreateRecord([FromBody] MaintenanceRecord body)
        {
            try
            {
                if (body.ScheduleId <= 0)
                    return BadRequest(new { message = "schedule_id is required." });
                await _repo.AddRecordAsync(body);
                return Ok(new { message = "Record created successfully." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPut("records/{id:int}")]
        [Authorize(Policy = AppPolicies.AdminOrTech)]
        public async Task<IActionResult> UpdateRecord(int id, [FromBody] MaintenanceRecord body)
        {
            try
            {
                var existing = await _repo.GetRecordByIdAsync(id);
                if (existing is null) return NotFound(new { message = "Record not found." });
                body.RecordId = id;
                await _repo.UpdateRecordAsync(body);
                return Ok(new { message = "Record updated successfully." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpDelete("records/{id:int}")]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<IActionResult> DeleteRecord(int id)
        {
            try
            {
                var existing = await _repo.GetRecordByIdAsync(id);
                if (existing is null) return NotFound(new { message = "Record not found." });
                await _repo.DeleteRecordAsync(id);
                return Ok(new { message = "Record deleted successfully." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }
    }
}
