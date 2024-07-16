using KXlsxConverterAPI.Models;

namespace KXlsxConverterAPI.Services
{
    public interface IEmployeeService
    {
        IEnumerable<Employee> GetAllEmployees();
        IEnumerable<Employee> GetAllByDivisionAndStoreNumber(int division, int storeNumber);
        Employee? GetEmployeeById(int id);
        void AddEmployee(Employee employee);
        void AddEmployeeBatch(List<Employee> employees);
        void DeleteEmployee(Employee employee);
        void DeleteAllEmployees();
        void DeleteAllByDivisionAndStoreNumber(int division, int storeNumber);
        void UpdateEmployee(Employee employee);
    }
}
