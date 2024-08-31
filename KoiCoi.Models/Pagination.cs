namespace KoiCoi.Models;

public partial class Pagination
{
    public int PageSize { get; set; }
    public int PageNumber { get; set; }
    public int PageCount { get; set; }
    public bool IsEndOfPages => PageNumber >= PageCount;
    public dynamic Data { get; set; }
}
