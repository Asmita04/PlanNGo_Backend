using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlanNGo_Backend.Service;

namespace PlanNGo_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UploadController : ControllerBase
    {
        private readonly FileUploadService _fileUploadService;

        public UploadController(FileUploadService fileUploadService)
        {
            _fileUploadService = fileUploadService;
        }

        // POST: api/Upload/event-image
        [HttpPost("event-image")]
        [Authorize(Roles = "organizer")]
        public async Task<IActionResult> UploadEventImage(IFormFile file)
        {
            try
            {
                var filePath = await _fileUploadService.UploadFileAsync(file, "events");
                if (filePath == null)
                {
                    return BadRequest("No file uploaded");
                }

                var fileUrl = _fileUploadService.GetFileUrl(filePath, Request);
                return Ok(new { filePath, fileUrl });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while uploading the file");
            }
        }

        // POST: api/Upload/profile-image
        [HttpPost("profile-image")]
        public async Task<IActionResult> UploadProfileImage(IFormFile file)
        {
            try
            {
                var filePath = await _fileUploadService.UploadFileAsync(file, "profiles");
                if (filePath == null)
                {
                    return BadRequest("No file uploaded");
                }

                var fileUrl = _fileUploadService.GetFileUrl(filePath, Request);
                return Ok(new { filePath, fileUrl });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while uploading the file");
            }
        }

        // DELETE: api/Upload
        [HttpDelete]
        public IActionResult DeleteFile([FromQuery] string filePath)
        {
            try
            {
                var deleted = _fileUploadService.DeleteFile(filePath);
                if (deleted)
                {
                    return Ok(new { message = "File deleted successfully" });
                }
                return NotFound("File not found");
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while deleting the file");
            }
        }
    }
}