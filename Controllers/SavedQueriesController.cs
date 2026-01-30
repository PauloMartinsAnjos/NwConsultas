using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NwConsultas.Database;
using NwConsultas.Models.QueryBuilder;
using NwConsultas.Models.ViewModels;

namespace NwConsultas.Controllers
{
    /// <summary>
    /// Controller para gerenciamento de queries salvas
    /// </summary>
    public class SavedQueriesController : Controller
    {
        private readonly NwConsultasDbContext _context;
        private readonly ILogger<SavedQueriesController> _logger;

        public SavedQueriesController(NwConsultasDbContext context, ILogger<SavedQueriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Lista todas as queries salvas
        /// </summary>
        public async Task<IActionResult> Index(string? searchTerm, int page = 1, int pageSize = 20)
        {
            var query = _context.SavedQueries.Where(q => q.IsActive);

            // Filtrar por termo de busca
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(q => 
                    q.Name.Contains(searchTerm) || 
                    (q.Description != null && q.Description.Contains(searchTerm)));
            }

            var totalQueries = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalQueries / (double)pageSize);

            var queries = await query
                .OrderByDescending(q => q.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new SavedQueriesViewModel
            {
                Queries = queries,
                SearchTerm = searchTerm,
                TotalQueries = totalQueries,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return View(viewModel);
        }

        /// <summary>
        /// Detalhes de uma query salva, incluindo histórico
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            var query = await _context.SavedQueries
                .Include(q => q.Executions.OrderByDescending(e => e.ExecutedAt).Take(50))
                .Include(q => q.Exports.OrderByDescending(e => e.ExportedAt).Take(20))
                .FirstOrDefaultAsync(q => q.Id == id);

            if (query == null)
            {
                return NotFound();
            }

            // Deserializar query definition para exibição
            var queryDefinition = JsonConvert.DeserializeObject<QueryDefinition>(query.QueryJson);
            ViewBag.QueryDefinition = queryDefinition;

            return View(query);
        }

        /// <summary>
        /// Duplicar uma query existente
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Duplicate(int id)
        {
            var original = await _context.SavedQueries.FindAsync(id);
            if (original == null)
            {
                return NotFound();
            }

            var duplicate = new Models.Database.SavedQuery
            {
                Name = $"{original.Name} (Cópia)",
                Description = original.Description,
                QueryJson = original.QueryJson,
                SqlGenerated = original.SqlGenerated,
                CreatedBy = "system"
            };

            _context.SavedQueries.Add(duplicate);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = duplicate.Id });
        }

        /// <summary>
        /// Excluir query (soft delete)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var query = await _context.SavedQueries.FindAsync(id);
            if (query == null)
            {
                return NotFound();
            }

            query.IsActive = false;
            query.UpdatedAt = DateTime.Now;
            
            _context.SavedQueries.Update(query);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Carregar query para edição no Query Builder
        /// </summary>
        public async Task<IActionResult> Edit(int id)
        {
            var query = await _context.SavedQueries.FindAsync(id);
            if (query == null)
            {
                return NotFound();
            }

            // Redirecionar para Query Builder com ID da query
            return RedirectToAction("Index", "QueryBuilder", new { id });
        }
    }
}
