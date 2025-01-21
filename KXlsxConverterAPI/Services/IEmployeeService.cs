using KXlsxConverterAPI.Models;

namespace KXlsxConverterAPI.Services
{
    public interface IEmployeeService
    {
        Task<IEnumerable<Employee>> GetAllEmployeesAsync();
        Task<IEnumerable<Employee>> GetAllByDivisionAndStoreNumberAsync(int division, int storeNumber);
        Task<Employee?> GetEmployeeByIdAsync(int id);
        Task AddEmployeeAsync(Employee employee);
        Task AddEmployeeBatchAsync(List<Employee> employees);
        void DeleteEmployee(Employee employee);
        void DeleteAllEmployees();
        Task DeleteAllByDivisionAndStoreNumberAsync(int division, int storeNumber);
        void UpdateEmployeeAsync(Employee employee);
    }
}
