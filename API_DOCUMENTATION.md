# API Documentation - AreYouOk

Base URL: `https://localhost:7001/api` (Development)

## Authentication

All authenticated endpoints require a JWT Bearer token in the Authorization header:
```
Authorization: Bearer YOUR_JWT_TOKEN
```

---

## Auth Endpoints

### POST /api/Auth/register
Register a new user account.

**Request Body:**
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@example.com",
  "phoneNumber": "+1234567890",
  "password": "Password123",
  "confirmPassword": "Password123"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "userId": 1,
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@example.com",
    "phoneNumber": "+1234567890",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAt": "2026-02-23T10:30:00Z"
  },
  "errorMessage": null,
  "errors": null
}
```

---

### POST /api/Auth/login
Authenticate user and get token.

**Request Body:**
```json
{
  "email": "john@example.com",
  "password": "Password123"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "userId": 1,
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@example.com",
    "phoneNumber": "+1234567890",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAt": "2026-02-23T10:30:00Z"
  }
}
```

**Error Response (401 Unauthorized):**
```json
{
  "success": false,
  "data": null,
  "errorMessage": "Invalid email or password"
}
```

---

### GET /api/Auth/profile
Get current user's profile. **[Requires Authentication]**

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@example.com",
    "phoneNumber": "+1234567890",
    "fullName": "John Doe"
  }
}
```

---

### PUT /api/Auth/profile
Update current user's profile. **[Requires Authentication]**

**Request Body:**
```json
{
  "firstName": "John",
  "lastName": "Smith",
  "phoneNumber": "+1234567890"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "firstName": "John",
    "lastName": "Smith",
    "email": "john@example.com",
    "phoneNumber": "+1234567890"
  }
}
```

---

## Journey Endpoints

### POST /api/Journey/start
Start a new journey. **[Requires Authentication]**

**Request Body:**
```json
{
  "checkInIntervalHours": 10,
  "familyMemberPhones": [
    "+9876543210",
    "+1122334455"
  ],
  "latitude": 40.7128,
  "longitude": -74.0060
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "userId": 1,
    "userName": "John Doe",
    "startTime": "2026-01-23T10:00:00Z",
    "endTime": null,
    "checkInIntervalHours": 10,
    "lastCheckInTime": "2026-01-23T10:00:00Z",
    "status": "Active",
    "latitude": 40.7128,
    "longitude": -74.0060,
    "lastLocationUpdate": "2026-01-23T10:00:00Z",
    "familyMembers": [
      {
        "id": 1,
        "phoneNumber": "+9876543210",
        "userId": 2,
        "displayName": "Jane Smith",
        "receiveNotifications": true,
        "addedAt": "2026-01-23T10:00:00Z",
        "isRegisteredUser": true
      }
    ],
    "timeUntilNextCheckIn": "09:59:45",
    "isOverdue": false
  }
}
```

**Error Response (400 Bad Request):**
```json
{
  "success": false,
  "data": null,
  "errorMessage": "You already have an active journey. Please end it before starting a new one."
}
```

---

### GET /api/Journey/active
Get user's active journey. **[Requires Authentication]**

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "userId": 1,
    "userName": "John Doe",
    "startTime": "2026-01-23T10:00:00Z",
    "status": "Active",
    "checkInIntervalHours": 10,
    "lastCheckInTime": "2026-01-23T10:00:00Z",
    "latitude": 40.7128,
    "longitude": -74.0060,
    "familyMembers": [/* ... */],
    "timeUntilNextCheckIn": "08:45:30",
    "isOverdue": false
  }
}
```

**Error Response (404 Not Found):**
```json
{
  "success": false,
  "errorMessage": "No active journey found"
}
```

---

### GET /api/Journey/{id}
Get journey by ID. **[Requires Authentication]**

User must be the journey owner or a family member.

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "userId": 1,
    "userName": "John Doe",
    "startTime": "2026-01-23T10:00:00Z",
    "endTime": null,
    "status": "Active",
    /* ... */
  }
}
```

---

### POST /api/Journey/end
End active journey. **[Requires Authentication]**

**Request Body:**
```json
{
  "latitude": 40.7580,
  "longitude": -73.9855
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "status": "Completed",
    "endTime": "2026-01-23T15:30:00Z",
    /* ... */
  }
}
```

---

### PUT /api/Journey/location
Update current location. **[Requires Authentication]**

**Request Body:**
```json
{
  "latitude": 40.7580,
  "longitude": -73.9855
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "latitude": 40.7580,
    "longitude": -73.9855,
    "lastLocationUpdate": "2026-01-23T12:00:00Z",
    /* ... */
  }
}
```

---

### GET /api/Journey/history
Get journey history with pagination. **[Requires Authentication]**

**Query Parameters:**
- `page` (optional, default: 1)
- `pageSize` (optional, default: 20)

**Request:**
```
GET /api/Journey/history?page=1&pageSize=10
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "startTime": "2026-01-23T10:00:00Z",
      "endTime": "2026-01-23T15:30:00Z",
      "status": "Completed"
      /* ... */
    },
    /* more journeys */
  ]
}
```

---

### GET /api/Journey/monitoring
Get journeys where user is a family member. **[Requires Authentication]**

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "id": 5,
      "userId": 3,
      "userName": "Alice Johnson",
      "status": "Active",
      "startTime": "2026-01-23T08:00:00Z",
      "checkInIntervalHours": 12,
      "lastCheckInTime": "2026-01-23T08:00:00Z",
      "timeUntilNextCheckIn": "11:45:20",
      "isOverdue": false,
      /* ... */
    }
  ]
}
```

---

## Safety Status Endpoints

### POST /api/SafetyStatus
Update safety status (check-in). **[Requires Authentication]**

**Request Body:**
```json
{
  "isSafe": true,
  "latitude": 40.7580,
  "longitude": -73.9855,
  "message": "All good, on my way home"
}
```

For alert:
```json
{
  "isSafe": false,
  "latitude": 40.7580,
  "longitude": -73.9855,
  "message": "Need help!"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "journeyId": 1,
    "isSafe": true,
    "timestamp": "2026-01-23T12:00:00Z",
    "latitude": 40.7580,
    "longitude": -73.9855,
    "message": "All good, on my way home",
    "updateType": "ManualSafe"
  }
}
```

---

### GET /api/SafetyStatus/journey/{journeyId}
Get safety status history for a journey. **[Requires Authentication]**

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "journeyId": 1,
      "isSafe": true,
      "timestamp": "2026-01-23T12:00:00Z",
      "latitude": 40.7580,
      "longitude": -73.9855,
      "message": "All good",
      "updateType": "ManualSafe"
    },
    {
      "id": 2,
      "journeyId": 1,
      "isSafe": false,
      "timestamp": "2026-01-23T22:05:00Z",
      "latitude": 40.7128,
      "longitude": -74.0060,
      "message": "Automatic alert: User missed check-in deadline",
      "updateType": "AutomaticAlert"
    }
  ]
}
```

---

## Error Responses

### Validation Error (400 Bad Request)
```json
{
  "success": false,
  "errorMessage": "Validation failed",
  "errors": {
    "Email": ["Email is required", "Invalid email address"],
    "Password": ["Password must be between 6 and 100 characters"]
  }
}
```

### Unauthorized (401)
```json
{
  "success": false,
  "errorMessage": "Unauthorized"
}
```

### Forbidden (403)
```json
{
  "success": false,
  "errorMessage": "Access denied"
}
```

### Not Found (404)
```json
{
  "success": false,
  "errorMessage": "Resource not found"
}
```

### Server Error (500)
```json
{
  "success": false,
  "errorMessage": "An error occurred while processing your request"
}
```

---

## Status Codes

- `200 OK` - Request successful
- `400 Bad Request` - Invalid request data
- `401 Unauthorized` - Missing or invalid token
- `403 Forbidden` - Insufficient permissions
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

---

## Rate Limiting

*To be implemented in production*

Recommended:
- Anonymous endpoints: 100 requests per 15 minutes
- Authenticated endpoints: 1000 requests per 15 minutes

---

## Swagger/OpenAPI

Interactive API documentation available at: `https://localhost:7001`

Features:
- Try out endpoints
- View request/response schemas
- Test authentication
- Download OpenAPI spec

---

## Webhooks (Future)

*Planned for future implementation*

Webhooks will notify external systems when:
- Journey starts
- Journey ends
- Alert triggered
- Check-in received

---

For complete examples and integration guides, see [QUICKSTART.md](QUICKSTART.md)
