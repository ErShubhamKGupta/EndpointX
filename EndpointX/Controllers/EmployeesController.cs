using EndpointX.Models;
using EndpointX.Models.Data;
using EndpointX.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EndpointX.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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

        // GET: api/Employee/user@example.com
        [HttpGet("{emailID}")]
        public async Task<ActionResult<Employee>> GetEmployeeByEmailId(string emailID)
        {
            _logger.LogInformation("Getting employee with email id {emailID}", emailID);
            if (string.IsNullOrWhiteSpace(emailID))
            {
                return BadRequest("Email is required.");
            }
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == emailID);

            if (employee == null)
            {
                _logger.LogWarning("Employee with Email ID {Id} not found", emailID);
                return NotFound();
            }

            return employee;
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
        private bool EmployeeExists(Guid id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }

    }
}
