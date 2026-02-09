using SQLite;

namespace AreYouOk.MobileApp.Data.Entities;

/// <summary>
/// Local database entity for user information
/// </summary>
[Table("Users")]
public class UserEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Unique]
    public int ServerId { get; set; }

    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public DateTime TokenExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastSyncedAt { get; set; }
    public bool IsActive { get; set; }
}
