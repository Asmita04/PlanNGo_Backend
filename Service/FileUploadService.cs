using Microsoft.AspNetCore.Mvc;

namespace PlanNGo_Backend.Service
{
    public class FileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB

        public FileUploadService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string?> UploadFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                return null;

            // Validate file size
            if (file.Length > _maxFileSize)
                throw new InvalidOperationException("File size exceeds maximum limit of 5MB");

            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                throw new InvalidOperationException("Invalid file type. Only image files are allowed.");

            // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", folder);
            Directory.CreateDirectory(uploadsPath);

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path for database storage
            return $"/uploads/{folder}/{fileName}";
        }

        public bool DeleteFile(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, filePath.TrimStart('/'));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }
            }
            catch
            {
                // Log error if needed
            }

            return false;
        }

        public string GetFileUrl(string? filePath, HttpRequest request)
        {
            if (string.IsNullOrEmpty(filePath))
                return string.Empty;

            var baseUrl = $"{request.Scheme}://{request.Host}";
            return $"{baseUrl}{filePath}";
        }
    }
}