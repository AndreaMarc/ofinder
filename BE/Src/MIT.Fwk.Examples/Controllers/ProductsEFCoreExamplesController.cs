using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MIT.Fwk.Examples.Data;
using MIT.Fwk.Examples.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MIT.Fwk.Examples.Controllers
{
    /// <summary>
    /// ESEMPI PRATICI: Query Database con EF Core DbContext (LINQ)
    ///
    /// Questo controller dimostra tutte le tecniche principali per query con EF Core:
    /// - AsNoTracking (performance per read-only)
    /// - Include/ThenInclude (Eager Loading per evitare N+1)
    /// - Projection (Select solo campi necessari)
    /// - Filtering, Ordering, Paging
    /// - GroupBy e Aggregazioni
    /// - FirstOrDefault vs Find
    ///
    /// FILE CORRELATI:
    /// - Entities/Examples/Product.cs
    /// - Entities/Examples/Review.cs
    /// - Data/OtherDbContext.cs
    /// </summary>
    [Route("api/examples/efcore")]
    public class ProductsEFCoreExamplesController : ControllerBase
    {
        private readonly OtherDbContext _context;

        public ProductsEFCoreExamplesController(
            OtherDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// ESEMPIO 1: Query semplice con AsNoTracking
        /// QUANDO USARE: Sempre per query read-only (migliori performance)
        /// </summary>
        [HttpGet("simple")]
        public async Task<IActionResult> Example01_SimpleQuery()
        {
            var products = await _context.ExampleProducts
                .AsNoTracking()  // Non traccia le entità = più veloce
                .ToListAsync();

            return Ok(new
            {
                Count = products.Count,
                Products = products
            });
        }

        /// <summary>
        /// ESEMPIO 2: Filtro e Ordinamento
        /// QUANDO USARE: Ricerche con criteri dinamici
        /// </summary>
        [HttpGet("filtered")]
        public async Task<IActionResult> Example02_FilteredQuery(
            [FromQuery] decimal? minPrice,
            [FromQuery] string searchTerm,
            [FromQuery] bool? isActive)
        {
            var query = _context.ExampleProducts.AsNoTracking();

            // Filtri dinamici
            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (!string.IsNullOrEmpty(searchTerm))
                query = query.Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm));

            if (isActive.HasValue)
                query = query.Where(p => p.IsActive == isActive.Value);

            // Ordinamento
            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .ThenBy(p => p.Name)
                .ToListAsync();

            return Ok(products);
        }

        /// <summary>
        /// ESEMPIO 3: Include per Eager Loading (evita problema N+1)
        /// QUANDO USARE: Quando serve caricare relazioni insieme all'entità principale
        /// ATTENZIONE: Non abusarne, carica solo ciò che serve!
        /// </summary>
        [HttpGet("with-relations")]
        public async Task<IActionResult> Example03_EagerLoading()
        {
            var products = await _context.ExampleProducts
                .Include(p => p.Category)              // Carica la categoria
                .AsNoTracking()
                .Take(10)  // Limita risultati per performance
                .ToListAsync();

            return Ok(products);
        }

        /// <summary>
        /// ESEMPIO 4: Projection con Select (ottimizzazione performance)
        /// QUANDO USARE: Quando serve solo un sottoinsieme di dati
        /// VANTAGGIO: Trasferisce meno dati dal database
        /// </summary>
        [HttpGet("summary")]
        public async Task<IActionResult> Example04_Projection()
        {
            var summary = await _context.ExampleProducts
                .AsNoTracking()
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    p.Stock,
                    CategoryName = p.Category != null ? p.Category.Name : "Nessuna"
                })
                .ToListAsync();

            return Ok(summary);
        }

        /// <summary>
        /// ESEMPIO 5: Paginazione
        /// QUANDO USARE: Liste grandi che non possono essere caricate tutte insieme
        /// </summary>
        [HttpGet("paged")]
        public async Task<IActionResult> Example05_Pagination(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var totalItems = await _context.ExampleProducts.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var products = await _context.ExampleProducts
                .AsNoTracking()
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalItems = totalItems,
                Items = products
            });
        }

        /// <summary>
        /// ESEMPIO 6: GroupBy e Aggregazioni
        /// QUANDO USARE: Report, statistiche, dashboard
        /// </summary>
        [HttpGet("stats-by-category")]
        public async Task<IActionResult> Example06_GroupBy()
        {
            var stats = await _context.ExampleProducts
                .AsNoTracking()
                .GroupBy(p => p.Category.Name)
                .Select(g => new
                {
                    Category = g.Key ?? "Senza Categoria",
                    ProductCount = g.Count(),
                    TotalStock = g.Sum(p => p.Stock),
                    AveragePrice = g.Average(p => p.Price),
                    MinPrice = g.Min(p => p.Price),
                    MaxPrice = g.Max(p => p.Price)
                })
                .OrderByDescending(x => x.ProductCount)
                .ToListAsync();

            return Ok(stats);
        }

        /// <summary>
        /// ESEMPIO 7: Find vs FirstOrDefault
        /// QUANDO USARE Find: Solo per ricerca per chiave primaria (più veloce)
        /// QUANDO USARE FirstOrDefault: Per qualsiasi altro filtro
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> Example07_FindVsFirstOrDefault(int id)
        {
            // METODO 1: Find (ottimizzato per PK, controlla prima la cache)
            var productFind = await _context.ExampleProducts.FindAsync(id);

            // METODO 2: FirstOrDefault (sempre va al database)
            var productFirst = await _context.ExampleProducts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            // METODO 3: SingleOrDefault (lancia eccezione se ci sono duplicati)
            // var productSingle = await _context.ExampleProducts.SingleOrDefaultAsync(p => p.Id == id);

            if (productFind == null)
                return NotFound();

            return Ok(productFind);
        }

        /// <summary>
        /// ESEMPIO 8: Any e Count per esistenza/conteggio
        /// QUANDO USARE Any: Quando serve solo sapere se esiste (più veloce di Count > 0)
        /// </summary>
        [HttpGet("check-availability/{id}")]
        public async Task<IActionResult> Example08_AnyAndCount(int id)
        {
            // Any è più veloce di Count quando serve solo sapere se esiste
            var exists = await _context.ExampleProducts.AnyAsync(p => p.Id == id);
            var hasStock = await _context.ExampleProducts.AnyAsync(p => p.Id == id && p.Stock > 0);

            // Count per ottenere il numero
            var totalProducts = await _context.ExampleProducts.CountAsync();
            var activeProducts = await _context.ExampleProducts.CountAsync(p => p.IsActive);

            return Ok(new
            {
                Exists = exists,
                HasStock = hasStock,
                TotalProducts = totalProducts,
                ActiveProducts = activeProducts
            });
        }

        /// <summary>
        /// ESEMPIO 9: Query complessa con Join multipli
        /// QUANDO USARE: Report complessi, dashboard
        /// </summary>
        [HttpGet("complex-report")]
        public async Task<IActionResult> Example09_ComplexQuery()
        {
            var report = await _context.ExampleProducts
                .AsNoTracking()
                .Where(p => p.IsActive)
                .Include(p => p.Category)
                .Select(p => new
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    Price = p.Price,
                    Stock = p.Stock,
                    Category = p.Category != null ? p.Category.Name : "N/A",
                    CreatedAt = p.CreatedAt
                })
                .OrderByDescending(x => x.CreatedAt)
                .Take(20)
                .ToListAsync();

            return Ok(report);
        }

        /// <summary>
        /// ESEMPIO 10: Update con EF Core
        /// PATTERN CONSIGLIATO: Find -> Modifica -> SaveChanges
        /// </summary>
        [HttpPatch("{id}/update-price")]
        public async Task<IActionResult> Example10_Update(int id, [FromQuery] decimal newPrice)
        {
            // 1. Find dell'entità (con tracking)
            var product = await _context.ExampleProducts.FindAsync(id);

            if (product == null)
                return NotFound();

            // 2. Modifica l'entità
            product.Price = newPrice;
            product.UpdatedAt = DateTime.UtcNow;

            // 3. Salva le modifiche
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Success = true,
                ProductId = product.Id,
                NewPrice = product.Price
            });
        }
    }
}
