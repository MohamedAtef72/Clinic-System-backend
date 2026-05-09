using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.DTO
{
    public class CloudinarySignatureDto
    {
        public string Signature { get; set; }
        public long Timestamp { get; set; }
        public string ApiKey { get; set; }
        public string CloudName { get; set; }
        public string Folder { get; set; }
    }
}
