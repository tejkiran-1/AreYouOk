namespace AreYouOk.Shared.Enums;

/// <summary>
/// Enum representing the status of a journey
/// </summary>
public enum JourneyStatus
{
    /// <summary>
    /// Journey is currently active and ongoing
    /// </summary>
    Active = 1,

    /// <summary>
    /// Journey has been completed successfully
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Journey has been cancelled by the user
    /// </summary>
    Cancelled = 3,

    /// <summary>
    /// Journey requires attention (missed check-in)
    /// </summary>
    Alert = 4
}
