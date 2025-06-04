using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelLayer.Entity;
using RepositoryLayer.Context;
using RepositoryLayer.DTO;
using RepositoryLayer.Interface;
using StackExchange.Redis;

namespace RepositoryLayer.Service
{
    public class AddressImplRL : IAddressRL
    {
        private readonly UserContext _context;
        private readonly ILogger<AddressImplRL> _logger;
        private readonly IDatabase _redisDatabase;
        private readonly IConnectionMultiplexer _redisConnection;

        public AddressImplRL(UserContext context, ILogger<AddressImplRL> logger, IConnectionMultiplexer redis)
        {
            _context = context;
            _logger = logger;
            _redisConnection = redis;
            _redisDatabase = redis.GetDatabase();
            _logger.LogDebug("AddressImplRL initialized for UserContext and Redis connection.");
        }

        private string GetUserAddressCacheKey(int userId) => $"UserAddress:{userId}";
        private string GetAllAddressCacheKey() => "All_addresses";

        public async Task<ResponseDTO<AddressEntity>> AddAddressAsync(AddAddressRequestDTO request, int userId)
        {
            _logger.LogInformation("Attempting to add address for UserId: {UserId}, AddressType: {AddressType}", userId, request.AddressType);
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogDebug("Checking for existing address with UserId: {UserId}, AddressType: {AddressType}", userId, request.AddressType);
                var address = await _context.Addresses.FirstOrDefaultAsync(a => a.UserId == userId && a.AddressType == request.AddressType);
                if (address != null)
                {
                    _logger.LogWarning("Address already exists for UserId: {UserId}, AddressType: {AddressType}", userId, request.AddressType);
                    return new ResponseDTO<AddressEntity>
                    {
                        IsSuccess = false,
                        Message = "Address already exists for this address type."
                    };
                }

                var newAddress = new AddressEntity
                {
                    UserId = userId,
                    Address = request.Address,
                    AddressType = request.AddressType,
                    City = request.City,
                    State = request.State,
                    Locality = request.Locality,
                    PhoneNumber = request.PhoneNumber,
                    FirstName = request.FirstName
                };

                _logger.LogDebug("Adding new address to database for UserId: {UserId}", userId);
                await _context.Addresses.AddAsync(newAddress);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogDebug("Caching new address for UserId: {UserId}", userId);
                var cacheKey = GetUserAddressCacheKey(userId);
                await _redisDatabase.StringSetAsync(cacheKey, JsonSerializer.Serialize(newAddress), TimeSpan.FromMinutes(30));

                var allCacheKey = GetAllAddressCacheKey();
                var allCached = await _redisDatabase.StringGetAsync(allCacheKey);
                List<AddressEntity> addressList;

                _logger.LogDebug("Updating all addresses cache");
                if (allCached.HasValue)
                {
                    try
                    {
                        addressList = JsonSerializer.Deserialize<List<AddressEntity>>(allCached) ?? new List<AddressEntity>();
                        _logger.LogDebug("Deserialized existing all addresses cache with {Count} entries", addressList.Count);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to deserialize all addresses cache. Initializing new list.");
                        addressList = new List<AddressEntity>();
                    }
                }
                else
                {
                    _logger.LogDebug("No existing all addresses cache found. Initializing new list.");
                    addressList = new List<AddressEntity>();
                }

                addressList.Add(newAddress);
                await _redisDatabase.StringSetAsync(allCacheKey, JsonSerializer.Serialize(addressList), TimeSpan.FromMinutes(30));
                _logger.LogInformation("Address added successfully for UserId: {UserId}, AddressType: {AddressType}", userId, request.AddressType);

                return new ResponseDTO<AddressEntity>
                {
                    IsSuccess = true,
                    Message = "Address added successfully.",
                    Data = newAddress
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding address for UserId: {UserId}", userId);
                await transaction.RollbackAsync();
                return new ResponseDTO<AddressEntity>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseDTO<AddressEntity>> DeleteAddressAsync(int addressId, int userId)
        {
            _logger.LogInformation("Attempting to delete address with AddressId: {AddressId}", addressId);
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogDebug("Searching for address with AddressId: {AddressId}", addressId);
                var address = await _context.Addresses.FirstOrDefaultAsync(x => x.AddressId == addressId && x.UserId == userId);
                if (address == null)
                {
                    _logger.LogWarning("Address not found for AddressId: {AddressId}", addressId);
                    return new ResponseDTO<AddressEntity>
                    {
                        IsSuccess = false,
                        Message = "Address not found."
                    };
                }

                _logger.LogDebug("Removing address with AddressId: {AddressId} from database", addressId);
                _context.Addresses.Remove(address);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogDebug("Clearing user address cache for UserId: {UserId}", address.UserId);
                var userCacheKey = GetUserAddressCacheKey(address.UserId);
                await _redisDatabase.KeyDeleteAsync(userCacheKey);

                var allAddressesCacheKey = GetAllAddressCacheKey();
                var cachedAllAddresses = await _redisDatabase.StringGetAsync(allAddressesCacheKey);
                if (cachedAllAddresses.HasValue)
                {
                    try
                    {
                        _logger.LogDebug("Updating all addresses cache after deletion");
                        var allAddresses = JsonSerializer.Deserialize<List<AddressEntity>>(cachedAllAddresses) ?? new List<AddressEntity>();
                        allAddresses.RemoveAll(a => a.AddressId == addressId);
                        await _redisDatabase.StringSetAsync(
                            allAddressesCacheKey,
                            JsonSerializer.Serialize(allAddresses),
                            TimeSpan.FromMinutes(30));
                        _logger.LogDebug("All addresses cache updated, {Count} addresses remaining", allAddresses.Count);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error deserializing all addresses cache during deletion. Clearing cache.");
                        await _redisDatabase.KeyDeleteAsync(allAddressesCacheKey);
                    }
                }

                _logger.LogInformation("Address deleted successfully for AddressId: {AddressId}", addressId);
                return new ResponseDTO<AddressEntity>
                {
                    IsSuccess = true,
                    Message = "Address deleted successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting address with AddressId: {AddressId}", addressId);
                await transaction.RollbackAsync();
                return new ResponseDTO<AddressEntity>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseDTO<List<AddressEntity>>> GetAllAddressesAsync(int userId)
        {
            _logger.LogInformation("Retrieving all addresses for UserId: {UserId}", userId);
            try
            {
                var cacheKey = GetAllAddressCacheKey();
                var cachedAddresses = await _redisDatabase.StringGetAsync(cacheKey);
                if (cachedAddresses.HasValue)
                {
                    try
                    {
                        _logger.LogDebug("Found cached addresses for key: {CacheKey}", cacheKey);
                        var addresses = JsonSerializer.Deserialize<List<AddressEntity>>(cachedAddresses);
                        _logger.LogInformation("Retrieved {Count} addresses from cache for UserId: {UserId}", addresses?.Count ?? 0, userId);
                        return new ResponseDTO<List<AddressEntity>>
                        {
                            IsSuccess = true,
                            Message = "Addresses retrieved from cache.",
                            Data = addresses
                        };
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error deserializing address list for UserId: {UserId}. Clearing cache.", userId);
                        await _redisDatabase.KeyDeleteAsync(cacheKey);
                    }
                }

                _logger.LogDebug("No cache found, querying database for UserId: {UserId}", userId);
                var addressesFromDb = await _context.Addresses.Where(a => a.UserId == userId).ToListAsync();
                if (addressesFromDb == null || addressesFromDb.Count == 0)
                {
                    _logger.LogWarning("No addresses found in database for UserId: {UserId}", userId);
                    return new ResponseDTO<List<AddressEntity>>
                    {
                        IsSuccess = false,
                        Message = "No addresses found."
                    };
                }

                _logger.LogDebug("Caching {Count} addresses for UserId: {UserId}", addressesFromDb.Count, userId);
                await _redisDatabase.StringSetAsync(cacheKey, JsonSerializer.Serialize(addressesFromDb), TimeSpan.FromMinutes(30));

                _logger.LogInformation("Retrieved {Count} addresses from database for UserId: {UserId}", addressesFromDb.Count, userId);
                return new ResponseDTO<List<AddressEntity>>
                {
                    IsSuccess = true,
                    Message = "Addresses retrieved from database.",
                    Data = addressesFromDb
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving addresses for UserId: {UserId}", userId);
                return new ResponseDTO<List<AddressEntity>>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseDTO<AddressEntity>> UpdateAddressAsync(UpdateAddressRequestDTO request, int userId)
        {
            _logger.LogInformation("Attempting to update address with AddressId: {AddressId} for UserId: {UserId}", request.AddressId, userId);
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogDebug("Searching for address with AddressId: {AddressId} and UserId: {UserId}", request.AddressId, userId);
                var address = await _context.Addresses.FirstOrDefaultAsync(a => a.AddressId == request.AddressId && a.UserId == userId);
                if (address == null)
                {
                    _logger.LogWarning("Address not found for AddressId: {AddressId}, UserId: {UserId}", request.AddressId, userId);
                    return new ResponseDTO<AddressEntity>
                    {
                        IsSuccess = false,
                        Message = "Address not found."
                    };
                }

                _logger.LogDebug("Updating address fields for AddressId: {AddressId}", request.AddressId);
                address.AddressType = request.AddressType;
                address.City = request.City;
                address.State = request.State;

                _context.Addresses.Update(address);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogDebug("Caching updated address for UserId: {UserId}", userId);
                var userCacheKey = GetUserAddressCacheKey(address.UserId);
                await _redisDatabase.StringSetAsync(userCacheKey, JsonSerializer.Serialize(address), TimeSpan.FromMinutes(30));

                var allCacheKey = GetAllAddressCacheKey();
                var cachedAll = await _redisDatabase.StringGetAsync(allCacheKey);

                if (cachedAll.HasValue)
                {
                    try
                    {
                        _logger.LogDebug("Updating all addresses cache for AddressId: {AddressId}", address.AddressId);
                        var addressList = JsonSerializer.Deserialize<List<AddressEntity>>(cachedAll) ?? new List<AddressEntity>();
                        var index = addressList.FindIndex(a => a.AddressId == address.AddressId);
                        if (index >= 0)
                        {
                            addressList[index] = address;
                            _logger.LogDebug("Updated existing address in all addresses cache at index: {Index}", index);
                        }
                        else
                        {
                            addressList.Add(address);
                            _logger.LogDebug("Added address to all addresses cache");
                        }

                        await _redisDatabase.StringSetAsync(allCacheKey, JsonSerializer.Serialize(addressList), TimeSpan.FromMinutes(30));
                        _logger.LogDebug("All addresses cache updated with {Count} entries", addressList.Count);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error updating all addresses cache for AddressId: {AddressId}. Clearing cache.", address.AddressId);
                        await _redisDatabase.KeyDeleteAsync(allCacheKey);
                    }
                }

                _logger.LogInformation("Address updated successfully for AddressId: {AddressId}, UserId: {UserId}", address.AddressId, userId);
                return new ResponseDTO<AddressEntity>
                {
                    IsSuccess = true,
                    Message = "Address updated successfully.",
                    Data = address
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address with AddressId: {AddressId} for UserId: {UserId}", request.AddressId, userId);
                await transaction.RollbackAsync();
                return new ResponseDTO<AddressEntity>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
    }
}