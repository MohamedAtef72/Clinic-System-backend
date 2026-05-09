using Clinic_System.Application.DTO;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.Interfaces
{
    public interface IPhotoService
    {
        Task<CloudinarySignatureDto> GetUploadSignatureAsync(string folder = "clinic_app_images");
    }

}

