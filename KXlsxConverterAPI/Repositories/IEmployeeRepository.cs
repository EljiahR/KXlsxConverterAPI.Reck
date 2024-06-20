using KXlsxConverterAPI.Models;

namespace KXlsxConverterAPI.Repositories
{
    public interface IEmployeeRepository : IGenericRepository<Employee>
    {
        IEnumerable<Employee> GetAllByDivisionAndStoreNumber(int division, int storeNumber);
    }
}
