# Developer Guide - AreYouOk Application

## Project Overview

AreYouOk is a comprehensive personal security application that allows users to share their safety status with family members during journeys. The application consists of three main components:

1. **Backend API** (.NET 10 Web API)
2. **Mobile App** (.NET MAUI)
3. **Shared Library** (DTOs and common models)

## Architecture

### Backend Architecture

```
┌─────────────────────────────────────────┐
│         API Controllers                  │
│  (AuthController, JourneyController,    │
│   SafetyStatusController)               │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│         Business Services                │
│  (UserService, JourneyService,          │
│   SafetyStatusService, Notification)    │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│      Data Access Layer                   │
│  (EF Core DbContext, Repositories)      │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│         SQL Server Database              │
└──────────────────────────────────────────┘

Background Service: JourneyMonitoringService (runs every minute)
```

### Mobile Architecture (MVVM Pattern)

```
┌─────────────────────────────────────────┐
│            XAML Views                    │
│  (LoginPage, HomePage, JourneyPage)     │
└──────────────┬──────────────────────────┘
               │ Data Binding
┌──────────────▼──────────────────────────┐
│           ViewModels                     │
│  (LoginViewModel, HomeViewModel, etc)   │
│  [CommunityToolkit.Mvvm]                │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│           Services                       │
│  (ApiClient, LocationService,           │
│   SettingsService)                      │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│         Backend API                      │
└──────────────────────────────────────────┘
```

## Key Components

### Backend API

#### 1. Models (Domain Entities)

**User.cs**
- Represents registered users
- Properties: Id, FirstName, LastName, Email, PhoneNumber, PasswordHash
- Relationships: One-to-Many with Journeys

**Journey.cs**
- Represents a user's safety journey
- Properties: UserId, StartTime, EndTime, CheckInIntervalHours, Status, Location
- Relationships: Belongs to User, Has Many FamilyMembers and SafetyStatuses
- Status Enum: Active, Completed, Alert, Cancelled

**FamilyMember.cs**
- Links family members to journeys
- Can reference a registered User or just store a phone number
- Properties: JourneyId, PhoneNumber, UserId (nullable), DeviceToken

**SafetyStatus.cs**
- Records each safety check-in
- Properties: JourneyId, IsSafe, Timestamp, Location, Message, UpdateType
- UpdateType Enum: ManualSafe, ManualAlert, AutomaticAlert

#### 2. Services

**UserService**
- Registration, authentication, profile management
- Password hashing with BCrypt
- JWT token generation

**JourneyService**
- Start/end journeys
- Location updates
- Journey history
- Family member management

**SafetyStatusService**
- Create safety check-ins
- Retrieve status history
- Trigger notifications

**NotificationService**
- Send notifications to family members
- Currently logs notifications (extend for FCM, SMS, Email)
- Alert sound notifications

**TokenService**
- JWT token generation
- Token validation configuration

#### 3. Background Services

**JourneyMonitoringService**
- Hosted service that runs continuously
- Checks every minute for overdue journeys
- Automatically creates alert status
- Notifies family members

#### 4. API Endpoints

**Authentication** (`/api/Auth`)
- POST `/register` - Create new user account
- POST `/login` - Authenticate user
- GET `/profile` - Get current user profile [Authorized]
- PUT `/profile` - Update user profile [Authorized]

**Journey Management** (`/api/Journey`)
- POST `/start` - Start new journey [Authorized]
- GET `/active` - Get active journey [Authorized]
- GET `/{id}` - Get journey by ID [Authorized]
- POST `/end` - End active journey [Authorized]
- PUT `/location` - Update current location [Authorized]
- GET `/history` - Get journey history [Authorized]
- GET `/monitoring` - Get journeys user is monitoring [Authorized]

**Safety Status** (`/api/SafetyStatus`)
- POST `/` - Update safety status (check-in) [Authorized]
- GET `/journey/{journeyId}` - Get safety history for journey [Authorized]

### Mobile App

#### 1. Services

**SettingsService**
- Stores user preferences and auth tokens
- Uses MAUI Preferences API
- Properties: AuthToken, UserId, UserEmail, ApiBaseUrl

**ApiClient**
- HTTP client wrapper for API calls
- Automatic token injection
- JSON serialization/deserialization
- Error handling

**LocationService**
- GPS location access
- Permission handling
- Uses MAUI Geolocation API

#### 2. ViewModels (MVVM Pattern)

Uses **CommunityToolkit.Mvvm** for:
- `[ObservableProperty]` - Auto-generate property changed events
- `[RelayCommand]` - Auto-generate command implementations
- `ObservableObject` base class

Example:
```csharp
public partial class LoginViewModel : ObservableObject
{
    [ObservableProperty]
    private string email = string.Empty;

    [RelayCommand]
    private async Task LoginAsync()
    {
        // Login logic
    }
}
```

#### 3. Pages (XAML)

- **LoginPage**: User authentication
- **RegisterPage**: New user registration
- **HomePage**: Dashboard with active journey status
- **JourneyPage**: Start/manage journeys
- **StatusPage**: Quick safety check-in buttons
- **HistoryPage**: Past journeys
- **MonitorPage**: Journeys user is monitoring

### Shared Library

**DTOs (Data Transfer Objects)**
- UserDtos: RegisterDto, LoginDto, AuthResponseDto, UserProfileDto
- JourneyDtos: StartJourneyDto, JourneyDto, EndJourneyDto, UpdateLocationDto
- SafetyStatusDtos: UpdateSafetyStatusDto, SafetyStatusDto, AlertNotificationDto
- FamilyMemberDtos: FamilyMemberDto, AddFamilyMembersDto
- ApiResponse<T>: Generic response wrapper

## Database Design

### Relationships

```
Users (1) ──┬─→ (N) Journeys
            │
            └─→ (N) FamilyMembers (as registered users)

Journeys (1) ─┬─→ (N) FamilyMembers
              │
              └─→ (N) SafetyStatuses
```

### Indexes

Optimized for common queries:
- Users: Email (unique), PhoneNumber (unique)
- Journeys: UserId, Status, (UserId, Status) composite
- FamilyMembers: JourneyId, PhoneNumber, UserId, (JourneyId, PhoneNumber) composite
- SafetyStatuses: JourneyId, Timestamp, (JourneyId, Timestamp) composite

## Security

### Authentication Flow

1. User registers → Password hashed with BCrypt → Stored in database
2. User logs in → Password verified → JWT token generated
3. Token contains: UserId, Email, Expiry (720 hours default)
4. Client stores token → Sends in Authorization header
5. API validates token on protected endpoints

### JWT Configuration

```json
{
  "JwtSettings": {
    "SecretKey": "32+ character secret key",
    "Issuer": "AreYouOkAPI",
    "Audience": "AreYouOkApp",
    "ExpiryHours": "720"
  }
}
```

### CORS Configuration

Development: AllowAll
Production: Configure specific origins

## Development Workflow

### Adding a New Feature

#### Backend:

1. **Create/Update Model** in `Models/`
2. **Update DbContext** if new entity
3. **Create Migration**: `dotnet ef migrations add FeatureName`
4. **Update Database**: `dotnet ef database update`
5. **Create DTO** in Shared project
6. **Implement Service** in `Services/`
7. **Create Controller** in `Controllers/`
8. **Test with Swagger**

#### Mobile:

1. **Create ViewModel** in `ViewModels/`
2. **Create XAML Page** in `Pages/`
3. **Create Code-behind** (Page.xaml.cs)
4. **Register in MauiProgram.cs**
5. **Add Navigation** in AppShell
6. **Test on Emulator/Device**

### Code Style Guidelines

#### Backend
- Use async/await for all I/O operations
- Return `ApiResponse<T>` from services
- Use `[Authorize]` attribute for protected endpoints
- Add XML comments to public methods
- Follow Repository pattern for data access

#### Mobile
- Use MVVM pattern strictly
- All business logic in ViewModels
- Views only for UI markup
- Use `CommunityToolkit.Mvvm` attributes
- Implement IDisposable for cleanup

## Testing

### Unit Testing (Backend)

Create test project:
```bash
dotnet new xunit -n AreYouOk.API.Tests
dotnet add reference ../AreYouOk.API/AreYouOk.API.csproj
```

Example test:
```csharp
[Fact]
public async Task RegisterAsync_ValidData_ReturnsSuccess()
{
    // Arrange
    var dto = new RegisterDto { /* ... */ };
    var service = new UserService(/* dependencies */);
    
    // Act
    var result = await service.RegisterAsync(dto);
    
    // Assert
    Assert.True(result.Success);
}
```

### Integration Testing

Test complete API flows:
```csharp
var client = _factory.CreateClient();
var response = await client.PostAsJsonAsync("/api/Auth/register", dto);
Assert.Equal(HttpStatusCode.OK, response.StatusCode);
```

### Mobile Testing

- Use ViewModels for unit testing
- Mock ApiClient and Services
- Test business logic without UI

## Deployment

### Backend API Deployment

#### Azure App Service:
```bash
# Publish
dotnet publish -c Release -o ./publish

# Deploy (using Azure CLI)
az webapp deployment source config-zip \
  --resource-group MyResourceGroup \
  --name MyAppService \
  --src ./publish.zip
```

#### Docker:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0
COPY publish/ App/
WORKDIR /App
ENTRYPOINT ["dotnet", "AreYouOk.API.dll"]
```

### Mobile App Deployment

#### Android:
```bash
dotnet publish -f net6.0-android -c Release -p:AndroidPackageFormat=apk
```

#### iOS (Mac only):
```bash
dotnet publish -f net6.0-ios -c Release
```

## Performance Optimization

### Backend

1. **Database Indexing**: All foreign keys and frequently queried fields
2. **Caching**: Implement Redis for frequently accessed data
3. **Async Operations**: All I/O operations are async
4. **Connection Pooling**: EF Core handles automatically
5. **Query Optimization**: Use `.AsNoTracking()` for read-only queries

### Mobile

1. **Image Caching**: Use FFImageLoading
2. **Lazy Loading**: Load data on demand
3. **Background Tasks**: Use WorkManager for periodic tasks
4. **Local Database**: Cache data with SQLite
5. **Compression**: Compress API responses

## Troubleshooting

### Common Issues

**"Cannot connect to database"**
- Check SQL Server is running
- Verify connection string
- Check firewall settings

**"Unauthorized"** 
- Token expired (check ExpiryHours)
- Token not sent in header
- Invalid secret key configuration

**"Mobile app can't connect to API"**
- Check API is running
- Verify API URL in SettingsService
- Android emulator: use 10.0.2.2 instead of localhost
- Physical device: use computer's IP address

**"Background service not running"**
- Check logs for errors
- Verify service is registered in Program.cs
- Check database connection in background service

## Future Enhancements

### High Priority
- [ ] Push notifications (FCM)
- [ ] SMS alerts (Twilio)
- [ ] Email notifications
- [ ] Maps integration
- [ ] Offline mode

### Medium Priority
- [ ] Geofencing
- [ ] Route tracking
- [ ] Emergency contacts
- [ ] SOS panic button
- [ ] Voice commands

### Low Priority
- [ ] Social sharing
- [ ] Statistics dashboard
- [ ] Multiple languages
- [ ] Themes customization
- [ ] Apple Watch app

## Resources

- [.NET MAUI Documentation](https://docs.microsoft.com/dotnet/maui/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [JWT Authentication](https://jwt.io/)
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/)
- [MAUI Community Toolkit](https://learn.microsoft.com/dotnet/communitytoolkit/maui/)

## Support

For issues, questions, or contributions:
1. Check existing documentation
2. Review code comments
3. Create GitHub issue
4. Contact development team

---

**Happy Coding! 🚀**
