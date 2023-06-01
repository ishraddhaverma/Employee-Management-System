using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.Models
{
    public static class ModelBuilderExtentions
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>().HasData(
              new Employee
              {
                  Id = 1,
                  Name = "Marry",
                  Department = Dept.IT,
                  Email = "Marry@gmail.com"
              },

           new Employee
           {
               Id = 2,
               Name = "Sherry",
               Department = Dept.HR,
               Email = "Sherry@gmail.com"
           });
        }
    }
}
