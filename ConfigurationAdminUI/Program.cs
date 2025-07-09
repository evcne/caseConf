using Microsoft.EntityFrameworkCore;
using ConfigurationReader.Data;
using ConfigurationReader;
using ConfigurationAdminUI.Hubs;


var builder = WebApplication.CreateBuilder(args);

// PostgreSQL DbContext bağlantısı
builder.Services.AddDbContext<ConfigurationDbContext>(options =>
{
    var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connStr);
});

// ConfigurationReader servis olarak ekleniyor
builder.Services.AddScoped<ConfigurationReader.ConfigurationReader>(provider =>
{
    var dbContext = provider.GetRequiredService<ConfigurationDbContext>();
    return new ConfigurationReader.ConfigurationReader(dbContext, "SERVICE-A", 10000);
});

// MVC
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

var app = builder.Build();

// Hata yönetimi
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapHub<ConfigHub>("/confighub");


// Varsayılan route: ConfigurationEntriesController
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=ConfigurationEntries}/{action=Index}/{id?}");

app.Run();
