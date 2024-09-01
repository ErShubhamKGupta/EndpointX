namespace EndpointX.Models
{
    public class Employee : BaseClass
    {
        public required string EmployeeName { get; set; }
        public required string EmployeeCode { get; set; }
        public required string Email { get; set; }
        public required DateTime DateOfBith { get; set; }
        public required string Designation { get; set; }
        public required int Salary { get; set; }
        public required DateTime JoiningDate { get; set; }
    }
}
