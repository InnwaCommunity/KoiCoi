using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoiCoi.Models.ForgetPassword;

public class OtpPayload
{
    public int Otpid { get; set; }

    public string EmailPhone { get; set; } = null!;

    public int UserId { get; set; }

    public string Passcode { get; set; } = null!;

    public string Ipaddress { get; set; } = null!;

    public string Otptoken { get; set; } = null!;

    public DateTime SendDateTime { get; set; }

    public int FailCount { get; set; }

    public int RetryCount { get; set; }

    public DateTime? LastModifiedDate { get; set; }

    public DateTime? CreatedDate { get; set; }
}
