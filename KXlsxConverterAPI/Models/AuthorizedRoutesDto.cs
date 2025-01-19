namespace KXlsxConverterAPI.Models;

public class AuthorizedRoutesDto 
{
    public string[] AuthorizedStores { get; set; }
    public AuthorizedRoutesDto(string[] storeNumbers)
    {
        AuthorizedStores = new string[storeNumbers.Count()];
        for (int i = 0; i < storeNumbers.Count(); i++)
            AuthorizedStores[i] = storeNumbers[i];
    }
}