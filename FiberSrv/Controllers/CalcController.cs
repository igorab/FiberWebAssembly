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

    [HttpGet("RebarDiameters")]
    public async Task<List<RebarDiameters>?> GetRebarDiametersAsync()
    {       
        return await _materialRepository.LoadRebarDiameters();
    }

    [HttpGet("FiberConcreteTable")]
    public async Task<List<Elements>?> GetFiberConcreteTableAsync()
    {
        return await _materialRepository.LoadFiberConcreteTable();
    }

    [HttpGet("Rebar")]
    public async Task<List<Rebar>> LoadRebarAsync()
    {
        return await _materialRepository.LoadRebar();
    }

    [HttpGet("BSFiberBeton")]
    public async Task<List<BSFiberBeton>> LoadBSFiberBetonAsync()
    {
        return await _materialRepository.LoadBSFiberBeton();
    }

    [HttpGet("FiberBft")]
    public async Task<List<FiberBft>> LoadFiberBftAsync()
    {
        return await _materialRepository.LoadFiberBft();
    }

    [HttpGet("BetonData")]
    public async Task<List<Beton>> LoadBetonDataAsync(int _betonTypeId)
    {
        return await _materialRepository.LoadBetonData(_betonTypeId);    
    }

    [HttpGet("HeavyBetonTable")]
    public async Task<Beton> HeavyBetonTableFindAsync(string betonClass, int betonTypeId = 0)
    {
        return await _materialRepository.HeavyBetonTableFind(betonClass,  betonTypeId);
    }


    [HttpGet("CalcParameters")]
    public async Task<IEnumerable<CalcParameters>> Get()
    {
        return await _repository.GetCalcParametersAsync();
    }

    [HttpPost("CalcParameter")]
    public async Task<IActionResult> Post([FromBody] CalcParameters parameters)
    {
        await _repository.AddCalcParameterAsync(parameters);
        return Ok();
    }
}
