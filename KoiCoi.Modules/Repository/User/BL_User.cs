using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoiCoi.Modules.Repository.User;

public class BL_User
{
    private readonly DA_User _daUser;

    public BL_User(DA_User daUser)
    {
        _daUser = daUser;
    }
    public async Task<dynamic> GetStatusType()
    {
        return await _daUser.GetStatusType();
    }
}
