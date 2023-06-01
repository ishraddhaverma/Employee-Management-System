using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.Models
{

    public class MockEmployeeRepository: IEmployeeRepository
    {
        private List<Employee> _employeeList;

        public MockEmployeeRepository()

        {
            _employeeList = new List<Employee>() 
            {
            
            new Employee() { Id = 1, Name = "Naina", Department = Dept.IT, Email = "naina@gmail.com" },
            new Employee() { Id = 2, Name = "Payal", Department = Dept.IT, Email = "payal@gmail.com" },
            new Employee() { Id = 3, Name = "Vedika", Department = Dept.HR, Email = "vedika@gmail.com" }
        };
        }


        public Employee Add(Employee employee)
        {
            employee.Id = _employeeList.Max(e => e.Id) + 1;
            _employeeList.Add(employee);
            return employee;

        }

        

        public Employee Delete(int id)
        {
            Employee employee = _employeeList.FirstOrDefault(e => e.Id == id);
            if (employee != null) {
                _employeeList.Remove(employee);
            }
            return employee;
        }

        public IEnumerable<Employee> GetAllEmployee()
        {
            return _employeeList;
        }

        public Employee GetEmployee(int Id)
        {
            return _employeeList.FirstOrDefault(e => e.Id == Id);
        }

        public Employee Update(Employee employeeChanges)
        {
            Employee employee = _employeeList.FirstOrDefault(e => e.Id == employeeChanges.Id);
            if (employee != null)
            {

                employee.Name = employeeChanges.Name;
                employee.Id = employeeChanges.Id;
                employee.Email = employeeChanges.Email;
                employee.Department = employeeChanges.Department;
            }
            return employee;

        }
    }
}
