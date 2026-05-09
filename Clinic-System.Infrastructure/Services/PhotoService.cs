using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Constant;
using System.Security.Cryptography;
using System.Text;
using Clinic_System.Application.DTO;

namespace Clinic_System.Infrastructure.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<PhotoService> _logger;
        private readonly string _apiSecret;

        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

        public PhotoService(
            IOptions<CloudinarySettings> config,
            ILogger<PhotoService> logger)
        {
            var account = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(account);
            _logger = logger;
            _apiSecret = config.Value.ApiSecret;
        }

        public async Task<CloudinarySignatureDto> GetUploadSignatureAsync(
            string folder = "clinic_app_images")
        {
            _logger.LogInformation(
                "[PHOTO_SERVICE] Generating Cloudinary upload signature");

            try
            {
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                var signingString =
                    $"folder={folder}&timestamp={timestamp}";

                using var sha1 = SHA1.Create();

                var hash = sha1.ComputeHash(
                    Encoding.UTF8.GetBytes(signingString + _apiSecret)
                );

                var signature = BitConverter
                    .ToString(hash)
                    .Replace("-", "")
                    .ToLower();

                return new CloudinarySignatureDto
                {
                    Signature = signature,
                    Timestamp = timestamp,
                    ApiKey = _cloudinary.Api.Account.ApiKey,
                    CloudName = _cloudinary.Api.Account.Cloud,
                    Folder = folder
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[PHOTO_SERVICE] Error generating upload signature");

                throw;
            }
        }
    }
}