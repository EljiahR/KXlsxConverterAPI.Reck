using KXlsxConverterAPI.Models;

namespace KXlsxConverterAPI.Repositories
{
    public interface IEmployeeRepository : IGenericRepository<Employee>
    {
        Task<IEnumerable<Employee>> GetAllByDivisionAndStoreNumberAsync(int division, int storeNumber);
        Task DeleteAllByDivisionAndStoreNumberAsync(int division, int storeNumber);
    }
}
