using Microsoft.AspNetCore.Mvc;
using BagoScout.Data;
using BagoScout.Models;
using Microsoft.EntityFrameworkCore;

namespace BagoScout.Controllers
{
    public class PreferencesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PreferencesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("api/preferences/get")]
        public async Task<IActionResult> GetPreferences()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not logged in" });
                }

                var preferences = await _context.JobPreferences
                    .FirstOrDefaultAsync(p => p.UserId == userId.Value);

                if (preferences == null)
                {
                    return Ok(new
                    {
                        preferredJobType = "",
                        preferredJobTitles = "",
                        minSalary = (int?)null,
                        maxSalary = (int?)null,
                        preferredLocation = "",
                        preferredLatitude = (double?)null,
                        preferredLongitude = (double?)null,
                        maxDistance = (int?)null,
                        preferredExperienceLevel = ""
                    });
                }

                return Ok(new
                {
                    preferences.PreferredJobType,
                    preferences.PreferredJobTitles,
                    preferences.MinSalary,
                    preferences.MaxSalary,
                    preferences.PreferredLocation,
                    preferences.PreferredLatitude,
                    preferences.PreferredLongitude,
                    preferences.MaxDistance,
                    preferences.PreferredExperienceLevel
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching preferences", error = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/preferences/save")]
        public async Task<IActionResult> SavePreferences([FromBody] JobPreferenceRequest request)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not logged in" });
                }

                var existingPreference = await _context.JobPreferences
                    .FirstOrDefaultAsync(p => p.UserId == userId.Value);

                if (existingPreference != null)
                {
                    // Update existing
                    existingPreference.PreferredJobType = request.PreferredJobType;
                    existingPreference.PreferredJobTitles = request.PreferredJobTitles;
                    existingPreference.MinSalary = request.MinSalary;
                    existingPreference.MaxSalary = request.MaxSalary;
                    existingPreference.PreferredLocation = request.PreferredLocation;
                    existingPreference.PreferredLatitude = request.PreferredLatitude;
                    existingPreference.PreferredLongitude = request.PreferredLongitude;
                    existingPreference.MaxDistance = request.MaxDistance;
                    existingPreference.PreferredExperienceLevel = request.PreferredExperienceLevel;
                    existingPreference.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // Create new
                    var newPreference = new JobPreference
                    {
                        UserId = userId.Value,
                        PreferredJobType = request.PreferredJobType,
                        PreferredJobTitles = request.PreferredJobTitles,
                        MinSalary = request.MinSalary,
                        MaxSalary = request.MaxSalary,
                        PreferredLocation = request.PreferredLocation,
                        PreferredLatitude = request.PreferredLatitude,
                        PreferredLongitude = request.PreferredLongitude,
                        MaxDistance = request.MaxDistance,
                        PreferredExperienceLevel = request.PreferredExperienceLevel,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.JobPreferences.Add(newPreference);
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Preferences saved successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error saving preferences", error = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/preferences/recommended-jobs")]
        public async Task<IActionResult> GetRecommendedJobs()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not logged in" });
                }

                var preferences = await _context.JobPreferences
                    .FirstOrDefaultAsync(p => p.UserId == userId.Value);

                var allJobs = await _context.Jobs
                    .Where(j => j.IsActive)
                    .OrderByDescending(j => j.CreatedAt)
                    .ToListAsync();

                if (preferences == null || 
                    (string.IsNullOrEmpty(preferences.PreferredJobType) && 
                     string.IsNullOrEmpty(preferences.PreferredJobTitles) && 
                     string.IsNullOrEmpty(preferences.PreferredExperienceLevel) &&
                     preferences.MinSalary == null && 
                     preferences.MaxSalary == null &&
                     string.IsNullOrEmpty(preferences.PreferredLocation)))
                {
                    // No preferences set, return all jobs without match info
                    var allJobsResult = allJobs.Take(10).Select(j => new
                    {
                        j.JobId,
                        j.Title,
                        j.Company,
                        j.Address,
                        j.SalaryRange,
                        j.JobType,
                        j.ExperienceLevel,
                        j.Description,
                        j.CreatedAt,
                        matchedCriteria = new List<string>()
                    }).ToList();

                    return Ok(allJobsResult);
                }

                // Parse preferred location coordinates if available
                // (Removed unused variables)

                // Calculate matches for each job
                var jobsWithMatches = allJobs.Select(job =>
                {
                    var matches = new List<string>();

                    // Check job type match
                    if (!string.IsNullOrEmpty(preferences.PreferredJobType) && 
                        job.JobType == preferences.PreferredJobType)
                    {
                        matches.Add("Job Type");
                    }

                    // Check experience level match
                    if (!string.IsNullOrEmpty(preferences.PreferredExperienceLevel) && 
                        job.ExperienceLevel == preferences.PreferredExperienceLevel)
                    {
                        matches.Add("Experience");
                    }

                    // Check job title match
                    if (!string.IsNullOrEmpty(preferences.PreferredJobTitles))
                    {
                        var titles = preferences.PreferredJobTitles.Split(',').Select(t => t.Trim().ToLower()).ToList();
                        if (titles.Any(t => job.Title.ToLower().Contains(t)))
                        {
                            matches.Add("Job Title");
                        }
                    }

                    // Check salary range match
                    if (!string.IsNullOrEmpty(job.SalaryRange))
                    {
                        var salaryParts = job.SalaryRange.Replace("₱", "").Replace(",", "").Split('-');
                        if (salaryParts.Length == 2)
                        {
                            if (int.TryParse(salaryParts[0].Trim(), out int jobMinSalary) &&
                                int.TryParse(salaryParts[1].Trim(), out int jobMaxSalary))
                            {
                                bool salaryMatch = false;

                                if (preferences.MinSalary.HasValue && preferences.MaxSalary.HasValue)
                                {
                                    // Check if there's any overlap between job salary and preferred salary
                                    salaryMatch = !(jobMaxSalary < preferences.MinSalary || jobMinSalary > preferences.MaxSalary);
                                }
                                else if (preferences.MinSalary.HasValue)
                                {
                                    salaryMatch = jobMaxSalary >= preferences.MinSalary;
                                }
                                else if (preferences.MaxSalary.HasValue)
                                {
                                    salaryMatch = jobMinSalary <= preferences.MaxSalary;
                                }

                                if (salaryMatch)
                                {
                                    matches.Add("Salary");
                                }
                            }
                        }
                    }

                    // Check location/distance match using raw database values
                    try
                    {
                        // Get max distance value
                        var maxDistProp = preferences.GetType().GetProperty("MaxDistance");
                        var maxDistObj = maxDistProp?.GetValue(preferences);
                        
                        if (!string.IsNullOrEmpty(preferences.PreferredLocation) && 
                            maxDistObj != null)
                        {
                            int maxDist = Convert.ToInt32(maxDistObj);
                            
                            // Get job coordinates
                            var jobLatProp = job.GetType().GetProperty("Latitude");
                            var jobLngProp = job.GetType().GetProperty("Longitude");
                            var jobLatObj = jobLatProp?.GetValue(job);
                            var jobLngObj = jobLngProp?.GetValue(job);
                            
                            if (jobLatObj != null && jobLngObj != null)
                            {
                                double jobLat = Convert.ToDouble(jobLatObj);
                                double jobLng = Convert.ToDouble(jobLngObj);
                                
                                // Use reflection to safely access properties
                                var prefType = preferences.GetType();
                                var latProp = prefType.GetProperty("PreferredLatitude");
                                var lngProp = prefType.GetProperty("PreferredLongitude");
                                
                                if (latProp != null && lngProp != null)
                                {
                                    var latObj = latProp.GetValue(preferences);
                                    var lngObj = lngProp.GetValue(preferences);
                                    
                                    if (latObj != null && lngObj != null)
                                    {
                                        double prefLat = Convert.ToDouble(latObj);
                                        double prefLng = Convert.ToDouble(lngObj);
                                        
                                        // Calculate distance
                                        const double R = 6371; // Earth radius in km
                                        var dLat = (jobLat - prefLat) * Math.PI / 180;
                                        var dLon = (jobLng - prefLng) * Math.PI / 180;
                                        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                                                Math.Cos(prefLat * Math.PI / 180) * Math.Cos(jobLat * Math.PI / 180) *
                                                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
                                        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                                        var distance = R * c;

                                        if (distance <= maxDist)
                                        {
                                            matches.Add("Location");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Silently ignore location matching errors
                    }

                    return new
                    {
                        job.JobId,
                        job.Title,
                        job.Company,
                        job.Address,
                        job.SalaryRange,
                        job.JobType,
                        job.ExperienceLevel,
                        job.Description,
                        job.CreatedAt,
                        matchedCriteria = matches,
                        matchCount = matches.Count
                    };
                })
                .Where(j => j.matchCount > 0) // Only show jobs with at least one match
                .OrderByDescending(j => j.matchCount) // Sort by number of matches
                .ThenByDescending(j => j.CreatedAt)
                .Take(10)
                .ToList();

                return Ok(jobsWithMatches);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching recommended jobs", error = ex.Message });
            }
        }

        // Haversine formula to calculate distance between two coordinates in kilometers
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth's radius in kilometers

            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }

    public class JobPreferenceRequest
    {
        public string? PreferredJobType { get; set; }
        public string? PreferredJobTitles { get; set; }
        public int? MinSalary { get; set; }
        public int? MaxSalary { get; set; }
        public string? PreferredLocation { get; set; }
        public double? PreferredLatitude { get; set; }
        public double? PreferredLongitude { get; set; }
        public int? MaxDistance { get; set; }
        public string? PreferredExperienceLevel { get; set; }
    }
}
