﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using BangazonWorkforce.Models;
using BangazonWorkforce.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BangazonWorkforce.Controllers
{
    public class EmployeesController : Controller
    {
        
            private readonly IConfiguration _config;

            public EmployeesController(IConfiguration config)
            {
                _config = config;
            }

            public SqlConnection Connection
            {
                get
                {
                    return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
                }
            }
        // GET: Employees
        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
            SELECT e.Id,
                e.FirstName,
                e.LastName,
                e.DepartmentId,
                e.isSupervisor,
                d.Name
            FROM Employee e
            JOIN Department d ON e.DepartmentId = d.Id
        ";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Employee> employees = new List<Employee>();
                    while (reader.Read())
                    {
                        Employee employee = new Employee
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            IsSuperVisor = reader.GetBoolean(reader.GetOrdinal("isSupervisor")),
                            DepartmentId = reader.GetInt32(reader.GetOrdinal("Id")),
                            CurrentDepartment = new Department
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                            }
                        };

                        employees.Add(employee);
                    }

                    reader.Close();

                    return View(employees);
                }
            }
        }

        // GET: Employees/Details/5
        public ActionResult Details(int id)
        {

            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT e.Id,
                            e.FirstName,
                            e.LastName,
                            e.DepartmentId,
                            e.isSupervisor,
                            d.Name,
                            c.Id,
                            c.Make,
                            c.Manufacturer,
                            ce.Id AS 'Computer Employee Id',
                            ce.UnassignDate,
                            tp.Id AS 'Training Program Id',
                            tp.Name AS 'Program Name'
                        FROM Employee e            
                        FULL JOIN Department d ON e.DepartmentId = d.Id
                        FULL JOIN ComputerEmployee ce ON ce.EmployeeId = e.Id
                        FULL JOIN Computer c ON ce.ComputerId = c.Id
                        FULL JOIN EmployeeTraining et ON et.EmployeeId = e.Id
                        FULL JOIN TrainingProgram tp ON et.TrainingProgramId = tp.Id
                        WHERE e.Id = @id
                        
                        ";

                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Employee employee = null ;

                   

                    while (reader.Read())
                    {

                        if (employee == null)
                        {
                            employee = new Employee
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                IsSuperVisor = reader.GetBoolean(reader.GetOrdinal("isSupervisor")),
                                DepartmentId = reader.GetInt32(reader.GetOrdinal("Id")),
                                CurrentDepartment = new Department()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                }
                            };
                        }
                                                                          

                        if (!reader.IsDBNull(reader.GetOrdinal("Computer Employee Id")) && reader.IsDBNull(reader.GetOrdinal("UnassignDate")))
                                {
                                    employee.CurrentComputer = new Computer()
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                        Make = reader.GetString(reader.GetOrdinal("Make")),
                                        Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer")),
                                    };
                                }
                        if (!reader.IsDBNull(reader.GetOrdinal("Training Program Id")))
                        {
                            TrainingProgram trainingProgram = new TrainingProgram
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Program Name"))
                            };
                            employee.TrainingPrograms.Add(trainingProgram);
                        }
                    }
                    reader.Close();


                    return View(employee);
                }
            }
        }

        // GET: Employees/Create
        public ActionResult Create()
        {
            // Create a new instance of a CreateStudentViewModel
            // If we want to get all the cohorts, we need to use the constructor that's expecting a connection string. 
            // When we create this instance, the constructor will run and get all the cohorts.
            CreateEmployeeViewModel employeeViewModel = new CreateEmployeeViewModel(_config.GetConnectionString("DefaultConnection"));

            // Once we've created it, we can pass it to the view
            return View(employeeViewModel);
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateEmployeeViewModel model)
        {                      
           
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO Employee
                ( FirstName, LastName, DepartmentId, isSupervisor )
                VALUES
                ( @firstName, @lastName, @departmentId, @isSupervisor )";
                        cmd.Parameters.Add(new SqlParameter("@firstName", model.employee.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", model.employee.LastName));                        
                        cmd.Parameters.Add(new SqlParameter("@departmentId", model.employee.DepartmentId));
                        cmd.Parameters.Add(new SqlParameter("@isSupervisor", model.employee.IsSuperVisor));
                        cmd.ExecuteNonQuery();

                        return RedirectToAction(nameof(Index));
                    }
                }
            }
           
        

        // GET: Employees/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Employees/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Employees/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}