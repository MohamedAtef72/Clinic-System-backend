using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Domain.Constant
{
    public class JwtSetting
    {
        public string SecritKey { get; set; } = null!;
        public string AudienceIP { get; set; } = null!;
        public string IssuerIP { get; set; } = null!;
        public int AccessTokenExpirationMinutes { get; set; }
        public int RefreshTokenExpirationDays { get; set; }
    }
}
