using BSFiberCore.Models.BL.Lib;
using System;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;
namespace FiberCore.Services;

public static class MaterialServices
{    
    private readonly static string _url = "https://localhost:7111/api/calc/";
    public static HttpClient httpClient { get; set; } 

    public static List<RebarDiameters>? GetRebarDiameters()
    {
        List<RebarDiameters>? diameters = new List<RebarDiameters>();

        try
        {
            string surl = _url + "RebarDiameters";

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
            string surl = _url + "RebarDiameters";
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
        string surl = _url + "Rebar";
        var response = await httpClient.GetAsync(surl);
        return await response.Content.ReadFromJsonAsync<List<Rebar>>();
    }

    internal static async Task<List<Elements>> GetFiberConcreteTableAsync()
    {        
        var response = await httpClient.GetAsync(_url + "FiberConcreteTable");
        return await response.Content.ReadFromJsonAsync<List<Elements>>();
    }

    internal static async Task<List<BSFiberBeton>> GetBSFiberBetonAsync()
    {        
        var response = await httpClient.GetAsync(_url + "BSFiberBeton");
        return await response.Content.ReadFromJsonAsync<List<BSFiberBeton>>();
    }

    internal static async Task<List<FiberBft>> GetFiberBftAsync()
    {
        var response = await httpClient.GetAsync(_url + "FiberBft");
        return await response.Content.ReadFromJsonAsync<List<FiberBft>>();
    }

    internal static async Task<List<Beton>> GetBetonDataAsync(int v)
    {        
        var response = await httpClient.GetAsync(_url + "BetonData");
        return await response.Content.ReadFromJsonAsync<List<Beton>>();
    }

    public static async Task<Beton> HeavyBetonTableFindAsync(string betonClass, int betonTypeId = 0)
    {
        try
        {
            // Формируем строку запроса с параметрами
            var queryString = $"?betonClass={Uri.EscapeDataString(betonClass)}&betonTypeId={betonTypeId}";
            var response = await httpClient.GetAsync(_url + "HeavyBetonTable" + queryString);

            // Проверяем успешность ответа
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Beton>();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
            return new Beton();
        }
    }


}
