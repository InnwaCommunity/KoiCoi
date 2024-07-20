using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoiCoi.Models.Via;

public partial class ViaUser
{
    public int UserId { get; set; }

    public string UserIdval { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Email { get; set; }

    public string Password { get; set; } = null!;

    public string? Phone { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string? DeviceId { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime ModifiedDate { get; set; }

    public bool? Inactive { get; set; }
}
