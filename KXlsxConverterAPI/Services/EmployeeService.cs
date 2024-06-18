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
            _repository.Add(employee);
        }

        public void DeleteEmployee(Employee employee)
        {
            _repository.Delete(employee);
        }

        public IEnumerable<Employee> GetAllEmployees()
        {
            return _repository.GetAll();
        }

        public Employee GetEmployeeById(int id)
        {
            return _repository.GetById(id);
        }

        public void UpdateEmployee(Employee employee)
        {
            _repository.Update(employee);
        }
    }
}
