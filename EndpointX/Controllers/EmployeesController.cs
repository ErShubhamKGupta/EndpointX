﻿using EndpointX.Models;
using EndpointX.Models.Data;
using EndpointX.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EndpointX.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmployeesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EmployeesController> _logger;

        public EmployeesController(ApplicationDbContext context, ILogger<EmployeesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Employees
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
            _logger.LogInformation("Getting all employees");
            return await _context.Employees.ToListAsync();
        }

        // GET: api/Employee/5
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Employee>> GetEmployee(Guid id)
        {
            _logger.LogInformation("Getting employee with ID {Id}", id);
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                _logger.LogWarning("Employee with ID {Id} not found", id);
                return NotFound();
            }

            return employee;
        }

        // GET: api/Employee/search?employeeCode=12345&email=johndoe@example.com
        [HttpGet("search")]
        public async Task<ActionResult<Employee>> GetEmployeeByEmailIdOrEmpCode(string emailID = null, string empCode = null)
        {
            _logger.LogInformation("Getting employee with email id {emailID} or employee code", emailID, empCode);

            if (string.IsNullOrEmpty(emailID) && string.IsNullOrEmpty(empCode))
            {
                return BadRequest("Either employeeCode or email id must be provided.");
            }

            Employee employee = null;

            if (!string.IsNullOrEmpty(empCode))
            {
                employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeCode == empCode);
            }

            if (employee == null && !string.IsNullOrEmpty(emailID))
            {
                employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == emailID);
            }

            if (employee == null)
            {
                _logger.LogWarning("Employee with code {EmployeeCode} or email {Email} not found.", empCode, emailID);
                return NotFound();
            }

            _logger.LogInformation("Retrieved employee with code {EmployeeCode} or email {Email}.", empCode, emailID);
            return Ok(employee);
        }

        // POST: api/Employee
        [HttpPost]
        public async Task<ActionResult> CreateEmployee(AddEmployeeDto employee)
        {
            _logger.LogInformation("Executing the method CreateEmployee.");
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid employee data received.");
                return BadRequest(ModelState);
            }
            var empEntity = new Employee
            {
                EmployeeName = employee.EmployeeName,
                EmployeeCode = employee.EmployeeCode,
                DateOfBith = employee.DateOfBith,
                Email = employee.Email,
                Designation = employee.Designation,
                Salary = employee.Salary,
                JoiningDate = employee.JoiningDate,
            };
            _context.Employees.Add(empEntity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new employee with ID {Id}", empEntity.Id);

            return Ok(empEntity);

        }

        // PUT: api/Employees/5
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateEmployee(Guid id, UpdateEmployeeDto employee)
        {
            _logger.LogInformation("Executing the method UpdateEmployee.");

            var emp = _context.Employees.FirstOrDefault(x => x.Id == id);
            if (emp == null)
            {
                _logger.LogWarning("Employee with ID {Id} not found", id);
                return NotFound();
            }

            emp.EmployeeName = employee.EmployeeName;
            emp.EmployeeCode = employee.EmployeeCode;
            emp.DateOfBith = employee.DateOfBith;
            emp.Email = employee.Email;
            emp.Designation = employee.Designation;
            emp.Salary = employee.Salary;
            emp.JoiningDate = employee.JoiningDate;
            _context.Employees.Update(emp);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated the employee with ID {Id}", emp.Id);

            return Ok(emp);
        }

        // DELETE: api/Employees/5
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteEmployee(Guid id)
        {
            _logger.LogInformation("Executing the method DeleteEmployee.");
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                _logger.LogWarning("Employee with ID {Id} not found", id);
                return NotFound();
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted employee with ID {Id}", id);

            return NoContent();
        }

    }
}
