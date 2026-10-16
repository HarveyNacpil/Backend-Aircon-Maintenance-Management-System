using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.AuthorizationModule;
using Api.CustomerModule;
using System.Linq;

namespace Api.Controllers
{
    public sealed record CreateCustomerRequest(
        string FirstName, string LastName,
        string? Phone, string? Email, string? Address,
        string? RegionCode, string? RegionName,
        string? ProvinceCode, string? ProvinceName,
        string? CityCode, string? CityName,
        string? BarangayCode, string? BarangayName,
        string? StreetAddress, double? Latitude, double? Longitude,
        int? StoreId = null);

    public sealed record UpdateCustomerRequest(
        string FirstName, string LastName,
        string? Phone, string? Email, string? Address,
        string? RegionCode, string? RegionName,
        string? ProvinceCode, string? ProvinceName,
        string? CityCode, string? CityName,
        string? BarangayCode, string? BarangayName,
        string? StreetAddress, double? Latitude, double? Longitude,
        int? StoreId = null);

    public sealed record UpdateCustomerCoordinatesRequest(double Latitude, double Longitude);

    [ApiController]
    [Route("api/customers")]
    [Produces("application/json")]
    [Authorize]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerRepository _repo;
        private readonly IConfiguration _config;

        public CustomerController(ICustomerRepository repo, IConfiguration config)
        {
            _repo = repo;
            _config = config;
        }

        /// <summary>Get all customers.</summary>
        [HttpGet]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> GetAll()
        {
            try   { return Ok(await _repo.GetAllAsync()); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        /// <summary>Get customer by ID.</summary>
        [HttpGet("{id:int}")]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var c = await _repo.GetByIdAsync(id);
                return c is null ? NotFound(new { message = "Customer not found." }) : Ok(c);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        /// <summary>Search customers.</summary>
        [HttpGet("search")]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            try   { return Ok(await _repo.SearchAsync(query)); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        /// <summary>Get paginated customers.</summary>
        [HttpGet("paginated")]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> GetPaginated(int pageNumber = 1, int pageSize = 25)
        {
            try   { return Ok(await _repo.GetPaginatedAsync(pageNumber, pageSize)); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        /// <summary>Create customer. Admin or Staff.</summary>
        [HttpPost]
        [Authorize(Policy = AppPolicies.AdminOrTech)]
        public async Task<IActionResult> Create([FromBody] CreateCustomerRequest req)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(req.FirstName) || string.IsNullOrWhiteSpace(req.LastName))
                    return BadRequest(new { message = "First name and last name are required." });

                string formattedAddress = !string.IsNullOrWhiteSpace(req.Address)
                    ? req.Address
                    : string.Join(", ", new[] { req.StreetAddress, req.BarangayName, req.CityName, req.ProvinceName, req.RegionName }
                        .Where(s => !string.IsNullOrWhiteSpace(s)));

                var customer = new Customer
                {
                    FirstName     = req.FirstName,
                    LastName      = req.LastName,
                    Phone         = req.Phone,
                    Email         = req.Email,
                    Address       = string.IsNullOrWhiteSpace(formattedAddress) ? null : formattedAddress,
                    RegionCode    = req.RegionCode,
                    RegionName    = req.RegionName,
                    ProvinceCode  = req.ProvinceCode,
                    ProvinceName  = req.ProvinceName,
                    CityCode      = req.CityCode,
                    CityName      = req.CityName,
                    BarangayCode  = req.BarangayCode,
                    BarangayName  = req.BarangayName,
                    StreetAddress = req.StreetAddress,
                    Latitude      = req.Latitude,
                    Longitude     = req.Longitude,
                    StoreId       = req.StoreId,
                };
                await _repo.AddAsync(customer);
                return Ok(new { message = "Customer created successfully." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        /// <summary>Update customer.</summary>
        [HttpPut("{id:int}")]
        [Authorize(Policy = AppPolicies.AdminOrTech)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerRequest req)
        {
            try
            {
                var existing = await _repo.GetByIdAsync(id);
                if (existing is null) return NotFound(new { message = "Customer not found." });

                string formattedAddress = !string.IsNullOrWhiteSpace(req.Address)
                    ? req.Address
                    : string.Join(", ", new[] { req.StreetAddress, req.BarangayName, req.CityName, req.ProvinceName, req.RegionName }
                        .Where(s => !string.IsNullOrWhiteSpace(s)));

                existing.FirstName     = req.FirstName;
                existing.LastName      = req.LastName;
                existing.Phone         = req.Phone;
                existing.Email         = req.Email;
                existing.Address       = string.IsNullOrWhiteSpace(formattedAddress) ? existing.Address : formattedAddress;
                existing.RegionCode    = req.RegionCode ?? existing.RegionCode;
                existing.RegionName    = req.RegionName ?? existing.RegionName;
                existing.ProvinceCode  = req.ProvinceCode ?? existing.ProvinceCode;
                existing.ProvinceName  = req.ProvinceName ?? existing.ProvinceName;
                existing.CityCode      = req.CityCode ?? existing.CityCode;
                existing.CityName      = req.CityName ?? existing.CityName;
                existing.BarangayCode  = req.BarangayCode ?? existing.BarangayCode;
                existing.BarangayName  = req.BarangayName ?? existing.BarangayName;
                existing.StreetAddress = req.StreetAddress ?? existing.StreetAddress;
                if (req.Latitude.HasValue) existing.Latitude = req.Latitude.Value;
                if (req.Longitude.HasValue) existing.Longitude = req.Longitude.Value;
                if (req.StoreId.HasValue) existing.StoreId = req.StoreId.Value;

                await _repo.UpdateAsync(existing);
                return Ok(new { message = "Customer updated successfully." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        /// <summary>Delete customer. Admin only.</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existing = await _repo.GetByIdAsync(id);
                if (existing is null) return NotFound(new { message = "Customer not found." });
                await _repo.DeleteAsync(id);
                return Ok(new { message = "Customer deleted successfully." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        /// <summary>
        /// Get customers for Customer Map Locator.
        /// Rule: Only customers with valid coordinates AND >= 1 aircon units. Exactly 1 marker per customer.
        /// </summary>
        [HttpGet("map-locations")]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public async Task<IActionResult> GetMapLocations()
        {
            try
            {
                var locations = await _repo.GetCustomerMapLocationsAsync();
                return Ok(locations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Update customer coordinates. Admin or Staff.</summary>
        [HttpPut("{id:int}/coordinates")]
        [Authorize(Policy = AppPolicies.AdminOrTech)]
        public async Task<IActionResult> UpdateCoordinates(int id, [FromBody] UpdateCustomerCoordinatesRequest req)
        {
            try
            {
                var existing = await _repo.GetByIdAsync(id);
                if (existing is null) return NotFound(new { message = "Customer not found." });
                await _repo.UpdateCoordinatesAsync(id, req.Latitude, req.Longitude);
                return Ok(new { message = "Coordinates updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Get Google Maps API configuration.</summary>
        [HttpGet("maps-config")]
        [Authorize(Policy = AppPolicies.AllRoles)]
        public IActionResult GetMapsConfig()
        {
            string? key = _config["GoogleMaps:ApiKey"];
            return Ok(new
            {
                hasGoogleMaps = !string.IsNullOrWhiteSpace(key) && key != "YOUR_GOOGLE_MAPS_API_KEY",
                apiKey = (!string.IsNullOrWhiteSpace(key) && key != "YOUR_GOOGLE_MAPS_API_KEY") ? key : null
            });
        }
    }
}
