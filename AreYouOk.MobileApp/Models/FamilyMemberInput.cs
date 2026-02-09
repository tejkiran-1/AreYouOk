namespace AreYouOk.MobileApp.Models;

/// <summary>
/// Represents a family member input for adding to a journey
/// </summary>
public class FamilyMemberInput
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}
