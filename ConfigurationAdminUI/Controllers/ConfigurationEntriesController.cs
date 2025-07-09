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

        public ConfigurationEntriesController(ConfigurationDbContext context, IHubContext<ConfigHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;

        }

        public async Task<IActionResult> Index(string search = "")
        {
            var entries = await _context.ConfigurationEntries
                .Where(e => e.Name.Contains(search) && e.IsActive)
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
                _context.Update(entry);
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
