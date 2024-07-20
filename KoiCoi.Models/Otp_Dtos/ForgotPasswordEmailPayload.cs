using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoiCoi.Models.Otp_Dtos;

public class ForgotPasswordEmailPayload
{
    public string Email { get; set; } = string.Empty;
}
