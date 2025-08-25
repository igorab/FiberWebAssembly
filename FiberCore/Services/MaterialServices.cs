using BSFiberCore.Models.BL.Lib;
using System;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;
namespace FiberCore.Services;

public static class MaterialServices
{    
    private readonly static string _url = "https://localhost:7111/api/calc";

    public static async Task<List<RebarDiameters>?> GetRebarDiametersAsync()
    {
        var client = new HttpClient();
        string surl = _url + "/rebar/diameters"; 
        var response = await client.GetAsync(surl); 
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
        var client = new HttpClient();
        var response = await client.GetAsync("https://api.example.com/elements");
        return await response.Content.ReadFromJsonAsync<List<Elements>>();
    }

    internal static async Task<List<BSFiberBeton>> GetBSFiberBetonAsync()
    {
        var client = new HttpClient();
        var response = await client.GetAsync("https://api.example.com/bsfiberbeton");
        return await response.Content.ReadFromJsonAsync<List<BSFiberBeton>>();
    }

    internal static async Task<List<FiberBft>> GetFiberBftAsync()
    {
        var client = new HttpClient();
        var response = await client.GetAsync("https://api.example.com/fiberbft");
        return await response.Content.ReadFromJsonAsync<List<FiberBft>>();
    }

    internal static async Task<List<Beton>> GetBetonDataAsync(int v)
    {
        var client = new HttpClient();
        var response = await client.GetAsync("https://api.example.com/betondata");
        return await response.Content.ReadFromJsonAsync<List<Beton>>();
    }
}
