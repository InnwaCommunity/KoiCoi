using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoiCoi.Models.User_Dto;

public partial class UploadUserProfileReqeust
{
    public string? base64data { get; set; }
    public string? description { get; set; }
}
