
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace HealthTech.ApiService.Domain.Profiles;

[Table("profiles")]
public class Profile : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; }

    [Column("full_name")]
    public string? FullName { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
