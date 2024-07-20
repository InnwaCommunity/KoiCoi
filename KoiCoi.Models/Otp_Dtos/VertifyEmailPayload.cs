using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoiCoi.Models.Otp_Dtos;

public partial class VertifyEmailPayload
{
    public int LoginId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string OTPPasscode { get; set; } = string.Empty;
    public string OTPPrefix { get; set; } = string.Empty;
}
