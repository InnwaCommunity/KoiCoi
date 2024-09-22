
namespace KoiCoi.Models;

public partial class FindByNamePayload
{
    public string? Name { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
