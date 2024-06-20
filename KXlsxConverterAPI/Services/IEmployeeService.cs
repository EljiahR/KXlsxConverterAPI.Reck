using KXlsxConverterAPI.Models;

namespace KXlsxConverterAPI.Services
{
    public interface IEmployeeService
    {
        IEnumerable<Employee> GetAllEmployees();
        IEnumerable<Employee> GetAllByDivisionAndStoreNumber(int division, int storeNumber);
        Employee GetEmployeeById(int id);
        void AddEmployee(Employee employee);
        void DeleteEmployee(Employee employee);
        void UpdateEmployee(Employee employee);
    }
}
