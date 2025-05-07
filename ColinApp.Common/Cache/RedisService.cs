using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ColinApp.Common.Cache
{
    public class RedisService
    {
        private readonly IDatabase _db;
        private readonly IConnectionMultiplexer _conn;

        public RedisService(string connectionString)
        {
            _conn = ConnectionMultiplexer.Connect(connectionString);
            _db = _conn.GetDatabase();
        }

        #region String 操作

        public async Task<bool> SetStringAsync(string key, string value, TimeSpan? expiry = null)
        {
            return await _db.StringSetAsync(key, value, expiry);
        }

        public async Task<string?> GetStringAsync(string key)
        {
            var value = await _db.StringGetAsync(key);
            return value.HasValue ? value.ToString() : null;
        }

        public async Task<bool> SetObjectAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var json = JsonSerializer.Serialize(value);
            return await _db.StringSetAsync(key, json, expiry);
        }

        public async Task<T?> GetObjectAsync<T>(string key)
        {
            var json = await _db.StringGetAsync(key);
            return json.HasValue ? JsonSerializer.Deserialize<T>(json!) : default;
        }

        #endregion

        #region Key 操作

        public async Task<bool> ExistsAsync(string key) => await _db.KeyExistsAsync(key);

        public async Task<bool> RemoveAsync(string key) => await _db.KeyDeleteAsync(key);

        public async Task<bool> ExpireAsync(string key, TimeSpan expiry) => await _db.KeyExpireAsync(key, expiry);

        public async Task<bool> RenameAsync(string oldKey, string newKey) => await _db.KeyRenameAsync(oldKey, newKey);

        #endregion

        #region Hash 操作

        public async Task HashSetAsync(string key, string field, string value)
        {
            await _db.HashSetAsync(key, field, value);
        }

        public async Task<string?> HashGetAsync(string key, string field)
        {
            var value = await _db.HashGetAsync(key, field);
            return value.HasValue ? value.ToString() : null;
        }

        public async Task<bool> HashDeleteAsync(string key, string field)
        {
            return await _db.HashDeleteAsync(key, field);
        }

        public async Task<bool> HashExistsAsync(string key, string field)
        {
            return await _db.HashExistsAsync(key, field);
        }

        public async Task<Dictionary<string, string>> HashGetAllAsync(string key)
        {
            var entries = await _db.HashGetAllAsync(key);
            return entries.ToDictionary(e => e.Name.ToString(), e => e.Value.ToString());
        }

        #endregion

        #region List 示例

        public async Task<long> ListLeftPushAsync(string key, string value) => await _db.ListLeftPushAsync(key, value);
        public async Task<string?> ListRightPopAsync(string key)
        {
            var val = await _db.ListRightPopAsync(key);
            return val.HasValue ? val.ToString() : null;
        }

        #endregion

        #region Set 示例

        public async Task<bool> SetAddAsync(string key, string value) => await _db.SetAddAsync(key, value);
        public async Task<bool> SetRemoveAsync(string key, string value) => await _db.SetRemoveAsync(key, value);
        public async Task<bool> SetContainsAsync(string key, string value) => await _db.SetContainsAsync(key, value);

        #endregion

        #region Publish/Subscribe（订阅仍为同步）

        public async Task<long> PublishAsync(string channel, string message) =>
            await _conn.GetSubscriber().PublishAsync(channel, message);

        public void Subscribe(string channel, Action<RedisChannel, RedisValue> handler) =>
            _conn.GetSubscriber().Subscribe(channel, handler);

        #endregion
    }
}
