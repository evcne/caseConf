using System.Collections.Concurrent;
using ConfigurationReader.Data;
using ConfigurationReader.Models;
using Microsoft.EntityFrameworkCore;
using System.Timers;

namespace ConfigurationReader
{
    public class ConfigurationReader
    {
        private readonly string _applicationName;
        private ConfigurationDbContext _dbContext;
        private readonly ConcurrentDictionary<string, string> _cache;
        private readonly System.Timers.Timer _timer;

        public string ApplicationName { get; }

        public ConfigurationReader(ConfigurationDbContext dbContext, string applicationName, int refreshIntervalMs)
        {
            _applicationName = applicationName;
            _dbContext = dbContext;
            _cache = new ConcurrentDictionary<string, string>();

            // İlk yükleme
            LoadConfigurationsAsync().Wait();

            // Periyodik güncelleme için timer başlat
            _timer = new System.Timers.Timer(refreshIntervalMs);
            _timer.Elapsed += async (s, e) => await LoadConfigurationsAsync();
            _timer.AutoReset = true;
            _timer.Enabled = true;

            ApplicationName = applicationName;

        }

        private ConcurrentDictionary<string, string> _cacheLoad = new();

        private async Task LoadConfigurationsAsync()
        {
            try
            {
                var configs = await _dbContext.ConfigurationEntries
                    .Where(c => c.ApplicationName == _applicationName && c.IsActive == true)
                    .ToListAsync();

                // Yeni cache dictionary oluştur
                var newCache = new ConcurrentDictionary<string, string>(
                    configs.Select(c => new KeyValuePair<string, string>(c.Name, c.Value))
                );

                // Cache'i atomik olarak değiştir
                Interlocked.Exchange(ref _cacheLoad, newCache);

                Console.WriteLine("✅ Config cache updated.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Config refresh failed: {ex.Message}");
            }
        }


        /*private async Task LoadConfigurationsAsync()
        {
            try
            {
                var configs = await _dbContext.ConfigurationEntries
                    .Where(c => c.ApplicationName == _applicationName && c.IsActive)
                    .ToListAsync();

                _cache.Clear();
                foreach (var config in configs)
                {
                    _cache[config.Name] = config.Value;
                }

                Console.WriteLine(" Config cache updated.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Config refresh failed: {ex.Message}");
            }
        }*/

        public T GetValue<T>(string key)
        {
            if (!_cache.TryGetValue(key, out var stringValue))
            {
                throw new KeyNotFoundException($"Configuration key not found: {key}");
            }

            try
            {
                return (T)Convert.ChangeType(stringValue, typeof(T));
            }
            catch (Exception ex)
            {
                throw new InvalidCastException($"Cannot convert key '{key}' value to {typeof(T).Name}.", ex);
            }
        }
    }
}
