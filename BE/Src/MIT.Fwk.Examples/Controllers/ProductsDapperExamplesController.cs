using Dapper;
using Microsoft.AspNetCore.Mvc;
using MIT.Fwk.Core.Services;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MIT.Fwk.Examples.Controllers
{
    /// <summary>
    /// ESEMPI PRATICI: Query Database con Dapper (Micro-ORM)
    ///
    /// QUANDO USARE DAPPER:
    /// - Query complesse con JOIN multipli
    /// - Performance critiche (più veloce di EF Core)
    /// - Query SQL raw ottimizzate
    /// - Stored procedures
    /// - Bulk operations
    ///
    /// VANTAGGI:
    /// - Molto più veloce di EF Core (fino a 3x)
    /// - Controllo totale sulla query SQL
    /// - Leggero e semplice
    ///
    /// SVANTAGGI:
    /// - Nessun change tracking
    /// - Query SQL scritte a mano
    /// - Nessuna validazione compile-time
    ///
    /// FILE CORRELATI:
    /// - Questo controller usa la stessa struttura DB di ProductsEFCoreExamplesController
    /// </summary>
    [Route("api/examples/dapper")]
    public class ProductsDapperExamplesController : ControllerBase
    {
        private readonly IConnectionStringProvider _connStringProvider;

        public ProductsDapperExamplesController(
            IConnectionStringProvider connStringProvider)
        {
            _connStringProvider = connStringProvider;
        }

        /// <summary>
        /// ESEMPIO 1: Query SELECT semplice
        /// Dapper mappa automaticamente le colonne alle proprietà
        /// </summary>
        [HttpGet("simple")]
        public async Task<IActionResult> Example01_SimpleQuery()
        {
            using var connection = new MySqlConnection(
                _connStringProvider.GetConnectionString("OtherDbContext")
            );

            var sql = "SELECT * FROM ExampleProducts WHERE IsActive = @IsActive LIMIT 50";

            var products = await connection.QueryAsync<ProductDto>(sql, new { IsActive = true });

            return Ok(new
            {
                Count = products.Count(),
                Products = products
            });
        }

        /// <summary>
        /// ESEMPIO 2: Query con parametri multipli e LIKE
        /// IMPORTANTE: Sempre usare parametri per prevenire SQL Injection
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> Example02_SearchWithParameters(
            [FromQuery] string keyword,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice)
        {
            using var connection = new MySqlConnection(
                _connStringProvider.GetConnectionString("OtherDbContext")
            );

            var sql = @"
                SELECT * FROM ExampleProducts
                WHERE (@Keyword IS NULL OR Name LIKE @KeywordPattern OR Description LIKE @KeywordPattern)
                  AND (@MinPrice IS NULL OR Price >= @MinPrice)
                  AND (@MaxPrice IS NULL OR Price <= @MaxPrice)
                ORDER BY Name
                LIMIT 100
            ";

            var products = await connection.QueryAsync<ProductDto>(sql, new
            {
                Keyword = keyword,
                KeywordPattern = $"%{keyword}%",
                MinPrice = minPrice,
                MaxPrice = maxPrice
            });

            return Ok(products);
        }

        /// <summary>
        /// ESEMPIO 3: Query con JOIN (One-to-Many mapping)
        /// Dapper supporta il mapping automatico con splitOn
        /// </summary>
        [HttpGet("with-category")]
        public async Task<IActionResult> Example03_JoinQuery()
        {
            using var connection = new MySqlConnection(
                _connStringProvider.GetConnectionString("OtherDbContext")
            );

            var sql = @"
                SELECT
                    p.Id, p.Name, p.Description, p.Price, p.Stock, p.IsActive,
                    c.Id, c.Name, c.Description
                FROM ExampleProducts p
                LEFT JOIN ExampleCategories c ON p.CategoryId = c.Id
                WHERE p.IsActive = 1
                LIMIT 50
            ";

            var productDict = new Dictionary<int, ProductWithCategoryDto>();

            var products = await connection.QueryAsync<ProductWithCategoryDto, CategoryDto, ProductWithCategoryDto>(
                sql,
                (product, category) =>
                {
                    if (!productDict.TryGetValue(product.Id, out var productEntry))
                    {
                        productEntry = product;
                        productEntry.Category = category;
                        productDict.Add(productEntry.Id, productEntry);
                    }
                    return productEntry;
                },
                splitOn: "Id"  // Dapper splitta qui: prima parte = Product, seconda = Category
            );

            return Ok(productDict.Values);
        }

        /// <summary>
        /// ESEMPIO 4: QueryFirstOrDefault (ritorna 1 risultato o null)
        /// QUANDO USARE:
        /// - FirstOrDefault: Ritorna il primo, ok se ci sono duplicati
        /// - SingleOrDefault: Lancia eccezione se ci sono duplicati (più sicuro)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> Example04_GetById(int id)
        {
            using var connection = new MySqlConnection(
                _connStringProvider.GetConnectionString("OtherDbContext")
            );

            var sql = "SELECT * FROM ExampleProducts WHERE Id = @Id";

            var product = await connection.QueryFirstOrDefaultAsync<ProductDto>(sql, new { Id = id });

            if (product == null)
                return NotFound();

            return Ok(product);
        }

        /// <summary>
        /// ESEMPIO 5: ExecuteScalar (ritorna un singolo valore)
        /// QUANDO USARE: COUNT, SUM, AVG, MAX, MIN
        /// </summary>
        [HttpGet("stats/count")]
        public async Task<IActionResult> Example05_Scalar()
        {
            using var connection = new MySqlConnection(
                _connStringProvider.GetConnectionString("OtherDbContext")
            );

            var totalCount = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM ExampleProducts"
            );

            var activeCount = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM ExampleProducts WHERE IsActive = 1"
            );

            var avgPrice = await connection.ExecuteScalarAsync<decimal>(
                "SELECT AVG(Price) FROM ExampleProducts WHERE IsActive = 1"
            );

            return Ok(new
            {
                TotalProducts = totalCount,
                ActiveProducts = activeCount,
                AveragePrice = Math.Round(avgPrice, 2)
            });
        }

        /// <summary>
        /// ESEMPIO 6: INSERT e ritorna ID generato
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Example06_Insert([FromBody] CreateProductDto dto)
        {
            using var connection = new MySqlConnection(
                _connStringProvider.GetConnectionString("OtherDbContext")
            );

            var sql = @"
                INSERT INTO ExampleProducts (Name, Description, Price, Stock, IsActive, CategoryId, CreatedAt)
                VALUES (@Name, @Description, @Price, @Stock, @IsActive, @CategoryId, @CreatedAt);
                SELECT LAST_INSERT_ID();
            ";

            var newId = await connection.ExecuteScalarAsync<int>(sql, new
            {
                dto.Name,
                dto.Description,
                dto.Price,
                dto.Stock,
                IsActive = true,
                dto.CategoryId,
                CreatedAt = DateTime.UtcNow
            });

            return Ok(new
            {
                Success = true,
                ProductId = newId
            });
        }

        /// <summary>
        /// ESEMPIO 7: UPDATE
        /// ExecuteAsync ritorna il numero di righe modificate
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Example07_Update(int id, [FromBody] UpdateProductDto dto)
        {
            using var connection = new MySqlConnection(
                _connStringProvider.GetConnectionString("OtherDbContext")
            );

            var sql = @"
                UPDATE ExampleProducts
                SET Name = @Name,
                    Description = @Description,
                    Price = @Price,
                    Stock = @Stock,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id
            ";

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                Id = id,
                dto.Name,
                dto.Description,
                dto.Price,
                dto.Stock,
                UpdatedAt = DateTime.UtcNow
            });

            if (rowsAffected == 0)
                return NotFound();

            return Ok(new { Success = true, RowsAffected = rowsAffected });
        }

        /// <summary>
        /// ESEMPIO 8: DELETE
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Example08_Delete(int id)
        {
            using var connection = new MySqlConnection(
                _connStringProvider.GetConnectionString("OtherDbContext")
            );

            var sql = "DELETE FROM ExampleProducts WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });

            if (rowsAffected == 0)
                return NotFound();

            return Ok(new { Success = true });
        }

        /// <summary>
        /// ESEMPIO 9: Query multipla (Multiple Result Sets)
        /// VANTAGGIO: Una sola chiamata al database per più query
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> Example09_MultipleQueries()
        {
            using var connection = new MySqlConnection(
                _connStringProvider.GetConnectionString("OtherDbContext")
            );

            var sql = @"
                SELECT COUNT(*) FROM ExampleProducts WHERE IsActive = 1;
                SELECT SUM(Stock) FROM ExampleProducts WHERE IsActive = 1;
                SELECT AVG(Price) FROM ExampleProducts WHERE IsActive = 1;
                SELECT * FROM ExampleProducts WHERE IsActive = 1 ORDER BY Price DESC LIMIT 5;
            ";

            using var multi = await connection.QueryMultipleAsync(sql);

            var totalProducts = await multi.ReadSingleAsync<int>();
            var totalStock = await multi.ReadSingleAsync<int>();
            var avgPrice = await multi.ReadSingleAsync<decimal>();
            var topProducts = await multi.ReadAsync<ProductDto>();

            return Ok(new
            {
                TotalProducts = totalProducts,
                TotalStock = totalStock,
                AveragePrice = Math.Round(avgPrice, 2),
                TopExpensiveProducts = topProducts
            });
        }

        /// <summary>
        /// ESEMPIO 10: Bulk Insert (molto più veloce di INSERT singoli)
        /// QUANDO USARE: Import di molti dati (migliaia di record)
        /// </summary>
        [HttpPost("bulk")]
        public async Task<IActionResult> Example10_BulkInsert([FromBody] List<CreateProductDto> products)
        {
            if (!products.Any())
                return BadRequest("Nessun prodotto da inserire");

            using var connection = new MySqlConnection(
                _connStringProvider.GetConnectionString("OtherDbContext")
            );

            var sql = @"
                INSERT INTO ExampleProducts (Name, Description, Price, Stock, IsActive, CategoryId, CreatedAt)
                VALUES (@Name, @Description, @Price, @Stock, @IsActive, @CategoryId, @CreatedAt)
            ";

            var parameters = products.Select(p => new
            {
                p.Name,
                p.Description,
                p.Price,
                p.Stock,
                IsActive = true,
                p.CategoryId,
                CreatedAt = DateTime.UtcNow
            });

            var rowsInserted = await connection.ExecuteAsync(sql, parameters);

            return Ok(new
            {
                Success = true,
                RowsInserted = rowsInserted
            });
        }
    }

    // ===== DTO CLASSES =====

    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
        public int? CategoryId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class ProductWithCategoryDto : ProductDto
    {
        public CategoryDto Category { get; set; }
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class CreateProductDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int? CategoryId { get; set; }
    }

    public class UpdateProductDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}
