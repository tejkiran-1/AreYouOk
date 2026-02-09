using System.Diagnostics;

namespace AreYouOk.MobileApp.Services;

/// <summary>
/// Enhanced service for storing and retrieving settings with secure preferences
/// </summary>
public class SettingsService
{
    private const string AUTH_TOKEN_KEY = "auth_token";
    private const string USER_ID_KEY = "user_id";
    private const string USER_EMAIL_KEY = "user_email";
    private const string USER_FIRST_NAME_KEY = "user_first_name";
    private const string USER_LAST_NAME_KEY = "user_last_name";
    private const string USER_PHONE_KEY = "user_phone";
    private const string API_BASE_URL_KEY = "api_base_url";
    private const string TOKEN_EXPIRES_AT_KEY = "token_expires_at";
    private const string LAST_SYNC_TIME_KEY = "last_sync_time";
    private const string IS_FIRST_LAUNCH_KEY = "is_first_launch";

    /// <summary>
    /// Default API base URL - update based on your environment
    /// For Android Emulator: http://10.0.2.2:5053
    /// For Physical Device: http://YOUR_COMPUTER_IP:5053
    /// </summary>
    private const string DEFAULT_API_BASE_URL = "http://192.168.2.106:5053";

    public string? AuthToken
    {
        get
        {
            try
            {
                var token = Preferences.Get(AUTH_TOKEN_KEY, null);
                Debug.WriteLine($"[SettingsService] GET AuthToken: {(string.IsNullOrEmpty(token) ? "NULL" : "EXISTS")}");
                return token;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SettingsService] Error getting AuthToken: {ex.Message}");
                return null;
            }
        }
        set
        {
            try
            {
                Debug.WriteLine($"[SettingsService] SET AuthToken: {(value == null ? "NULL" : "VALUE")}");
                if (value == null)
                    Preferences.Remove(AUTH_TOKEN_KEY);
                else
                    Preferences.Set(AUTH_TOKEN_KEY, value);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SettingsService] Error setting AuthToken: {ex.Message}");
            }
        }
    }

    public int? UserId
    {
        get
        {
            try
            {
                var value = Preferences.Get(USER_ID_KEY, -1);
                int? result = value == -1 ? null : value;
                Debug.WriteLine($"[SettingsService] GET UserId: {result?.ToString() ?? "NULL"}");
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SettingsService] Error getting UserId: {ex.Message}");
                return null;
            }
        }
        set
        {
            try
            {
                Debug.WriteLine($"[SettingsService] SET UserId: {value?.ToString() ?? "NULL"}");
                if (value == null)
                    Preferences.Remove(USER_ID_KEY);
                else
                    Preferences.Set(USER_ID_KEY, value.Value);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SettingsService] Error setting UserId: {ex.Message}");
            }
        }
    }

    public string? UserEmail
    {
        get
        {
            try
            {
                return Preferences.Get(USER_EMAIL_KEY, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SettingsService] Error getting UserEmail: {ex.Message}");
                return null;
            }
        }
        set
        {
            try
            {
                if (value == null)
                    Preferences.Remove(USER_EMAIL_KEY);
                else
                    Preferences.Set(USER_EMAIL_KEY, value);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SettingsService] Error setting UserEmail: {ex.Message}");
            }
        }
    }

    public string? UserFirstName
    {
        get
        {
            try
            {
                return Preferences.Get(USER_FIRST_NAME_KEY, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SettingsService] Error getting UserFirstName: {ex.Message}");
                return null;
            }
        }
        set
        {
            try
            {
                if (value == null)
                    Preferences.Remove(USER_FIRST_NAME_KEY);
                else
                    Preferences.Set(USER_FIRST_NAME_KEY, value);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SettingsService] Error setting UserFirstName: {ex.Message}");
            }
        }
    }

    public string? UserLastName
    {
        get
        {
            try
            {
                return Preferences.Get(USER_LAST_NAME_KEY, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SettingsService] Error getting UserLastName: {ex.Message}");
                return null;
            }
        }
        set
        {
            try
            {
                if (value == null)
                    Preferences.Remove(USER_LAST_NAME_KEY);
                else
                    Preferences.Set(USER_LAST_NAME_KEY, value);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SettingsService] Error setting UserLastName: {ex.Message}");
            }
        }
    }

    public string? UserPhoneNumber
    {
        get
        {
            try
            {
                return Preferences.Get(USER_PHONE_KEY, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SettingsService] Error getting UserPhoneNumber: {ex.Message}");
                return null;
            }
        }
        set
        {
            try
            {
                if (value == null)
                    Preferences.Remove(USER_PHONE_KEY);
                else
                    Preferences.Set(USER_PHONE_KEY, value);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SettingsService] Error setting UserPhoneNumber: {ex.Message}");
            }
        }
    }

    public string ApiBaseUrl
    {
        get
        {
            try
            {
                return Preferences.Get(API_BASE_URL_KEY, DEFAULT_API_BASE_URL);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SettingsService] Error getting ApiBaseUrl: {ex.Message}");
                return DEFAULT_API_BASE_URL;
            }
        }
        set
        {
            try
            {
                Preferences.Set(API_BASE_URL_KEY, value);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SettingsService] Error setting ApiBaseUrl: {ex.Message}");
            }
        }
    }

    public DateTime? TokenExpiresAt
    {
        get
        {
            try
            {
                var ticks = Preferences.Get(TOKEN_EXPIRES_AT_KEY, -1L);
                return ticks == -1L ? null : new DateTime(ticks, DateTimeKind.Utc);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SettingsService] Error getting TokenExpiresAt: {ex.Message}");
                return null;
            }
        }
        set
        {
            try
            {
                if (value == null)
                    Preferences.Remove(TOKEN_EXPIRES_AT_KEY);
                else
                    Preferences.Set(TOKEN_EXPIRES_AT_KEY, value.Value.Ticks);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SettingsService] Error setting TokenExpiresAt: {ex.Message}");
            }
        }
    }

    public DateTime? LastSyncTime
    {
        get
        {
            try
            {
                var ticks = Preferences.Get(LAST_SYNC_TIME_KEY, -1L);
                return ticks == -1L ? null : new DateTime(ticks, DateTimeKind.Utc);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SettingsService] Error getting LastSyncTime: {ex.Message}");
                return null;
            }
        }
        set
        {
            try
            {
                if (value == null)
                    Preferences.Remove(LAST_SYNC_TIME_KEY);
                else
                    Preferences.Set(LAST_SYNC_TIME_KEY, value.Value.Ticks);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SettingsService] Error setting LastSyncTime: {ex.Message}");
            }
        }
    }

    public bool IsFirstLaunch
    {
        get
        {
            try
            {
                return Preferences.Get(IS_FIRST_LAUNCH_KEY, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SettingsService] Error getting IsFirstLaunch: {ex.Message}");
                return true;
            }
        }
        set
        {
            try
            {
                Preferences.Set(IS_FIRST_LAUNCH_KEY, value);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SettingsService] Error setting IsFirstLaunch: {ex.Message}");
            }
        }
    }

    public bool IsLoggedIn
    {
        get
        {
            try
            {
                var hasToken = !string.IsNullOrEmpty(AuthToken);
                var hasUserId = UserId.HasValue;
                var tokenNotExpired = TokenExpiresAt == null || TokenExpiresAt > DateTime.UtcNow;
                
                var result = hasToken && hasUserId && tokenNotExpired;
                Debug.WriteLine($"[SettingsService] IsLoggedIn: {result} (Token: {hasToken}, UserId: {hasUserId}, TokenValid: {tokenNotExpired})");
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SettingsService] Error checking IsLoggedIn: {ex.Message}");
                return false;
            }
        }
    }

    public string UserFullName
    {
        get
        {
            try
            {
                return $"{UserFirstName} {UserLastName}".Trim();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SettingsService] Error getting UserFullName: {ex.Message}");
                return "User";
            }
        }
    }

    public void SaveUserSession(int userId, string email, string firstName, string lastName, string token, DateTime? expiresAt = null)
    {
        try
        {
            Debug.WriteLine($"[SettingsService] Saving user session for {email}");
            UserId = userId;
            UserEmail = email;
            UserFirstName = firstName;
            UserLastName = lastName;
            AuthToken = token;
            TokenExpiresAt = expiresAt;
            IsFirstLaunch = false;
            Debug.WriteLine("[SettingsService] User session saved successfully");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SettingsService] Error saving user session: {ex.Message}");
            throw;
        }
    }

    public void ClearUserData()
    {
        try
        {
            Debug.WriteLine("[SettingsService] Clearing user data");
            AuthToken = null;
            UserId = null;
            UserEmail = null;
            UserFirstName = null;
            UserLastName = null;
            UserPhoneNumber = null;
            TokenExpiresAt = null;
            LastSyncTime = null;
            Debug.WriteLine("[SettingsService] User data cleared successfully");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SettingsService] Error clearing user data: {ex.Message}");
        }
    }

    public bool IsTokenExpiringSoon(int minutesThreshold = 30)
    {
        try
        {
            if (TokenExpiresAt == null)
                return false;

            return (TokenExpiresAt.Value - DateTime.UtcNow).TotalMinutes < minutesThreshold;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SettingsService] Error checking IsTokenExpiringSoon: {ex.Message}");
            return false;
        }
    }
}
