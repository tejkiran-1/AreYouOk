using System.ComponentModel.DataAnnotations;

namespace AreYouOk.Shared.DTOs;

/// <summary>
/// DTO for family member information
/// </summary>
public class FamilyMemberDto
{
    public int Id { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string? DisplayName { get; set; }
    public bool ReceiveNotifications { get; set; }
    public DateTime AddedAt { get; set; }
    
    /// <summary>
    /// Whether this family member is a registered user
    /// </summary>
    public bool IsRegisteredUser => UserId.HasValue;
}

/// <summary>
/// DTO for adding family members to a journey
/// </summary>
public class AddFamilyMembersDto
{
    [Required(ErrorMessage = "At least one family member phone number is required")]
    [MinLength(1, ErrorMessage = "At least one family member phone number is required")]
    public List<string> PhoneNumbers { get; set; } = new();
}

/// <summary>
/// DTO for removing a family member
/// </summary>
public class RemoveFamilyMemberDto
{
    [Required(ErrorMessage = "Family member ID is required")]
    public int FamilyMemberId { get; set; }
}
