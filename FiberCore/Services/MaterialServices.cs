using System;
using System.Net.Http.Json;
using BSFiberCore.Models.BL.Lib;
namespace FiberCore.Services;


public static class MaterialServices
{
    public static async Task<List<RebarDiameters>?> GetRebarDiametersAsync()
    {
        var client = new HttpClient();
        var response = await client.GetAsync("https://api.example.com/rebar/diameters");
        return await response.Content.ReadFromJsonAsync < List<RebarDiameters>>();
    }

    public static async Task<List<Rebar>?> GetRebarAsync()
    {
        var client = new HttpClient();
        var response = await client.GetAsync("https://api.example.com/rebar");
        return await response.Content.ReadFromJsonAsync<List<Rebar>>();
    }

    internal static async Task<List<Elements>> GetFiberConcreteTableAsync()
    {
        throw new NotImplementedException();
    }

    internal static async Task<List<BSFiberBeton>> GetBSFiberBetonAsync()
    {
        throw new NotImplementedException();
    }

    internal static async Task<List<FiberBft>> GetFiberBftAsync()
    {
        throw new NotImplementedException();
    }

    internal static async Task<List<Beton>> GetBetonDataAsync(int v)
    {
        throw new NotImplementedException();
    }
}
