# Quick Start Guide - AreYouOk Application

## Step 1: Setup Database

### Option A: Using Entity Framework Migrations (Recommended)

1. Open terminal in the `AreYouOk.API` directory
2. Run the following commands:

```bash
# Install EF Core tools globally (if not already installed)
dotnet tool install --global dotnet-ef

# Create initial migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update
```

This will create the database schema automatically.

### Option B: Manual SQL Script

If you prefer to create the database manually, see `DatabaseSchema.sql` for the complete script.

## Step 2: Configure API

1. Open `AreYouOk.API/appsettings.json`
2. Update the connection string if needed:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AreYouOkDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

For SQL Server:
```json
"DefaultConnection": "Server=localhost;Database=AreYouOkDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True"
```

3. **IMPORTANT**: Change the JWT secret key in production:
```json
{
  "JwtSettings": {
    "SecretKey": "CHANGE_THIS_TO_A_SECURE_RANDOM_STRING_AT_LEAST_32_CHARACTERS"
  }
}
```

## Step 3: Run the Backend API

```bash
cd AreYouOk.API
dotnet restore
dotnet build
dotnet run
```

The API will start at:
- HTTPS: `https://localhost:7001`
- HTTP: `http://localhost:5001`
- Swagger UI: `https://localhost:7001` (open in browser)

## Step 4: Test the API

Open `https://localhost:7001` in your browser to access Swagger UI.

### Test Registration:
```json
POST /api/Auth/register
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@example.com",
  "phoneNumber": "+1234567890",
  "password": "Password123",
  "confirmPassword": "Password123"
}
```

### Test Login:
```json
POST /api/Auth/login
{
  "email": "john@example.com",
  "password": "Password123"
}
```

Copy the `token` from the response.

### Test Start Journey (requires authentication):
1. Click "Authorize" button in Swagger
2. Enter: `Bearer YOUR_TOKEN_HERE`
3. Try the endpoint:

```json
POST /api/Journey/start
{
  "checkInIntervalHours": 10,
  "familyMemberPhones": ["+9876543210"],
  "latitude": 40.7128,
  "longitude": -74.0060
}
```

## Step 5: Configure Mobile App

1. Open `AreYouOk.MobileApp/Services/SettingsService.cs`
2. Update the API URL:

For **Android Emulator**:
```csharp
private const string DEFAULT_API_BASE_URL = "https://10.0.2.2:7001/api";
```

For **iOS Simulator**:
```csharp
private const string DEFAULT_API_BASE_URL = "https://localhost:7001/api";
```

For **Physical Device** (replace with your computer's IP):
```csharp
private const string DEFAULT_API_BASE_URL = "https://192.168.1.100:7001/api";
```

## Step 6: Run the Mobile App

### Using Visual Studio 2022:

1. Set `AreYouOk.MobileApp` as the startup project
2. Select your target (Android/iOS/Windows)
3. Press F5 to run

### Using Command Line:

For Android:
```bash
cd AreYouOk.MobileApp
dotnet build -t:Run -f net6.0-android
```

For iOS (Mac only):
```bash
cd AreYouOk.MobileApp
dotnet build -t:Run -f net6.0-ios
```

For Windows:
```bash
cd AreYouOk.MobileApp
dotnet build -t:Run -f net6.0-windows10.0.19041.0
```

## Troubleshooting

### Database Connection Issues

**Error**: "Cannot open database"
- Check SQL Server is running
- Verify connection string
- Check SQL Server authentication mode

**Solution for LocalDB**:
```bash
sqllocaldb start mssqllocaldb
```

### HTTPS Certificate Issues

**Error**: "SSL connection could not be established"

**Solution**:
```bash
dotnet dev-certs https --trust
```

### Mobile App Cannot Connect to API

**Android Emulator**:
- Use `10.0.2.2` instead of `localhost`
- Ensure API is running
- Check firewall settings

**Physical Device**:
- Ensure device and computer are on same network
- Use computer's IP address, not `localhost`
- May need to disable HTTPS redirect in development

### Build Errors

**Missing Workloads**:
```bash
dotnet workload restore
dotnet workload install android
dotnet workload install ios
dotnet workload install maccatalyst
```

## Testing the Complete Flow

1. **Register User** (via Swagger or Mobile App)
2. **Login** (get authentication token)
3. **Start Journey** with family member phone numbers
4. **Check Active Journey** status
5. **Update Safety Status** (click "I'm safe")
6. **Wait** (or manually set check-in time to 1 minute for testing)
7. **Observe Background Service** creates alert if deadline missed
8. **End Journey** when done

## Development Tips

### Watch API Logs
The background service logs every minute. Watch for:
```
Journey Monitoring Service started
Journey {id} missed deadline. User: {name}
Alert sent for journey {id}
```

### Test Notifications
Currently notifications are logged. To integrate real notifications:
1. Set up Firebase Cloud Messaging (FCM)
2. Update `NotificationService.cs`
3. Add device tokens to family members

### Debug Mobile App
- Use Visual Studio debugger
- Check Output window for exceptions
- Use breakpoints in ViewModels

## Next Steps

1. **Create MAUI Pages**: Build the UI pages (Login, Home, Journey, etc.)
2. **Add ViewModels**: Implement MVVM pattern
3. **Integrate Notifications**: Set up FCM for push notifications
4. **Add Maps**: Integrate maps for location display
5. **Testing**: Write unit and integration tests
6. **Deploy**: Prepare for production deployment

## Production Checklist

- [ ] Change JWT secret key
- [ ] Use production database
- [ ] Enable HTTPS only
- [ ] Configure proper CORS
- [ ] Add rate limiting
- [ ] Set up logging and monitoring
- [ ] Implement caching
- [ ] Add health checks
- [ ] Configure CI/CD
- [ ] Set up backup strategy
- [ ] Add privacy policy
- [ ] Test on all target platforms

---

**Need Help?** Check the main README.md for more details!
