using KXlsxConverterAPI.Models;
using KXlsxConverterAPI.Repositories;

namespace KXlsxConverterAPI.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        public EmployeeService(IEmployeeRepository repository)
        {
            _employeeRepository = repository;
        }

        public async Task AddEmployeeAsync(Employee employee)
        {
            await _employeeRepository.AddAsync(employee);
        }
        public async Task AddEmployeeBatchAsync(List<Employee> employees)
        {
            await _employeeRepository.AddAllAsync(employees);
        }

        public void DeleteEmployee(Employee employee)
        {
            _employeeRepository.Delete(employee);
        }

        public void DeleteAllEmployees()
        {
            _employeeRepository.DeleteAll();
        }

        public async Task DeleteAllByDivisionAndStoreNumberAsync(int division, int storeNumber)
        {
           await _employeeRepository.DeleteAllByDivisionAndStoreNumberAsync(division, storeNumber);
        }

        public async Task<IEnumerable<Employee>> GetAllByDivisionAndStoreNumberAsync(int division, int storeNumber)
        { 
            return await _employeeRepository.GetAllByDivisionAndStoreNumberAsync(division, storeNumber);
        }

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        {
            return await _employeeRepository.GetAllAsync();
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            return await _employeeRepository.GetByIdAsync(id);
        }

        public void UpdateEmployee(Employee employee)
        {
            _employeeRepository.Update(employee);
        }
    }
}
