using KXlsxConverterAPI.Data;
using KXlsxConverterAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace KXlsxConverterAPI.Repositories
{
    public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(EmployeeContext context) : base(context) { }

        public async Task<IEnumerable<Employee>> GetAllByDivisionAndStoreNumberAsync(int division, int storeNumber)
        {
            return await _dbSet.Where(e => e.Division == division && e.StoreNumber == storeNumber).ToListAsync();
        }
        public async Task DeleteAllByDivisionAndStoreNumberAsync(int division, int storeNumber)
        {
            await _dbSet.Where(e => e.Division == division && e.StoreNumber == storeNumber).ExecuteDeleteAsync();
        }
    }
}
