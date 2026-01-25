namespace AreYouOk.Shared.DTOs;

/// <summary>
/// Generic API response wrapper
/// </summary>
/// <typeparam name="T">Type of data in the response</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Whether the API call was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Response data
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Error message if the call failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Validation errors (field-specific)
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; set; }

    /// <summary>
    /// Creates a successful response
    /// </summary>
    public static ApiResponse<T> SuccessResponse(T data)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data
        };
    }

    /// <summary>
    /// Creates an error response
    /// </summary>
    public static ApiResponse<T> ErrorResponse(string errorMessage)
    {
        return new ApiResponse<T>
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }

    /// <summary>
    /// Creates a validation error response
    /// </summary>
    public static ApiResponse<T> ValidationErrorResponse(Dictionary<string, string[]> errors)
    {
        return new ApiResponse<T>
        {
            Success = false,
            ErrorMessage = "Validation failed",
            Errors = errors
        };
    }
}

/// <summary>
/// API response without data
/// </summary>
public class ApiResponse : ApiResponse<object>
{
}
