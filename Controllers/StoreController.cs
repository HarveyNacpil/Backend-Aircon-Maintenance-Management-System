using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.AuthorizationModule;
using Api.StoreModule;

namespace Api.Controllers
{
    public sealed record CreateStoreRequest(
        string Name,
        string Code,
        string? Status,
        bool? IsMainBranch,
        string? Street,
        string? Barangay,
        string? CityMunicipality,
        string? Province,
        string? Region,
        string? PostalCode,
        double Latitude,
        double Longitude,
        string? LandmarkNotes,
        string? LocationSource,
        bool? IsVerified,
        string? Phone,
        string? Mobile,
        string? Email,
        string? WebsiteUrl,
        string? OpeningHours,
        double? ServiceRadiusKm,
        string? ServicesOffered,
        bool? AcceptsWalkIns,
        string? ParkingNotes,
        int? ManagerId,
        string? AssignedTechnicians,
        string? BusinessName,
        string? RegistrationNumber,
        string? LogoUrl);

    public sealed record UpdateStoreRequest(
        string Name,
        string Code,
        string? Status,
        bool? IsMainBranch,
        string? Street,
        string? Barangay,
        string? CityMunicipality,
        string? Province,
        string? Region,
        string? PostalCode,
        double Latitude,
        double Longitude,
        string? LandmarkNotes,
        string? LocationSource,
        bool? IsVerified,
        string? Phone,
        string? Mobile,
        string? Email,
        string? WebsiteUrl,
        string? OpeningHours,
        double? ServiceRadiusKm,
        string? ServicesOffered,
        bool? AcceptsWalkIns,
        string? ParkingNotes,
        int? ManagerId,
        string? AssignedTechnicians,
        string? BusinessName,
        string? RegistrationNumber,
        string? LogoUrl);

    public sealed record ReassignAndDeleteRequest(int TargetStoreId);

    [ApiController]
    [Route("api/stores")]
    [Produces("application/json")]
    [Authorize]
    public class StoreController : ControllerBase
    {
        private readonly IStoreRepository _repo;

        public StoreController(IStoreRepository repo)
        {
            _repo = repo;
        }

        private static bool IsInPhilippines(double lat, double lng)
        {
            // Bounding box of the Philippine archipelago
            return lat >= 4.0 && lat <= 22.0 && lng >= 115.0 && lng <= 128.0;
        }

        /// <summary>Get all branches/stores.</summary>
        [HttpGet]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var stores = await _repo.GetAllAsync();
                return Ok(stores);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Get store by ID.</summary>
        [HttpGet("{id:int}")]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var store = await _repo.GetByIdAsync(id);
                if (store == null) return NotFound(new { message = $"Store with ID #{id} was not found." });
                return Ok(store);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Create a new branch/store (Admin only).</summary>
        [HttpPost]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<IActionResult> Create([FromBody] CreateStoreRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return BadRequest(new { message = "Branch name is required." });

            if (string.IsNullOrWhiteSpace(req.Code))
                return BadRequest(new { message = "Branch code (e.g. OLG-01) is required." });

            if (!IsInPhilippines(req.Latitude, req.Longitude))
                return BadRequest(new { message = "Coordinates must be within the Philippines (Latitude: 4.0 - 22.0, Longitude: 115.0 - 128.0)." });

            try
            {
                var store = new Store
                {
                    Name               = req.Name.Trim(),
                    Code               = req.Code.Trim().ToUpperInvariant(),
                    Status             = string.IsNullOrWhiteSpace(req.Status) ? "Open" : req.Status,
                    IsMainBranch       = req.IsMainBranch ?? false,
                    Street             = req.Street?.Trim(),
                    Barangay           = req.Barangay?.Trim(),
                    CityMunicipality   = req.CityMunicipality?.Trim(),
                    Province           = req.Province?.Trim(),
                    Region             = req.Region?.Trim(),
                    PostalCode         = req.PostalCode?.Trim(),
                    Latitude           = req.Latitude,
                    Longitude          = req.Longitude,
                    LandmarkNotes      = req.LandmarkNotes?.Trim(),
                    LocationSource     = string.IsNullOrWhiteSpace(req.LocationSource) ? "Pin dropped" : req.LocationSource,
                    IsVerified         = req.IsVerified ?? false,
                    Phone              = req.Phone?.Trim(),
                    Mobile             = req.Mobile?.Trim(),
                    Email              = req.Email?.Trim(),
                    WebsiteUrl         = req.WebsiteUrl?.Trim(),
                    OpeningHours       = req.OpeningHours,
                    ServiceRadiusKm    = req.ServiceRadiusKm ?? 15.0,
                    ServicesOffered    = req.ServicesOffered,
                    AcceptsWalkIns     = req.AcceptsWalkIns ?? true,
                    ParkingNotes       = req.ParkingNotes?.Trim(),
                    ManagerId          = req.ManagerId,
                    AssignedTechnicians = req.AssignedTechnicians,
                    BusinessName       = req.BusinessName?.Trim(),
                    RegistrationNumber = req.RegistrationNumber?.Trim(),
                    LogoUrl            = req.LogoUrl?.Trim()
                };

                int newId = await _repo.AddAsync(store);
                var created = await _repo.GetByIdAsync(newId);
                return CreatedAtAction(nameof(GetById), new { id = newId }, created);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Update an existing branch/store (Admin only).</summary>
        [HttpPut("{id:int}")]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateStoreRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return BadRequest(new { message = "Branch name is required." });

            if (string.IsNullOrWhiteSpace(req.Code))
                return BadRequest(new { message = "Branch code is required." });

            if (!IsInPhilippines(req.Latitude, req.Longitude))
                return BadRequest(new { message = "Coordinates must be within the Philippines (Latitude: 4.0 - 22.0, Longitude: 115.0 - 128.0)." });

            try
            {
                var existing = await _repo.GetByIdAsync(id);
                if (existing == null) return NotFound(new { message = $"Store with ID #{id} was not found." });

                existing.Name               = req.Name.Trim();
                existing.Code               = req.Code.Trim().ToUpperInvariant();
                existing.Status             = string.IsNullOrWhiteSpace(req.Status) ? existing.Status : req.Status;
                existing.IsMainBranch       = req.IsMainBranch ?? existing.IsMainBranch;
                existing.Street             = req.Street?.Trim();
                existing.Barangay           = req.Barangay?.Trim();
                existing.CityMunicipality   = req.CityMunicipality?.Trim();
                existing.Province           = req.Province?.Trim();
                existing.Region             = req.Region?.Trim();
                existing.PostalCode         = req.PostalCode?.Trim();
                existing.Latitude           = req.Latitude;
                existing.Longitude          = req.Longitude;
                existing.LandmarkNotes      = req.LandmarkNotes?.Trim();
                existing.LocationSource     = string.IsNullOrWhiteSpace(req.LocationSource) ? existing.LocationSource : req.LocationSource;
                existing.IsVerified         = req.IsVerified ?? existing.IsVerified;
                existing.Phone              = req.Phone?.Trim();
                existing.Mobile             = req.Mobile?.Trim();
                existing.Email              = req.Email?.Trim();
                existing.WebsiteUrl         = req.WebsiteUrl?.Trim();
                existing.OpeningHours       = req.OpeningHours ?? existing.OpeningHours;
                existing.ServiceRadiusKm    = req.ServiceRadiusKm ?? existing.ServiceRadiusKm;
                existing.ServicesOffered    = req.ServicesOffered ?? existing.ServicesOffered;
                existing.AcceptsWalkIns     = req.AcceptsWalkIns ?? existing.AcceptsWalkIns;
                existing.ParkingNotes       = req.ParkingNotes?.Trim();
                existing.ManagerId          = req.ManagerId;
                existing.AssignedTechnicians = req.AssignedTechnicians;
                existing.BusinessName       = req.BusinessName?.Trim();
                existing.RegistrationNumber = req.RegistrationNumber?.Trim();
                existing.LogoUrl            = req.LogoUrl?.Trim();

                await _repo.UpdateAsync(existing);
                var updated = await _repo.GetByIdAsync(id);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Check if branch can be deleted or has dependencies.</summary>
        [HttpGet("{id:int}/check-delete")]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<IActionResult> CheckDelete(int id)
        {
            try
            {
                var check = await _repo.CheckCanDeleteAsync(id);
                return Ok(check);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Delete branch/store (Admin only). Returns 400 if active associations exist.</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var store = await _repo.GetByIdAsync(id);
                if (store == null) return NotFound(new { message = $"Store with ID #{id} was not found." });

                var check = await _repo.CheckCanDeleteAsync(id);
                if (!check.CanDelete)
                {
                    return BadRequest(check);
                }

                await _repo.DeleteAsync(id);
                return Ok(new { message = $"Branch '{store.Name}' was deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Reassign all customers, technicians, and requests to target store, then delete source store (Admin only).</summary>
        [HttpPost("{id:int}/reassign-and-delete")]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<IActionResult> ReassignAndDelete(int id, [FromBody] ReassignAndDeleteRequest req)
        {
            if (id == req.TargetStoreId)
                return BadRequest(new { message = "Source and target branch cannot be the same." });

            try
            {
                var source = await _repo.GetByIdAsync(id);
                if (source == null) return NotFound(new { message = $"Source branch #{id} not found." });

                var target = await _repo.GetByIdAsync(req.TargetStoreId);
                if (target == null) return NotFound(new { message = $"Target branch #{req.TargetStoreId} not found." });

                await _repo.ReassignAndDeleteAsync(id, req.TargetStoreId);
                return Ok(new { message = $"Successfully reassigned all associations from '{source.Name}' to '{target.Name}' and deleted branch." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Set a branch as the Main Branch (Admin only).</summary>
        [HttpPost("{id:int}/set-main")]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<IActionResult> SetMain(int id)
        {
            try
            {
                var store = await _repo.GetByIdAsync(id);
                if (store == null) return NotFound(new { message = $"Store #{id} not found." });

                await _repo.SetMainBranchAsync(id);
                return Ok(new { message = $"Branch '{store.Name}' is now set as the Main Branch." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
