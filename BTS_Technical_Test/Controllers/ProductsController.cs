using BTS_Technical_Test.Data;
using BTS_Technical_Test.DTO;
using BTS_Technical_Test.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace BTS_Technical_Test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public ProductsController(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [HttpGet]
        public IActionResult GetAll([FromQuery] string? search, [FromQuery] string? category, [FromQuery] int limit = 10, [FromQuery] int page = 1)
        {
            var cacheKey = $"products_{search}_{category}_{limit}_{page}";

            if (_cache.TryGetValue(cacheKey, out var cachedData))
                return Ok(cachedData);

            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Title.Contains(search));

            if (!string.IsNullOrEmpty(category))
                query = query.Where(p => p.Category == category);

            var result = query
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
            return Ok(result);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            if (_cache.TryGetValue($"product_{id}", out Product? cachedProduct))
                return Ok(cachedProduct);

            var product = _context.Products.Find(id);
            if (product == null)
                return NotFound(new { message = "Produk tidak ditemukan" });

            _cache.Set($"product_{id}", product, TimeSpan.FromMinutes(5));
            return Ok(product);
        }

        [HttpPost]
        [Authorize]
        [EnableRateLimiting("ProductLimit")]
        public IActionResult Create([FromBody] ProductRequest request)
        {
            var username = User.FindFirstValue(ClaimTypes.Name) ?? "System";
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0";

            var product = new Product
            {
                Title = request.Title,
                Price = request.Price,
                Description = request.Description,
                Category = request.Category,
                Images = request.Images,
                CreatedBy = username,
                CreatedById = userId,
                UpdatedBy = username,
                UpdatedById = userId
            };

            _context.Products.Add(product);
            _context.SaveChanges();
            _cache.Remove($"products_"); // Invalidate cache secara sederhana

            return Ok(product);
        }

        [HttpPut("{id}")]
        [Authorize]
        [EnableRateLimiting("ProductLimit")]
        public IActionResult Update(int id, [FromBody] ProductRequest request)
        {
            var product = _context.Products.Find(id);
            if (product == null)
                return NotFound(new { message = "Produk tidak ditemukan" });

            var username = User.FindFirstValue(ClaimTypes.Name) ?? "System";
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0";

            product.Title = request.Title;
            product.Price = request.Price;
            product.Description = request.Description;
            product.Category = request.Category;
            product.Images = request.Images;
            product.UpdatedAt = DateTime.Now;
            product.UpdatedBy = username;
            product.UpdatedById = userId;

            _context.SaveChanges();
            _cache.Remove($"product_{id}");

            return Ok(product);
        }

        [HttpDelete("{id}")]
        [Authorize]
        [EnableRateLimiting("ProductLimit")]
        public IActionResult Delete(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
                return NotFound(new { message = "Produk tidak ditemukan" });

            _context.Products.Remove(product);
            _context.SaveChanges();
            _cache.Remove($"product_{id}");

            return Ok(new { message = "Produk berhasil dihapus" });
        }
    }
}