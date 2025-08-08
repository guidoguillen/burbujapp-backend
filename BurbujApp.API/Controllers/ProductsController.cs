using BurbujApp.Domain.Entities;
using BurbujApp.Persistence.Context;
using BurbujApp.Shared.DTOs.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BurbujApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly BurbujAppDbContext _context;

    public ProductsController(BurbujAppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
    {
        var products = await _context.Products
            .Where(p => p.IsActive)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Brand = p.Brand,
                Category = p.Category,
                Price = p.Price,
                Stock = p.Stock,
                MinimumStock = p.MinimumStock,
                BarCode = p.BarCode,
                IsActive = p.IsActive
            })
            .ToListAsync();

        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return NotFound();

        var productDto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Brand = product.Brand,
            Category = product.Category,
            Price = product.Price,
            Stock = product.Stock,
            MinimumStock = product.MinimumStock,
            BarCode = product.BarCode,
            IsActive = product.IsActive
        };

        return Ok(productDto);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto createProductDto)
    {
        // Verificar si el código de barras ya existe
        if (!string.IsNullOrEmpty(createProductDto.BarCode))
        {
            var existingProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.BarCode == createProductDto.BarCode);
            
            if (existingProduct != null)
                return BadRequest("Ya existe un producto con este código de barras");
        }

        var product = new Product
        {
            Name = createProductDto.Name,
            Description = createProductDto.Description,
            Brand = createProductDto.Brand,
            Category = createProductDto.Category,
            Price = createProductDto.Price,
            Stock = createProductDto.Stock,
            MinimumStock = createProductDto.MinimumStock,
            BarCode = createProductDto.BarCode
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Registrar movimiento de inventario inicial si hay stock
        if (product.Stock > 0)
        {
            var movement = new InventoryMovement
            {
                ProductId = product.Id,
                Type = MovementType.Purchase,
                Quantity = product.Stock,
                Reason = "Stock inicial",
                CreatedBy = User.FindFirst(ClaimTypes.Email)?.Value ?? "Sistema"
            };

            _context.InventoryMovements.Add(movement);
            await _context.SaveChangesAsync();
        }

        var productDto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Brand = product.Brand,
            Category = product.Category,
            Price = product.Price,
            Stock = product.Stock,
            MinimumStock = product.MinimumStock,
            BarCode = product.BarCode,
            IsActive = product.IsActive
        };

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, productDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, UpdateProductDto updateProductDto)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return NotFound();

        // Verificar código de barras si se está cambiando
        if (!string.IsNullOrEmpty(updateProductDto.BarCode) && updateProductDto.BarCode != product.BarCode)
        {
            var existingProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.BarCode == updateProductDto.BarCode && p.Id != id);
            
            if (existingProduct != null)
                return BadRequest("Ya existe un producto con este código de barras");
        }

        product.Name = updateProductDto.Name;
        product.Description = updateProductDto.Description;
        product.Brand = updateProductDto.Brand;
        product.Category = updateProductDto.Category;
        product.Price = updateProductDto.Price;
        product.MinimumStock = updateProductDto.MinimumStock;
        product.BarCode = updateProductDto.BarCode;
        product.IsActive = updateProductDto.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id}/inventory-movement")]
    public async Task<IActionResult> CreateInventoryMovement(int id, CreateInventoryMovementDto movementDto)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return NotFound();

        // Validar que el movimiento no deje stock negativo
        if (movementDto.Type == MovementType.Sale || movementDto.Type == MovementType.Adjustment)
        {
            if (movementDto.Quantity > product.Stock)
                return BadRequest("No hay suficiente stock disponible");
        }

        var movement = new InventoryMovement
        {
            ProductId = id,
            Type = movementDto.Type,
            Quantity = movementDto.Quantity,
            Reason = movementDto.Reason,
            Reference = movementDto.Reference,
            CreatedBy = User.FindFirst(ClaimTypes.Email)?.Value ?? "Sistema"
        };

        // Actualizar el stock del producto
        switch (movementDto.Type)
        {
            case MovementType.Purchase:
            case MovementType.Return:
                product.Stock += movementDto.Quantity;
                break;
            case MovementType.Sale:
            case MovementType.Adjustment:
                product.Stock -= movementDto.Quantity;
                break;
        }

        product.UpdatedAt = DateTime.UtcNow;

        _context.InventoryMovements.Add(movement);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Movimiento registrado exitosamente", newStock = product.Stock });
    }

    [HttpGet("{id}/movements")]
    public async Task<ActionResult<IEnumerable<InventoryMovementDto>>> GetProductMovements(int id)
    {
        var movements = await _context.InventoryMovements
            .Include(m => m.Product)
            .Where(m => m.ProductId == id)
            .Select(m => new InventoryMovementDto
            {
                Id = m.Id,
                ProductId = m.ProductId,
                ProductName = m.Product.Name,
                Type = m.Type,
                Quantity = m.Quantity,
                Reason = m.Reason,
                Reference = m.Reference,
                CreatedAt = m.CreatedAt,
                CreatedBy = m.CreatedBy
            })
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        return Ok(movements);
    }

    [HttpGet("low-stock")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetLowStockProducts()
    {
        var products = await _context.Products
            .Where(p => p.IsActive && p.Stock <= p.MinimumStock)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Brand = p.Brand,
                Category = p.Category,
                Price = p.Price,
                Stock = p.Stock,
                MinimumStock = p.MinimumStock,
                BarCode = p.BarCode,
                IsActive = p.IsActive
            })
            .ToListAsync();

        return Ok(products);
    }
}
