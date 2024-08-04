
using Microsoft.Extensions.Configuration;

namespace KoiCoi.Modules.Repository.PostFeature;

public class DA_Post
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;

    public DA_Post(AppDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }
}
