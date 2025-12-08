using System;
using System.Collections.Generic;
using LiteDB;
using SoftwareCenter.Core.Diagnostics;
using SoftwareCenter.Kernel.Data;

namespace SoftwareCenter.Kernel.Services
{
    /// <summary>
    /// Provides a shared, persistent data store for modules with ownership and audit tracing.
    /// </summary>
    public class GlobalDataStore : IDisposable
    {
        private readonly LiteDatabase _db;
        private readonly ILiteCollection<BsonDocument> _collection;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalDataStore"/> class.
        /// </summary>
        /// <param name="databasePath">The path to the LiteDB database file.</param>
        public GlobalDataStore(string databasePath = "global_store.db")
        {
            _db = new LiteDatabase(databasePath);
            _collection = _db.GetCollection("datastore");
            _collection.EnsureIndex(x => x["Key"]);
        }

        public T Get<T>(string key, ITraceContext traceContext)
        {
            var doc = _collection.FindOne(Query.EQ("Key", key));
            if (doc == null)
            {
                return default;
            }

            var storeItem = BsonMapper.Global.ToObject<StoreItem<T>>(doc);
            
            // Audit log the read operation (implementation to be added)
            // LogAccess(key, "Get", traceContext.TraceId);

            return storeItem.Value;
        }

        public void Set<T>(string key, T value, string ownerModuleId, ITraceContext traceContext)
        {
            var existingDoc = _collection.FindOne(Query.EQ("Key", key));

            if (existingDoc != null)
            {
                var existingItem = BsonMapper.Global.ToObject<StoreItem<T>>(existingDoc);
                if (existingItem.OwnerModuleId != ownerModuleId)
                {
                    throw new InvalidOperationException($"Module '{ownerModuleId}' does not have ownership of key '{key}'.");
                }

                existingItem.Value = value;
                existingItem.LastUpdatedAt = DateTimeOffset.UtcNow;
                existingItem.LastUpdaterTraceId = traceContext.TraceId;
                _collection.Update(BsonMapper.Global.ToBson(existingItem));
            }
            else
            {
                var newItem = new StoreItem<T>
                {
                    Id = Guid.NewGuid(),
                    Key = key,
                    Value = value,
                    OwnerModuleId = ownerModuleId,
                    CreatorTraceId = traceContext.TraceId,
                    CreatedAt = DateTimeOffset.UtcNow,
                    LastUpdaterTraceId = traceContext.TraceId,
                    LastUpdatedAt = DateTimeOffset.UtcNow
                };
                _collection.Insert(BsonMapper.Global.ToBson(newItem));
            }
        }

        public void Dispose()
        {
            _db?.Dispose();
        }
    }
}
