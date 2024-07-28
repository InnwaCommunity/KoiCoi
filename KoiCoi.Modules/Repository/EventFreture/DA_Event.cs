using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoiCoi.Modules.Repository.EventFreture;

public class DA_Event
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;

    public DA_Event(AppDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }
}
