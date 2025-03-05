namespace KXlsxConverterAPI.Helpers;

using System;
using System.Net.Http;
using System.Text.Json;
using KXlsxConverterAPI.Models;

public class SpecialDayHelpers 
{
    public static async Task<PublicHoliday[]> GetHolidays(string year) 
    {
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        using var httpClient = new HttpClient();
        using var response = await httpClient.GetAsync($"https://date.nager.at/api/v3/publicholidays/{year}/US");
        if (response.IsSuccessStatusCode)
        {
            using var jsonStream = await response.Content.ReadAsStreamAsync();
            return JsonSerializer.Deserialize<PublicHoliday[]>(jsonStream, jsonSerializerOptions) ?? [];
        } else 
        {
            return [];
        }
    }
}
