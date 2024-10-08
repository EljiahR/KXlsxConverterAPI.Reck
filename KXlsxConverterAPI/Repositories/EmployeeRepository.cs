﻿using KXlsxConverterAPI.Data;
using KXlsxConverterAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace KXlsxConverterAPI.Repositories
{
    public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(EmployeeContext context) : base(context) { }

        public IEnumerable<Employee> GetAllByDivisionAndStoreNumber(int division, int storeNumber)
        {
            return _dbSet.Where(e => e.Division == division && e.StoreNumber == storeNumber).ToList();
        }
        public void DeleteAllByDivisionAndStoreNumber(int division, int storeNumber)
        {
            var employeesToDelete = GetAllByDivisionAndStoreNumber(division, storeNumber);
            _dbSet.RemoveRange(employeesToDelete);
        }
    }
}
