namespace AreYouOk.MobileApp.Services;

/// <summary>
/// Service for storing and retrieving settings locally
/// </summary>
public class SettingsService
{
    private const string AUTH_TOKEN_KEY = "auth_token";
    private const string USER_ID_KEY = "user_id";
    private const string USER_EMAIL_KEY = "user_email";
    private const string USER_FIRST_NAME_KEY = "user_first_name";
    private const string USER_LAST_NAME_KEY = "user_last_NAME";
    private const string API_BASE_URL_KEY = "api_base_url";

    /// <summary>
    /// Default API base URL - update this to match your API server
    /// For Android Emulator, use 10.0.2.2 instead of localhost
    /// </summary>
    private const string DEFAULT_API_BASE_URL = "http://10.0.2.2:5053";

    public string? AuthToken
    {
        get
        {
            var token = Preferences.Get(AUTH_TOKEN_KEY, null);
            System.Diagnostics.Debug.WriteLine($"[SettingsService] GET AuthToken: {(string.IsNullOrEmpty(token) ? "NULL" : "EXISTS")}");
            return token;
        }
        set
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsService] SET AuthToken: {(value == null ? "NULL" : "VALUE")}");
            if (value == null)
                Preferences.Remove(AUTH_TOKEN_KEY);
            else
                Preferences.Set(AUTH_TOKEN_KEY, value);
        }
    }

    public int? UserId
    {
        get
        {
            var value = Preferences.Get(USER_ID_KEY, -1);
            int? result = value == -1 ? null : value;
            System.Diagnostics.Debug.WriteLine($"[SettingsService] GET UserId: {result?.ToString() ?? "NULL"}");
            return result;
        }
        set
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsService] SET UserId: {value?.ToString() ?? "NULL"}");
            if (value == null)
                Preferences.Remove(USER_ID_KEY);
            else
                Preferences.Set(USER_ID_KEY, value.Value);
        }
    }

    public string? UserEmail
    {
        get => Preferences.Get(USER_EMAIL_KEY, null);
        set
        {
            if (value == null)
                Preferences.Remove(USER_EMAIL_KEY);
            else
                Preferences.Set(USER_EMAIL_KEY, value);
        }
    }

    public string? UserFirstName
    {
        get => Preferences.Get(USER_FIRST_NAME_KEY, null);
        set
        {
            if (value == null)
                Preferences.Remove(USER_FIRST_NAME_KEY);
            else
                Preferences.Set(USER_FIRST_NAME_KEY, value);
        }
    }

    public string? UserLastName
    {
        get => Preferences.Get(USER_LAST_NAME_KEY, null);
        set
        {
            if (value == null)
                Preferences.Remove(USER_LAST_NAME_KEY);
            else
                Preferences.Set(USER_LAST_NAME_KEY, value);
        }
    }

    public string ApiBaseUrl
    {
        get => Preferences.Get(API_BASE_URL_KEY, DEFAULT_API_BASE_URL);
        set => Preferences.Set(API_BASE_URL_KEY, value);
    }

    public bool IsLoggedIn => !string.IsNullOrEmpty(AuthToken) && UserId.HasValue;

    public string UserFullName => $"{UserFirstName} {UserLastName}".Trim();

    /// <summary>
    /// Clear all user data (logout)
    /// </summary>
    public void ClearUserData()
    {
        AuthToken = null;
        UserId = null;
        UserEmail = null;
        UserFirstName = null;
        UserLastName = null;
    }
}
