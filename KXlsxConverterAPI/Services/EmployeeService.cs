using KXlsxConverterAPI.Models;
using KXlsxConverterAPI.Repositories;

namespace KXlsxConverterAPI.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IGenericEFRepository<Employee> _repository;
        public EmployeeService(IGenericEFRepository<Employee> repository)
        {
            _repository = repository;
        }

        public void AddEmployee(Employee employee)
        {
            throw new NotImplementedException();
        }

        public void DeleteEmployee(Employee employee)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Employee> GetAllEmployees()
        {
            throw new NotImplementedException();
        }

        public Employee GetEmployeeById(int id)
        {
            throw new NotImplementedException();
        }

        public void UpdateEmployee(Employee employee)
        {
            throw new NotImplementedException();
        }
    }
}
