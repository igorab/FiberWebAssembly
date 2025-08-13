using Microsoft.AspNetCore.Mvc;
using FiberSrv.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FiberSrv.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CalcController : ControllerBase
{
    private readonly CalcRepository _repository;

    public CalcController(CalcRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("check-connection")]
    public async Task<IActionResult> CheckConnection()
    {
        var isConnected = await _repository.CheckDatabaseConnectionAsync();
        if (isConnected)
        {
            return Ok("Соединение с базой данных успешно установлено.");
        }
        else
        {
            return StatusCode(500, "Не удалось установить соединение с базой данных.");
        }
    }


    [HttpGet]
    public async Task<IEnumerable<CalcParameters>> Get()
    {
        return await _repository.GetCalcParametersAsync();
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CalcParameters parameters)
    {
        await _repository.AddCalcParameterAsync(parameters);
        return Ok();
    }
}
