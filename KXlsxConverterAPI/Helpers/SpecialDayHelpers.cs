namespace KXlsxConverterAPI.Helpers;

using System;
using System.Net.Http;
using System.Text.Json;

public class SpecialDayHelpers 
{
    public static async Task<PublicHoliday[]?> GetHolidays() 
    {
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        using var httpClient = new HttpClient();
        using var response = await httpClient.GetAsync("https://date.nager.at/api/v3/publicholidays/2022/US");
        if (response.IsSuccessStatusCode)
        {
            using var jsonStream = await response.Content.ReadAsStreamAsync();
            return JsonSerializer.Deserialize<PublicHoliday[]>(jsonStream, jsonSerializerOptions);
        } else 
        {
            return null;
        }
    }
}

public class PublicHoliday
{
    public DateTime Date { get; set; }
    public string LocalName { get; set; }
    public string Name { get; set; }
    public string CountryCode { get; set; }
    public bool Fixed { get; set; }
    public bool Global { get; set; }
    public string[] Counties { get; set; }
    public int? LaunchYear { get; set; }
    public string[] Types { get; set; }
}
