
namespace KoiCoi.Database.AppDbContextModels;

public partial class PostImage
{
    public int ImageId { get; set; }

    public string Url { get; set; } = null!;

    public string? Description { get; set; }

    public int PostId { get; set; }

    public DateTime? CreatedDate { get; set; }
}
