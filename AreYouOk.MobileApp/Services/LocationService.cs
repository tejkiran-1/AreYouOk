namespace AreYouOk.MobileApp.Services;

/// <summary>
/// Service for getting device location
/// </summary>
public class LocationService
{
    /// <summary>
    /// Get current location
    /// </summary>
    public async Task<Location?> GetCurrentLocationAsync()
    {
        try
        {
            var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
            var location = await Geolocation.GetLocationAsync(request);
            return location;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting location: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Check if location permission is granted
    /// </summary>
    public async Task<bool> CheckAndRequestLocationPermissionAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            return status == PermissionStatus.Granted;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error checking location permission: {ex.Message}");
            return false;
        }
    }
}
