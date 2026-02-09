using SQLite;
using AreYouOk.MobileApp.Data.Entities;
using System.Diagnostics;

namespace AreYouOk.MobileApp.Data;

/// <summary>
/// Service for managing local SQLite database operations
/// </summary>
public class DatabaseService
{
    private SQLiteAsyncConnection? _database;
    private bool _isInitialized = false;
    private readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);

    public DatabaseService()
    {
        Debug.WriteLine("[DatabaseService] Constructor called");
    }

    private SQLiteAsyncConnection GetDatabase()
    {
        if (_database == null)
        {
            try
            {
                var dbPath = Path.Combine(FileSystem.AppDataDirectory, "areyouok.db3");
                Debug.WriteLine($"[DatabaseService] Database path: {dbPath}");
                
                // Ensure directory exists
                var directory = Path.GetDirectoryName(dbPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    Debug.WriteLine($"[DatabaseService] Created directory: {directory}");
                }
                
                _database = new SQLiteAsyncConnection(dbPath, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DatabaseService] Error creating database connection: {ex.Message}");
                throw;
            }
        }
        return _database;
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized)
            return;

        await _initLock.WaitAsync();
        try
        {
            if (_isInitialized)
                return;

            Debug.WriteLine("[DatabaseService] Initializing database tables...");
            
            var db = GetDatabase();
            
            await db.CreateTableAsync<UserEntity>();
            await db.CreateTableAsync<JourneyEntity>();
            await db.CreateTableAsync<SafetyStatusEntity>();
            
            _isInitialized = true;
            Debug.WriteLine("[DatabaseService] Database initialized successfully");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DatabaseService] Error initializing database: {ex.Message}");
            Debug.WriteLine($"[DatabaseService] Stack trace: {ex.StackTrace}");
            throw;
        }
        finally
        {
            _initLock.Release();
        }
    }

    #region User Operations

    public async Task<UserEntity?> GetCurrentUserAsync()
    {
        await InitializeAsync();
        var db = GetDatabase();
        return await db.Table<UserEntity>()
            .Where(u => u.IsActive)
            .FirstOrDefaultAsync();
    }

    public async Task<UserEntity?> GetUserByServerIdAsync(int serverId)
    {
        await InitializeAsync();
        var db = GetDatabase();
        return await db.Table<UserEntity>()
            .Where(u => u.ServerId == serverId)
            .FirstOrDefaultAsync();
    }

    public async Task<int> SaveUserAsync(UserEntity user)
    {
        await InitializeAsync();
        var db = GetDatabase();
        
        // Deactivate all other users
        var existingUsers = await db.Table<UserEntity>().ToListAsync();
        foreach (var existingUser in existingUsers)
        {
            if (existingUser.Id != user.Id)
            {
                existingUser.IsActive = false;
                await db.UpdateAsync(existingUser);
            }
        }

        user.IsActive = true;
        user.LastSyncedAt = DateTime.UtcNow;

        if (user.Id == 0)
        {
            user.CreatedAt = DateTime.UtcNow;
            return await db.InsertAsync(user);
        }
        else
        {
            await db.UpdateAsync(user);
            return user.Id;
        }
    }

    public async Task<int> DeleteUserAsync(UserEntity user)
    {
        await InitializeAsync();
        var db = GetDatabase();
        return await db.DeleteAsync(user);
    }

    public async Task LogoutCurrentUserAsync()
    {
        await InitializeAsync();
        var db = GetDatabase();
        var currentUser = await GetCurrentUserAsync();
        if (currentUser != null)
        {
            currentUser.IsActive = false;
            currentUser.AuthToken = string.Empty;
            await db.UpdateAsync(currentUser);
        }
    }

    #endregion

    #region Journey Operations

    public async Task<JourneyEntity?> GetActiveJourneyAsync(int userId)
    {
        await InitializeAsync();
        var db = GetDatabase();
        return await db.Table<JourneyEntity>()
            .Where(j => j.UserId == userId && j.Status == "Active")
            .OrderByDescending(j => j.StartTime)
            .FirstOrDefaultAsync();
    }

    public async Task<JourneyEntity?> GetJourneyByServerIdAsync(int serverId)
    {
        await InitializeAsync();
        var db = GetDatabase();
        return await db.Table<JourneyEntity>()
            .Where(j => j.ServerId == serverId)
            .FirstOrDefaultAsync();
    }

    public async Task<List<JourneyEntity>> GetJourneysAsync(int userId, int skip = 0, int take = 20)
    {
        await InitializeAsync();
        var db = GetDatabase();
        return await db.Table<JourneyEntity>()
            .Where(j => j.UserId == userId)
            .OrderByDescending(j => j.StartTime)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> SaveJourneyAsync(JourneyEntity journey)
    {
        await InitializeAsync();
        var db = GetDatabase();
        journey.LastSyncedAt = DateTime.UtcNow;

        if (journey.Id == 0)
        {
            return await db.InsertAsync(journey);
        }
        else
        {
            await db.UpdateAsync(journey);
            return journey.Id;
        }
    }

    public async Task<int> DeleteJourneyAsync(JourneyEntity journey)
    {
        await InitializeAsync();
        var db = GetDatabase();
        return await db.DeleteAsync(journey);
    }

    #endregion

    #region Safety Status Operations

    public async Task<List<SafetyStatusEntity>> GetUnsyncedSafetyStatusesAsync(int userId)
    {
        await InitializeAsync();
        var db = GetDatabase();
        return await db.Table<SafetyStatusEntity>()
            .Where(s => s.UserId == userId && !s.IsSynced)
            .ToListAsync();
    }

    public async Task<List<SafetyStatusEntity>> GetSafetyStatusesForJourneyAsync(int journeyId)
    {
        await InitializeAsync();
        var db = GetDatabase();
        return await db.Table<SafetyStatusEntity>()
            .Where(s => s.JourneyId == journeyId)
            .OrderByDescending(s => s.CheckInTime)
            .ToListAsync();
    }

    public async Task<int> SaveSafetyStatusAsync(SafetyStatusEntity status)
    {
        await InitializeAsync();
        var db = GetDatabase();
        status.CreatedAt = DateTime.UtcNow;

        if (status.Id == 0)
        {
            return await db.InsertAsync(status);
        }
        else
        {
            await db.UpdateAsync(status);
            return status.Id;
        }
    }

    public async Task MarkSafetyStatusAsSyncedAsync(int localId, int serverId)
    {
        await InitializeAsync();
        var db = GetDatabase();
        var status = await db.Table<SafetyStatusEntity>()
            .Where(s => s.Id == localId)
            .FirstOrDefaultAsync();

        if (status != null)
        {
            status.IsSynced = true;
            status.ServerId = serverId;
            await db.UpdateAsync(status);
        }
    }

    #endregion

    #region Data Sync and Cleanup

    public async Task ClearAllDataAsync()
    {
        await InitializeAsync();
        var db = GetDatabase();
        await db.DeleteAllAsync<SafetyStatusEntity>();
        await db.DeleteAllAsync<JourneyEntity>();
        await db.DeleteAllAsync<UserEntity>();
        Debug.WriteLine("[DatabaseService] All data cleared");
    }

    public async Task<int> GetDatabaseSizeInBytesAsync()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "areyouok.db3");
        if (File.Exists(dbPath))
        {
            var fileInfo = new FileInfo(dbPath);
            return (int)fileInfo.Length;
        }
        return 0;
    }

    #endregion
}
