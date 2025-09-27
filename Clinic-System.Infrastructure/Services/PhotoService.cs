using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Constant;
using Microsoft.AspNetCore.Http;

namespace Clinic_System.Infrastructure.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly Cloudinary _cloudinary;

        public PhotoService(IOptions<CloudinarySettings> config)
        {
            var acc = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(acc);
        }

        public async Task<string> UploadImageAsync(IFormFile? file)
        {
            if (file != null)
            {
                if (file.Length <= 0)
                    return null;

                using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "clinic_app_images"
                };

                var result = await _cloudinary.UploadAsync(uploadParams);
                return result.SecureUrl.ToString();
            }
            else
            {
                return null;
            }
        }
    }
}
