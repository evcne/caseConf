using Xunit;
using ConfigurationReader;
using ConfigurationReader.Models;
using ConfigurationReader.Data;
using Microsoft.EntityFrameworkCore;

namespace Configuration.Tests
{
    public class ConfigurationTests
    {
        private ConfigurationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ConfigurationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new ConfigurationDbContext(options);

            context.ConfigurationEntries.AddRange(
                new ConfigurationEntry
                {
                    Name = "SiteName",
                    Type = "string",
                    Value = "testsite.com",
                    IsActive = true,
                    ApplicationName = "SERVICE-A"
                },
                new ConfigurationEntry
                {
                    Name = "MaxItemCount",
                    Type = "int",
                    Value = "25",
                    IsActive = true,
                    ApplicationName = "SERVICE-A"
                }
            );
            context.SaveChanges();

            return context;
        }


        [Fact]
        public void GetValue_ReturnsCorrectStringValue()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var reader = new ConfigurationReader.ConfigurationReader(context, "SERVICE-A", 10000);

            // Act
            string siteName = reader.GetValue<string>("SiteName");

            // Assert
            Assert.Equal("testsite.com", siteName);
        }

        [Fact]
        public void GetValue_ReturnsCorrectIntValue()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var reader = new ConfigurationReader.ConfigurationReader(context, "SERVICE-A", 10000);

            // Act
            int itemCount = reader.GetValue<int>("MaxItemCount");

            // Assert
            Assert.Equal(25, itemCount);
        }

        [Fact]
        public void GetValue_KeyNotFound_ThrowsException()
        {
            var context = GetInMemoryDbContext();
            var reader = new ConfigurationReader.ConfigurationReader(context, "SERVICE-A", 10000);

            Assert.Throws<KeyNotFoundException>(() => reader.GetValue<string>("NonExistentKey"));
        }
        
        [Fact]
        public void GetValue_UsesCache_WhenDatabaseIsUnavailable()
        {
            // Arrange: InMemory db context oluştur ve konfigürasyonları ekle
            var context = GetInMemoryDbContext();

            // İlk ConfigurationReader nesnesi, başarılı db erişimiyle cache dolacak
            var reader = new ConfigurationReader.ConfigurationReader(context, "SERVICE-A", 10000);

            // İlk çağrıyla cache hazırlandı
            var siteNameFromDb = reader.GetValue<string>("SiteName");
            Assert.Equal("testsite.com", siteNameFromDb);

            // Şimdi db'deki veriyi sildik ya da context'i boşalttık diyelim (offline simülasyonu)
            context.ConfigurationEntries.RemoveRange(context.ConfigurationEntries);
            context.SaveChanges();

            // Cache’den değer okunmalı; DB boş olsa da hata almamalı
            var siteNameFromCache = reader.GetValue<string>("SiteName");

            // Assert
            Assert.Equal("testsite.com", siteNameFromCache);
        }
    }
}
