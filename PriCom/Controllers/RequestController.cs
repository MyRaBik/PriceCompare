using Domain.DTOs;
using Microsoft.AspNetCore.Mvc;
using Application.Interfaces.Services;

namespace PriCom.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestController : ControllerBase
    {
        private readonly IRequestService _service;

        public RequestController(IRequestService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var request = await _service.GetByIdAsync(id);
            return request == null ? NotFound($"Запрос с ID {id} не найден.") : Ok(request);
        }

        [HttpGet("{id}/products")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductsByRequestId(int id)
        {
            var products = await _service.GetProductsByRequestIdAsync(id);
            return products == null || !products.Any()
                ? NotFound($"Товары по запросу с id={id} не найдены.")
                : Ok(products);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] RequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var newId = await _service.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = newId }, dto);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var request = await _service.GetByIdAsync(id);
            if (request == null) return NotFound($"Запрос с ID {id} не найден.");

            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
