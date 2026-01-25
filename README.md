# AreYouOk - Personal Security Mobile Application

A comprehensive safety tracking application built with .NET 10, MAUI, and SQL Server that allows users to share their safety status with family members during journeys.

## 🎯 Features

### Core Functionality
- **User Registration & Authentication**: Secure JWT-based authentication
- **Journey Management**: Start/end journeys with customizable check-in intervals
- **Safety Check-ins**: Manual "I'm safe" or "I'm not safe" buttons
- **Automatic Alerts**: System automatically alerts family members if check-in deadline is missed
- **Location Tracking**: Real-time GPS location sharing with family members
- **Family Member Management**: Add family members by phone number
- **Journey History**: View past journeys and safety status updates
- **Monitoring Dashboard**: Family members can monitor active journeys

### Security Features
- Password hashing with BCrypt
- JWT token authentication
- Secure API endpoints with role-based access

### Technical Highlights
- **.NET 10**: Latest .NET framework
- **MAUI**: Cross-platform mobile app (Android, iOS, Windows)
- **SQL Server**: Robust database with Entity Framework Core
- **Background Service**: Automatic monitoring of journey deadlines
- **RESTful API**: Clean API architecture with Swagger documentation

## 📁 Project Structure

```
AreYouOk/
├── AreYouOk.API/                 # Backend Web API
│   ├── Controllers/              # API controllers
│   ├── Data/                     # DbContext and migrations
│   ├── Models/                   # Database entities
│   ├── Services/                 # Business logic services
│   ├── BackgroundServices/       # Journey monitoring service
│   └── Program.cs               # API configuration
│
├── AreYouOk.MobileApp/          # MAUI Mobile App
│   ├── Pages/                    # XAML pages
│   ├── ViewModels/               # MVVM ViewModels
│   ├── Services/                 # API client & services
│   └── MauiProgram.cs           # App configuration
│
└── AreYouOk.Shared/             # Shared DTOs and Models
    └── DTOs/                     # Data Transfer Objects
```

## 🚀 Getting Started

### Prerequisites
- .NET 10 SDK
- Visual Studio 2022 (v17.12 or later) or VS Code
- SQL Server or SQL Server Express / LocalDB
- Android SDK (for Android development)
- Xcode (for iOS development on Mac)

### Database Setup

1. **Update Connection String** in `AreYouOk.API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AreYouOkDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

2. **Create Database Migration**:
```bash
cd AreYouOk.API
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Running the Backend API

```bash
cd AreYouOk.API
dotnet run
```

The API will be available at:
- HTTPS: `https://localhost:7001`
- HTTP: `http://localhost:5001`
- Swagger UI: `https://localhost:7001` (root)

### Running the Mobile App

1. **Update API Base URL** in `AreYouOk.MobileApp/Services/SettingsService.cs`:
```csharp
private const string DEFAULT_API_BASE_URL = "https://YOUR_API_HOST:7001/api";
```

For Android emulator, use: `https://10.0.2.2:7001/api`

2. **Run the app**:
```bash
cd AreYouOk.MobileApp
dotnet build
dotnet run --framework net6.0-android  # For Android
```

Or use Visual Studio to run on your preferred platform.

## 📱 How to Use the App

### For Journey Owners

1. **Register/Login**: Create an account or login with existing credentials
2. **Start Journey**:
   - Set check-in interval (e.g., 10 hours)
   - Add family member phone numbers
   - Start journey with current location
3. **During Journey**:
   - Click "I'm Safe" button regularly within the deadline
   - Click "I'm Not Safe" if you're in danger
   - App tracks your location automatically
4. **End Journey**: Click "End Journey" when you reach your destination safely

### For Family Members

1. **Receive Notification**: Get notified when added to someone's journey
2. **Monitor Status**: View active journeys you're monitoring
3. **Get Alerts**: Receive notifications when:
   - User checks in as safe
   - User reports danger
   - User misses check-in deadline

## 🔒 Security Configuration

### JWT Settings
Update in `appsettings.json`:
```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLongForHS256!",
    "Issuer": "AreYouOkAPI",
    "Audience": "AreYouOkApp",
    "ExpiryHours": "720"
  }
}
```

**Important**: Change the `SecretKey` in production!

## 📊 Database Schema

### Tables
- **Users**: User accounts (FirstName, LastName, Email, Phone, PasswordHash)
- **Journeys**: Journey records (UserId, StartTime, EndTime, CheckInInterval, Status, Location)
- **FamilyMembers**: Journey family members (JourneyId, PhoneNumber, UserId, DeviceToken)
- **SafetyStatuses**: Safety check-in records (JourneyId, IsSafe, Timestamp, Location, Message)

## 🔧 API Endpoints

### Authentication
- `POST /api/Auth/register` - Register new user
- `POST /api/Auth/login` - Login user
- `GET /api/Auth/profile` - Get user profile
- `PUT /api/Auth/profile` - Update user profile

### Journey Management
- `POST /api/Journey/start` - Start new journey
- `GET /api/Journey/active` - Get active journey
- `GET /api/Journey/{id}` - Get journey by ID
- `POST /api/Journey/end` - End journey
- `PUT /api/Journey/location` - Update location
- `GET /api/Journey/history` - Get journey history
- `GET /api/Journey/monitoring` - Get monitored journeys

### Safety Status
- `POST /api/SafetyStatus` - Update safety status
- `GET /api/SafetyStatus/journey/{journeyId}` - Get status history

## 🎨 UI Features

The MAUI app includes:
- **Responsive Design**: Adapts to all mobile screen sizes
- **Beautiful UI**: Modern, user-friendly interface
- **Location Maps**: Visual location display
- **Push Notifications**: Real-time alerts
- **Dark Mode Support**: Comfortable viewing in any lighting

## 🔄 Background Services

### Journey Monitoring Service
- Runs every minute
- Checks all active journeys
- Automatically sends alerts when deadlines are missed
- Updates journey status to "Alert"
- Notifies all family members

## 📦 NuGet Packages Used

### Backend API
- Microsoft.EntityFrameworkCore.SqlServer
- Microsoft.EntityFrameworkCore.Tools
- Microsoft.AspNetCore.Authentication.JwtBearer
- Swashbuckle.AspNetCore
- BCrypt.Net-Next

### Mobile App
- CommunityToolkit.Maui
- CommunityToolkit.Mvvm
- Plugin.LocalNotification
- Microsoft.Maui.Controls

## 🚧 Future Enhancements

- Push notifications via Firebase Cloud Messaging (FCM)
- SMS alerts via Twilio
- Email notifications
- Geofencing
- Route tracking
- Emergency contacts
- SOS button
- Voice commands
- Offline mode

## 🤝 Contributing

This is a demonstration project. Feel free to fork and enhance it!

## 📄 License

This project is for educational purposes.

## 📞 Support

For issues or questions, please create an issue in the repository.

## ⚠️ Important Notes

1. **Production Deployment**:
   - Change JWT secret key
   - Use proper SSL certificates
   - Configure CORS properly
   - Set up proper database security
   - Implement rate limiting
   - Add logging and monitoring

2. **Notification Setup**:
   - Current implementation logs notifications
   - Integrate with FCM for push notifications
   - Add SMS gateway integration

3. **Testing**:
   - Test thoroughly on all target platforms
   - Implement unit tests
   - Add integration tests
   - Test background service thoroughly

4. **Privacy**:
   - Ensure compliance with data protection regulations
   - Add privacy policy
   - Implement data retention policies
   - Allow users to delete their data

---

**Built with ❤️ using .NET 10, MAUI, and SQL Server**
