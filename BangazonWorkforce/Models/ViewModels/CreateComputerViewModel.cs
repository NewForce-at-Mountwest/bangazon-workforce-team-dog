using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonWorkforce.Models.ViewModels
{
    public class CreateComputerViewModel
    {
        // This is where our dropdown options will go! SelectListItem is a built in type for dropdown lists
        public List<SelectListItem> Employees { get; set; }


        // An individual computer. When we render the form (i.e. make a GET request to computer/Create) this will be null. When we submit the form (i.e. make a POST request to computers/Create), this will hold the data from the form.
        public Computer computer { get; set; }

        // Connection to the database
        protected string _connectionString;

        protected SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_connectionString);
            }
        }

        // Empty constructor so that we can create a new instance of this view model when we make our POST request (in which case it won't need a connection string)
        public CreateComputerViewModel() { }

        // This is an example of method overloading! We have one constructor with no parameter and another constructor that's expecting a connection string. We can call either one!
        public CreateComputerViewModel(string connectionString)
        {
            _connectionString = connectionString;

            // When we create a new instance of this view model, we'll call the internal methods to get all the departments from the database
            // Then we'll map over them and convert the list of departments to a list of select list items
            Employees = GetAllEmployees()
                .Select(employee => new SelectListItem()
                {
                    Text = employee.FirstName + employee.LastName,
                    Value = employee.Id.ToString()

                })
                .ToList();

            // Add an option with instructiosn for how to use the dropdown
            Employees.Insert(0, new SelectListItem
            {
                Text = "Choose an employee",
                Value = "0"
            });

        }

        // Internal method -- connects to DB, gets all employees, returns list of employees
        protected List<Employee> GetAllEmployees()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, FirstName, LastName FROM Employee";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Employee> employees = new List<Employee>();
                    while (reader.Read())
                    {
                        employees.Add(new Employer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName"))
                        });
                    }

                    reader.Close();

                    return departments;
                }
            }
        }
    }
}
