using BSFiberCore.Models.BL.Lib;
using System;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;
namespace FiberCore.Services;

public static class MaterialServices
{    
    private readonly static string _url = "https://localhost:7111/api/calc";
    public static HttpClient httpClient { get; set; } 


    public static List<RebarDiameters>? GetRebarDiameters()
    {
        List<RebarDiameters>? diameters = new List<RebarDiameters>();

        try
        {
            string surl = _url + "/rebar/diameters";

            using (var client = new HttpClient())
            {
                HttpResponseMessage response = client.GetAsync(surl).GetAwaiter().GetResult();

                if (response.IsSuccessStatusCode)
                {
                    diameters = response.Content.ReadFromJsonAsync<List<RebarDiameters>>().GetAwaiter().GetResult();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
            // Логирование или другие действия по обработке ошибок
        }

        return diameters;
    }


    public static async Task<List<RebarDiameters>?> GetRebarDiametersAsync()
    {
        List<RebarDiameters>? diameters = new List<RebarDiameters>();

        try
        {
            string surl = _url + "/rebar/diameters";
            HttpResponseMessage response = await httpClient.GetAsync(surl);

            if (response.IsSuccessStatusCode)
            {
                diameters = await response.Content.ReadFromJsonAsync<List<RebarDiameters>>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
            // Логирование или другие действия по обработке ошибок
        }

        return diameters;
    }

    public static async Task<List<Rebar>?> GetRebarAsync()
    {        
        string surl = _url + "/rebar";
        var response = await httpClient.GetAsync(surl);
        return await response.Content.ReadFromJsonAsync<List<Rebar>>();
    }

    internal static async Task<List<Elements>> GetFiberConcreteTableAsync()
    {        
        var response = await httpClient.GetAsync(_url + "/elements");
        return await response.Content.ReadFromJsonAsync<List<Elements>>();
    }

    internal static async Task<List<BSFiberBeton>> GetBSFiberBetonAsync()
    {        
        var response = await httpClient.GetAsync(_url + "/bsfiberbeton");
        return await response.Content.ReadFromJsonAsync<List<BSFiberBeton>>();
    }

    internal static async Task<List<FiberBft>> GetFiberBftAsync()
    {
        var response = await httpClient.GetAsync(_url + "/fiberbft");
        return await response.Content.ReadFromJsonAsync<List<FiberBft>>();
    }

    internal static async Task<List<Beton>> GetBetonDataAsync(int v)
    {        
        var response = await httpClient.GetAsync(_url + "/betondata");
        return await response.Content.ReadFromJsonAsync<List<Beton>>();
    }
}
