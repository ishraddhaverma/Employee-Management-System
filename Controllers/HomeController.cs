﻿using EmployeeManagement.Models;
using EmployeeManagement.Security;
using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.Controllers
{

    [Authorize]

    public class HomeController : Controller
    {

        private readonly IEmployeeRepository _employeeRepository;
        private readonly IHostingEnvironment hostingEnviornment;
        private readonly ILogger logger;
        private readonly IDataProtector protector;

#pragma warning restore S3459 // Unassigned members should be removed
        private readonly Employee employee;

      
        public HomeController(IEmployeeRepository employeeRepository , IHostingEnvironment hostingEnviornment, ILogger<HomeController> logger, IDataProtectionProvider dataProtectionProvider,
                              DataProtectionPurposeStrings dataProtectionPurposeStrings)
        {
            
            _employeeRepository = employeeRepository; //dependancy injection
            this.hostingEnviornment = hostingEnviornment;
            this.logger = logger;
            this.protector = dataProtectionProvider.CreateProtector(
               dataProtectionPurposeStrings.EmployeeIdRouteValue);
        }

       // [Route("")]
      //  [Route("Home")]   //attribute routing
       // [Route("Home/Index")]
       [AllowAnonymous]
        public ViewResult Index()
        {
          var model=  _employeeRepository.GetAllEmployee().Select(e =>
          {
              // Encrypt the ID value and store in EncryptedId property
              e.EncryptedId = protector.Protect(e.Id.ToString());
              return e;
          });
            return View(model);
        }


        // [Route("Home/Details/{id?}")]
        [AllowAnonymous]
        public ViewResult Details(string id)
        {
            //throw new Exception("Exception from Details View");

            logger.LogTrace("Trace Log");
            logger.LogDebug("Debug Log");
            logger.LogInformation("Information Log");
            logger.LogWarning("Warning Log");
            logger.LogError("Error Log");
            logger.LogCritical("Critical Log");

            string decryptedId= protector.Unprotect(id);
            int decryptedIntId = Convert.ToInt32(decryptedId);


            Employee employee = _employeeRepository.GetEmployee(decryptedIntId);

           // Employee employee = _employeeRepository.GetEmployee(id.Value);
            if (employee == null)
            {
                Response.StatusCode = 404;
                return View("EmployeeNotFound", decryptedIntId);
            }
            HomeDetailsViewModel homeDetailsViewModel = new HomeDetailsViewModel()
            {
                Employee = employee,
                PageTitle = "Employee details"

            };


            return View(homeDetailsViewModel);


            // Employee model = _employeeRepository GetEmployee(1);
            // ViewData["Employee"] = model;  
            //ViewData["PageTitle"] = "Employee Details";  //viewdata
            //ViewBag.emp = "Emp1";   //viewbag
            //return View(model); //strongly typed data




        }

       [HttpGet]
        public ViewResult Create()
        {
            return View();
        }



        [HttpGet]

        public ViewResult Edit(int id)
        {
            Employee employee = _employeeRepository.GetEmployee(id);
            EmployeeEditViewModel employeeEditViewModel = new EmployeeEditViewModel
            {
                Id = employee.Id,
                Name = employee.Name,
                Email = employee.Email,
                Department = employee.Department,
                ExistingPhotoPath = employee.PhotoPath

            };
            return View(employeeEditViewModel);
        }






        [HttpPost]

        public IActionResult Edit(EmployeeEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                Employee employee = _employeeRepository.GetEmployee(model.Id);
                employee.Name = model.Name;
                employee.Email = model.Email;
                employee.Department = model.Department;
            
                if(model.Photos != null)
                {
                    if(model.ExistingPhotoPath != null)
                    {
                       string filePath= Path.Combine(hostingEnviornment.WebRootPath,"images",model.ExistingPhotoPath);
                        System.IO.File.Delete(filePath);
                    }
                    employee.PhotoPath = ProcessUploadedFile(model);
                }


                _employeeRepository.Update(employee);

                return RedirectToAction("Index");
            }
            return View();

        }

        private string ProcessUploadedFile(EmployeeCreateViewModel model)
        {
            string uniqueFileName = null;
            if (model.Photos != null && model.Photos.Count > 0)
            {
                foreach (IFormFile photo in model.Photos)
                {
                    string uploadsFolder = Path.Combine(hostingEnviornment.WebRootPath, "images");
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + photo.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                   using(var fileStream= new FileStream(filePath, FileMode.Create))
                    {
                        photo.CopyTo(fileStream);
                    }
                        
                }

            }

            return uniqueFileName;
        }

        [HttpPost]
       

        public IActionResult Create(EmployeeCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = ProcessUploadedFile(model);

                Employee newEmployee = new Employee
                {
                    Name = model.Name,
                    Email = model.Email,
                    Department = model.Department,
                    PhotoPath = uniqueFileName

                };

                _employeeRepository.Add(newEmployee);

                return RedirectToAction("Details", new { id = newEmployee.Id });
            }
            return View();





        }


    }
}
