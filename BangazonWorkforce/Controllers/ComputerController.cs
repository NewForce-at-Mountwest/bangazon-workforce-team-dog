using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using BangazonWorkforce.Models;
using BangazonWorkforce.Models.ViewModels;

namespace BangazonWorkforce.Controllers
{
    public class ComputerController : Controller
    {
        //set up connection to the database
        private readonly IConfiguration _config;

        public ComputerController(IConfiguration config)
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




        // GET: Computer for a list of all computers
        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                //SQL command to bring back all of the items needed
                {
                    cmd.CommandText = @"SELECT c.Id AS 'Computer Id', c.Make, c.Manufacturer, c.PurchaseDate, C.DecomissionDate, e.FirstName as 'First Name', e.LastName AS 'Last Name' 
                            FROM Computer c FULL JOIN ComputerEmployee ce ON ce.ComputerId = c.Id 
                            LEFT JOIN Employee e ON ce.EmployeeId = e.Id";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Computer> computers = new List<Computer>();
                    DateTime? NullDateTime = null;

                    while (reader.Read())
                    {

                        //create individual instance of computer
                        Computer currentComputer = new Computer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Computer Id")),
                            Make = reader.GetString(reader.GetOrdinal("Make")),
                            Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer")),
                            PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                            DecomissionDate = reader.IsDBNull(reader.GetOrdinal("DecomissionDate")) ? NullDateTime : reader.GetDateTime(reader.GetOrdinal("DecomissionDate"))

                    };
                        //only print owner names if they are in database
                        if (!reader.IsDBNull(reader.GetOrdinal("Last Name")))
                        {
                            currentComputer.CurrentEmployee = new Employee { FirstName = reader.GetString(reader.GetOrdinal("First Name")), LastName = reader.GetString(reader.GetOrdinal("Last Name")) };
                        };

                        computers.Add(currentComputer);
                    }
                    reader.Close();
                    return View(computers);
                }
            }
        }
    

        // GET: Computer/Details/5
        public ActionResult Details(int id)
        {
//query the database for an individual computer 
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, Make, Manufacturer, PurchaseDate, DecomissionDate
                        FROM Computer
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Computer computer = null;
                    DateTime? NullDateTime = null;

//create instance of computer

                    if (reader.Read())
                    {
                        computer = new Computer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Make = reader.GetString(reader.GetOrdinal("Make")),
                            Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer")),
                            PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                            DecomissionDate = reader.IsDBNull(reader.GetOrdinal("DecomissionDate")) ? NullDateTime : reader.GetDateTime(reader.GetOrdinal("DecomissionDate"))
                        };
                    }
                    reader.Close();
                    //return the details view of a single computer with its details
                    return View(computer);
                }
            }
            return View();
        }

        // GET: Computer/Create
        public ActionResult Create()
        {
            CreateComputerViewModel computerViewModel = new CreateComputerViewModel(_config.GetConnectionString("DefaultConnection"));

            return View(computerViewModel);
        }

        // POST: Computer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateComputerViewModel computerViewModel)
        {
            {
                int newId = 0;
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO Computer
                ( Make, Manufacturer, PurchaseDate)
                 OUTPUT INSERTED.Id
                VALUES
                ( @Make, @Manufacturer, @PurchaseDate)";

                        cmd.Parameters.Add(new SqlParameter("@Make", computerViewModel.computer.Make));
                        cmd.Parameters.Add(new SqlParameter("@Manufacturer", computerViewModel.computer.Manufacturer));
                        cmd.Parameters.Add(new SqlParameter("@PurchaseDate", computerViewModel.computer.PurchaseDate));
                        newId = (int)cmd.ExecuteScalar();

                      
                        // if an employee is assigned insert an entry into the computeremployee table
                        if (computerViewModel.assignedEmployee.Id != 0)
                        {
                            cmd.CommandText = @"INSERT INTO ComputerEmployee
                (ComputerId, EmployeeId, AssignDate)
                 VALUES
                  (@ComputerId, @EmployeeId, @AssignDate) ";

                            cmd.Parameters.Add(new SqlParameter("@ComputerId", newId));
                            cmd.Parameters.Add(new SqlParameter("@EmployeeId", computerViewModel.assignedEmployee.Id));
                            cmd.Parameters.Add(new SqlParameter("@AssignDate", DateTime.Now));

                            cmd.ExecuteNonQuery();
                        }
                        return RedirectToAction(nameof(Index));
                    
                                      
                    }
                }
            }
        }

        // GET: Computer/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Computer/Edit/5
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

        // GET: Computer/Delete/5
        //conditonal only delete computer if it has no assign date in the computeremployee table
        public ActionResult Delete(int id)
        {
          // query database for computer you wish to delete with employee assigned if applicable
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT c.Id AS 'Computer Id', c.Make, c.Manufacturer, c.PurchaseDate,
                    c.DecomissionDate, ce.Id AS 'Employee Computer', ce.ComputerId, ce.EmployeeId, e.Id AS 'Employee Id'
                    FROM Computer c
                    LEFT JOIN ComputerEmployee ce
                    ON ce.ComputerId  = c.Id 
                    LEFT JOIN Employee e
                    ON ce.EmployeeId = e.Id 
                    WHERE c.Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                   
                    DateTime? NullDateTime = null;
                    Computer computer = new Computer();
                    //create instance of computer and a list of assigned employees
                   if (reader.Read())
                    {
                       computer = new Computer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Computer Id")),
                            Make = reader.GetString(reader.GetOrdinal("Make")),
                            Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer")),
                            PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                            DecomissionDate = reader.IsDBNull(reader.GetOrdinal("DecomissionDate")) ? NullDateTime : reader.GetDateTime(reader.GetOrdinal("DecomissionDate"))
                        };
                        //If ComputerEmployee.Id is null assign boolean
                        if (!reader.IsDBNull(reader.GetOrdinal("Employee Computer")))
                        { computer.IsAssigned = true; }
                    }
                    reader.Close();
                    return View(computer);

                }
            }
        }
        // POST: Computer/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, Computer computer)
        {

            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                   
                        {
                            cmd.CommandText = @"DELETE FROM Computer WHERE Id = @id";
                            cmd.Parameters.Add(new SqlParameter("@id", id));

                            int rowsAffected = cmd.ExecuteNonQuery();
                        }
                    }
                    return RedirectToAction(nameof(Index));
                }

            catch
            {
                return View();
            }
        }
    }
}