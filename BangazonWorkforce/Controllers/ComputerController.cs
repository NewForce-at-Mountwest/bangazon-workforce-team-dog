﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using BangazonWorkforce.Models;


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
                    cmd.CommandText = @"SELECT Computer.Id, Computer.Make, Computer.Manufacturer, Computer.PurchaseDate, Computer.DecomissionDate FROM Computer";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Computer> computers = new List<Computer>();
                    DateTime? NullDateTime = null;

                    while (reader.Read())
                    {

                        //create individual instance of computer
                        Computer currentComputer = new Computer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Make = reader.GetString(reader.GetOrdinal("Make")),
                            Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer")),
                            PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                            DecomissionDate = reader.IsDBNull(reader.GetOrdinal("DecomissionDate")) ? NullDateTime : reader.GetDateTime(reader.GetOrdinal("DecomissionDate"))

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
            return View();
        }

        // POST: Computer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Computer computer)
        {
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO Computer
                ( Make, Manufacturer, PurchaseDate )
                VALUES
                ( @Make, @Manufacturer, @PurchaseDate )";
                        cmd.Parameters.Add(new SqlParameter("@Make", computer.Make));
                        cmd.Parameters.Add(new SqlParameter("@Manufacturer", computer.Manufacturer));
                        cmd.Parameters.Add(new SqlParameter("@PurchaseDate", computer.PurchaseDate));

                        cmd.ExecuteNonQuery();

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
          // query database for computer you wish to delete with 
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT c.Id AS 'Computer Id', c.Make, c.Manufacturer, c.PurchaseDate,
                    c.DecomissionDate, ce.ComputerId, ce.EmployeeId, e.Id 
                    FROM Computer c LEFT JOIN ComputerEmployee ce
                    ON ce.ComputerId = c.Id LEFT JOIN Employee e
                    ON ce.EmployeeId = e.Id WHERE c.Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Computer computer = null;
                    DateTime? NullDateTime = null;
                   
                    //create instance of computer and a list of assigned employees
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
                    return View(); 
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