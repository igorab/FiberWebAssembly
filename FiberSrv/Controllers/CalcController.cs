using BSFiberCore.Models.BL.Lib;
using FiberSrv.Data;
using FiberSrv.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FiberSrv.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CalcController : ControllerBase
{
    private readonly CalcRepository _repository;

    private readonly MaterialRepository _materialRepository;

    public CalcController(CalcRepository repository, MaterialRepository materialRepository)
    {
        _repository = repository;
        _materialRepository = materialRepository;
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

    [HttpGet("rebar/diameters")]
    public async Task<List<RebarDiameters>?> GetRebarDiametersAsync()
    {       
        return await _materialRepository.LoadRebarDiameters();
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
