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

        public void AddEmployee(Employee employee)
        {
            _employeeRepository.Add(employee);
        }
        public void AddEmployeeBatch(List<Employee> employees)
        {
            _employeeRepository.AddAll(employees);
        }

        public void DeleteEmployee(Employee employee)
        {
            _employeeRepository.Delete(employee);
        }

        public void DeleteAllEmployees()
        {
            _employeeRepository.DeleteAll();
        }

        public void DeleteAllByDivisionAndStoreNumber(int division, int storeNumber)
        {
            _employeeRepository.DeleteAllByDivisionAndStoreNumber(division, storeNumber);
        }

        public IEnumerable<Employee> GetAllByDivisionAndStoreNumber(int division, int storeNumber)
        { 
            return _employeeRepository.GetAllByDivisionAndStoreNumber(division, storeNumber);
        }

        public IEnumerable<Employee> GetAllEmployees()
        {
            return _employeeRepository.GetAll();
        }

        public Employee? GetEmployeeById(int id)
        {
            return _employeeRepository.GetById(id);
        }

        public void UpdateEmployee(Employee employee)
        {
            _employeeRepository.Update(employee);
        }
    }
}
