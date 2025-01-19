namespace KXlsxConverterAPI.Models;

public class AuthorizedRoutesDto 
{
    public Store[] AuthorizedStores { get; set; }
    public AuthorizedRoutesDto(int[] storeNumbers, int[] divsionNumbers)
    {
        AuthorizedStores = new Store[storeNumbers.Count()];
        for (int i = 0; i < storeNumbers.Count(); i++)
            AuthorizedStores[i] = new (storeNumbers[i], divsionNumbers[i]);
    }
}

public class Store
{
    public int StoreNumber { get; set; } 
    public int DivisionNumber { get; set; }
    public Store(int store, int division)
    {
        StoreNumber = store;
        DivisionNumber = division;
    }

    public Store(string store, string division)
    {
        StoreNumber = int.Parse(store);
        DivisionNumber = int.Parse(division);
    }
}