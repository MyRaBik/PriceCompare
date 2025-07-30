using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace PriCom.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        // Выполняет поиск товаров (с парсингом и сохранением запроса)
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Запрос не может быть пустым.");

            var result = await _productService.SearchProductAsync(query);
            return Ok(result);
        }

        // Получает историю цен по товару
        [HttpGet("{id}/history")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductHistory(int id)
        {
            var result = await _productService.GetPriceHistoryAsync(id);
            return result == null ? NotFound($"Товар с ID {id} не найден.") : Ok(result);
        }
    }
}
