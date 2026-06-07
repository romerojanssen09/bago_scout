using Microsoft.AspNetCore.Mvc;
using BagoScout.Data;
using BagoScout.Models;
using Microsoft.EntityFrameworkCore;

namespace BagoScout.Controllers
{
    public class JobsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JobsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Search()
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Redirect("/?login=true");
            }

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View();
        }

        public IActionResult Manage()
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Redirect("/?login=true");
            }

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View();
        }

        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        [HttpGet]
        [Route("api/jobs/all")]
        public async Task<IActionResult> GetAllJobs()
        {
            try
            {
                var jobs = await _context.Jobs
                    .Where(j => j.IsActive)
                    .OrderByDescending(j => j.CreatedAt)
                    .Select(j => new
                    {
                        j.JobId,
                        j.Title,
                        j.Description,
                        j.Company,
                        j.Latitude,
                        j.Longitude,
                        j.Address,
                        j.SalaryRange,
                        j.JobType,
                        j.ExperienceLevel,
                        j.Requirements,
                        j.CreatedAt
                    })
                    .ToListAsync();

                return Ok(jobs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching jobs", error = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/jobs/employer")]
        public async Task<IActionResult> GetEmployerJobs()
        {
            try
            {
                var userId = await this.GetAuthorizedUserIdAsync(_context);
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not logged in" });
                }

                var jobs = await _context.Jobs
                    .Where(j => j.EmployerId == userId.Value)
                    .OrderByDescending(j => j.CreatedAt)
                    .Select(j => new
                    {
                        j.JobId,
                        j.Title,
                        j.Description,
                        j.Company,
                        j.Latitude,
                        j.Longitude,
                        j.Address,
                        j.SalaryRange,
                        j.JobType,
                        j.ExperienceLevel,
                        j.Requirements,
                        j.IsActive,
                        j.CreatedAt
                    })
                    .ToListAsync();

                return Ok(jobs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching employer jobs", error = ex.Message });
            }
        }

        [HttpDelete]
        [Route("api/jobs/delete/{id}")]
        public async Task<IActionResult> DeleteJob(int id)
        {
            try
            {
                var userId = await this.GetAuthorizedUserIdAsync(_context);
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not logged in" });
                }

                var job = await _context.Jobs
                    .FirstOrDefaultAsync(j => j.JobId == id && j.EmployerId == userId.Value);

                if (job == null)
                {
                    return NotFound(new { message = "Job not found or you don't have permission to delete it" });
                }

                _context.Jobs.Remove(job);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Job deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting job", error = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/jobs/create")]
        public async Task<IActionResult> CreateJob([FromBody] Job job)
        {
            try
            {
                var userId = await this.GetAuthorizedUserIdAsync(_context);
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not logged in" });
                }

                job.EmployerId = userId.Value;
                job.CreatedAt = DateTime.UtcNow;
                job.IsActive = true;

                _context.Jobs.Add(job);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Job posted successfully", jobId = job.JobId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating job", error = ex.Message });
            }
        }

        [HttpPut]
        [Route("api/jobs/update/{id}")]
        public async Task<IActionResult> UpdateJob(int id, [FromBody] Job updatedJob)
        {
            try
            {
                var userId = await this.GetAuthorizedUserIdAsync(_context);
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not logged in" });
                }

                var job = await _context.Jobs
                    .FirstOrDefaultAsync(j => j.JobId == id && j.EmployerId == userId.Value);

                if (job == null)
                {
                    return NotFound(new { message = "Job not found or you don't have permission to update it" });
                }

                // Update job fields
                job.Title = updatedJob.Title;
                job.Description = updatedJob.Description;
                job.Company = updatedJob.Company;
                job.JobType = updatedJob.JobType;
                job.ExperienceLevel = updatedJob.ExperienceLevel;
                job.SalaryRange = updatedJob.SalaryRange;
                job.Requirements = updatedJob.Requirements;
                job.Latitude = updatedJob.Latitude;
                job.Longitude = updatedJob.Longitude;
                job.Address = updatedJob.Address;
                job.IsActive = updatedJob.IsActive;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Job updated successfully", jobId = job.JobId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating job", error = ex.Message });
            }
        }

        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        [HttpGet]
        [Route("api/jobs/{id}")]
        public async Task<IActionResult> GetJobById(int id)
        {
            try
            {
                var userId = await this.GetAuthorizedUserIdAsync(_context);

                var job = await _context.Jobs
                    .FirstOrDefaultAsync(j => j.JobId == id);

                if (job == null)
                {
                    return NotFound(new { message = "Job not found" });
                }

                // If job is inactive, only the owner (employer) can view it
                if (!job.IsActive && (userId == null || job.EmployerId != userId.Value))
                {
                    return NotFound(new { message = "Job is inactive" });
                }

                return Ok(new
                {
                    job.JobId,
                    job.Title,
                    job.Description,
                    job.Company,
                    job.Latitude,
                    job.Longitude,
                    job.Address,
                    job.SalaryRange,
                    job.JobType,
                    job.ExperienceLevel,
                    job.Requirements,
                    job.IsActive,
                    job.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching job", error = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/jobs/apply")]
        public async Task<IActionResult> ApplyToJob([FromBody] Application application)
        {
            try
            {
                var userId = await this.GetAuthorizedUserIdAsync(_context);
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not logged in" });
                }

                application.SeekerId = userId.Value;
                application.AppliedAt = DateTime.UtcNow;
                application.Status = "Pending";

                // Check if already applied
                var existingApplication = await _context.Applications
                    .FirstOrDefaultAsync(a => a.JobId == application.JobId && a.SeekerId == userId.Value);

                if (existingApplication != null)
                {
                    return BadRequest(new { message = "You have already applied to this job" });
                }

                _context.Applications.Add(application);
                await _context.SaveChangesAsync();

                // Send push notification to employer
                var seeker = await _context.Users.FindAsync(userId.Value);
                var job = await _context.Jobs.FindAsync(application.JobId);
                if (job != null)
                {
                    var employer = await _context.Users.FindAsync(job.EmployerId);
                    if (employer != null && !string.IsNullOrEmpty(employer.FcmToken))
                    {
                        var seekerName = seeker != null ? $"{seeker.FirstName} {seeker.LastName}".Trim() : "A seeker";
                        var title = "New Application Received";
                        var body = $"{seekerName} has applied for '{job.Title}'.";
                        var data = new Dictionary<string, string>
                        {
                            { "type", "application" },
                            { "jobId", job.JobId.ToString() }
                        };
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await BagoScout.Services.PushNotificationHelper.SendNotificationAsync(employer.FcmToken, title, body, data);
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error sending application push notification: {ex.Message}");
                            }
                        });
                    }
                }

                return Ok(new { message = "Application submitted successfully", applicationId = application.ApplicationId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error submitting application", error = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/jobs/{jobId}/applications")]
        public async Task<IActionResult> GetJobApplications(int jobId)
        {
            try
            {
                var userId = await this.GetAuthorizedUserIdAsync(_context);
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not logged in" });
                }

                // Verify the job belongs to this employer
                var job = await _context.Jobs
                    .FirstOrDefaultAsync(j => j.JobId == jobId && j.EmployerId == userId.Value);

                if (job == null)
                {
                    return NotFound(new { message = "Job not found or you don't have permission" });
                }

                var applications = await _context.Applications
                    .Where(a => a.JobId == jobId)
                    .Join(_context.Users,
                        app => app.SeekerId,
                        user => user.UserId,
                        (app, user) => new
                        {
                            app.ApplicationId,
                            app.JobId,
                            app.SeekerId,
                            SeekerName = user.FirstName + " " + user.LastName,
                            SeekerEmail = user.Email,
                            SeekerPhone = user.PhoneNumber,
                            SeekerPhoto = user.SelfiePhotoPath,
                            app.CoverLetter,
                            app.Status,
                            app.AppliedAt
                        })
                    .OrderByDescending(a => a.AppliedAt)
                    .ToListAsync();

                return Ok(applications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching applications", error = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/applications/employer")]
        public async Task<IActionResult> GetAllEmployerApplications()
        {
            try
            {
                var userId = await this.GetAuthorizedUserIdAsync(_context);
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not logged in" });
                }

                // Get all jobs for this employer
                var jobIds = await _context.Jobs
                    .Where(j => j.EmployerId == userId.Value)
                    .Select(j => j.JobId)
                    .ToListAsync();

                // Get all applications for these jobs
                var applications = await _context.Applications
                    .Where(a => jobIds.Contains(a.JobId))
                    .Join(_context.Jobs,
                        app => app.JobId,
                        job => job.JobId,
                        (app, job) => new { app, job })
                    .Join(_context.Users,
                        combined => combined.app.SeekerId,
                        user => user.UserId,
                        (combined, user) => new
                        {
                            combined.app.ApplicationId,
                            combined.app.JobId,
                            JobTitle = combined.job.Title,
                            combined.app.SeekerId,
                            SeekerName = user.FirstName + " " + user.LastName,
                            SeekerEmail = user.Email,
                            SeekerPhone = user.PhoneNumber,
                            SeekerPhoto = user.SelfiePhotoPath,
                            combined.app.CoverLetter,
                            combined.app.Status,
                            combined.app.AppliedAt
                        })
                    .OrderByDescending(a => a.AppliedAt)
                    .ToListAsync();

                return Ok(applications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching applications", error = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/applications/seeker")]
        public async Task<IActionResult> GetSeekerApplications()
        {
            try
            {
                var userId = await this.GetAuthorizedUserIdAsync(_context);
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not logged in" });
                }

                var applications = await _context.Applications
                    .Where(a => a.SeekerId == userId.Value)
                    .Join(_context.Jobs,
                        app => app.JobId,
                        job => job.JobId,
                        (app, job) => new
                        {
                            app.ApplicationId,
                            app.JobId,
                            JobTitle = job.Title,
                            Company = job.Company,
                            JobLocation = job.Address,
                            EmployerId = job.EmployerId,
                            app.Status,
                            app.AppliedAt,
                            app.CoverLetter,
                            HasEmails = _context.EmailMessages.Any(e => e.ApplicationId == app.ApplicationId && e.ReceiverId == userId.Value)
                        })
                    .OrderByDescending(a => a.AppliedAt)
                    .ToListAsync();

                return Ok(applications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching applications", error = ex.Message });
            }
        }

        [HttpPut]
        [Route("api/applications/{applicationId}/status")]
        public async Task<IActionResult> UpdateApplicationStatus(int applicationId, [FromBody] StatusUpdateRequest request)
        {
            try
            {
                var userId = await this.GetAuthorizedUserIdAsync(_context);
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not logged in" });
                }

                // Verify the application belongs to a job owned by this employer
                var application = await _context.Applications
                    .Join(_context.Jobs,
                        app => app.JobId,
                        job => job.JobId,
                        (app, job) => new { app, job })
                    .Where(x => x.app.ApplicationId == applicationId && x.job.EmployerId == userId.Value)
                    .Select(x => x.app)
                    .FirstOrDefaultAsync();

                if (application == null)
                {
                    return NotFound(new { message = "Application not found or you don't have permission" });
                }

                // Only update if status is Pending or if explicitly changing to Accepted/Rejected
                if (application.Status == "Pending" || request.Status == "Accepted" || request.Status == "Rejected")
                {
                    application.Status = request.Status;
                    application.ReviewedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    // Send push notification to seeker
                    var seeker = await _context.Users.FindAsync(application.SeekerId);
                    var job = await _context.Jobs.FindAsync(application.JobId);
                    if (seeker != null && !string.IsNullOrEmpty(seeker.FcmToken) && job != null)
                    {
                        var title = "Application Status Updated";
                        var body = $"Your application for '{job.Title}' was {request.Status.ToLower()}.";
                        var data = new Dictionary<string, string>
                        {
                            { "type", "application_status" },
                            { "applicationId", application.ApplicationId.ToString() }
                        };
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await BagoScout.Services.PushNotificationHelper.SendNotificationAsync(seeker.FcmToken, title, body, data);
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error sending application status update FCM: {ex.Message}");
                            }
                        });
                    }
                }

                return Ok(new { message = "Application status updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating application status", error = ex.Message });
            }
        }
    }

    public class StatusUpdateRequest
    {
        public string Status { get; set; } = string.Empty;
    }
}
