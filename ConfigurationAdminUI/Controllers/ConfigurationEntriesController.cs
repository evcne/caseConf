using Microsoft.AspNetCore.Mvc;
using ConfigurationReader.Data;
using ConfigurationReader.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using ConfigurationAdminUI.Hubs;


namespace ConfigurationAdminUI.Controllers
{
    public class ConfigurationEntriesController : Controller
    {
        private readonly ConfigurationDbContext _context;
        private readonly IHubContext<ConfigHub> _hubContext;
        private readonly string _applicationName;
        private readonly ConfigurationReader.ConfigurationReader _reader;

        public ConfigurationEntriesController(ConfigurationDbContext context,
        IHubContext<ConfigHub> hubContext,
        ConfigurationReader.ConfigurationReader reader)
        {
            _context = context;
            _hubContext = hubContext;
            _reader = reader;
            _applicationName = reader.ApplicationName;

        }

        public async Task<IActionResult> Index(string search = "")
        {
            var entries = await _context.ConfigurationEntries
                .Where(e => e.Name.Contains(search) && e.IsActive && e.ApplicationName == _applicationName)
                .ToListAsync();
            return View(entries);
        }

        public async Task<IActionResult> Update(ConfigurationEntry entry)
        {
            // Veritabanı güncelleme işlemleri
            _context.Update(entry);
            await _context.SaveChangesAsync();

            // SignalR ile clientlara haber ver
            await _hubContext.Clients.All.SendAsync("ConfigUpdated");

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ConfigurationEntry entry)
        {
            if (ModelState.IsValid)
            {
                _context.Add(entry);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(entry);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var entry = await _context.ConfigurationEntries.FindAsync(id);
            if (entry == null) return NotFound();
            return View(entry);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ConfigurationEntry entry)
        {
            if (id != entry.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var local = _context.ConfigurationEntries.Local
                    .FirstOrDefault(e => e.Id == entry.Id);

                    if (local != null)
                    {
                        _context.Entry(local).State = EntityState.Detached;
                    }

                    _context.Entry(entry).State = EntityState.Modified;
                //_context.Update(entry);
                await _context.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("ConfigurationChanged", entry.Name);
                return RedirectToAction(nameof(Index));
            }
            return View(entry);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var entry = await _context.ConfigurationEntries
                .FirstOrDefaultAsync(m => m.Id == id);

            if (entry == null) return NotFound();

            return View(entry);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entry = await _context.ConfigurationEntries.FindAsync(id);
            if (entry != null)
            {
                _context.ConfigurationEntries.Remove(entry);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

    }

    
}
