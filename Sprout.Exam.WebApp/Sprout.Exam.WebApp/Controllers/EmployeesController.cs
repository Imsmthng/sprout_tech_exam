using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Sprout.Exam.Business.DataTransferObjects;
using Sprout.Exam.Common.Enums;
using Sprout.Exam.DataAccess;
using Sprout.Exam.Common;

namespace Sprout.Exam.WebApp.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {

        private readonly EmployeeContext _context;

        public EmployeesController(EmployeeContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Refactor this method to go through proper layers and fetch from the DB.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await Task.FromResult(_context.Employees.Where(x => x.IsDeleted == false).ToList());
            List<EmployeeDto> employeesDto = new List<EmployeeDto>();

            foreach (var employee in result)
            {
                EmployeeDto employeeDto = new EmployeeDto();
                employeeDto.Id = employee.Id;
                employeeDto.FullName = employee.FullName;
                employeeDto.Birthdate = employee.Birthdate.ToString("yyyy-MM-dd");
                employeeDto.Tin = employee.Tin;
                employeeDto.TypeId = employee.EmployeeTypeId;

                employeesDto.Add(employeeDto);
            }

            //return Ok(result);
            return Ok(employeesDto);
        }

        /// <summary>
        /// Refactor this method to go through proper layers and fetch from the DB.
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await Task.FromResult(_context.Employees.FirstOrDefault(m => m.Id == id));
            EmployeeDto employeeDto = new EmployeeDto();
            employeeDto.Id = result.Id;
            employeeDto.FullName = result.FullName;
            employeeDto.Birthdate = result.Birthdate.ToString("yyyy-MM-dd");
            employeeDto.Tin = result.Tin;
            employeeDto.TypeId = result.EmployeeTypeId;

            //return Ok(result);
            return Ok(employeeDto);
        }

        /// <summary>
        /// Refactor this method to go through proper layers and update changes to the DB.
        /// </summary>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(EditEmployeeDto input)
        {
            var item = await Task.FromResult(_context.Employees.FirstOrDefault(m => m.Id == input.Id));
            if (item == null) return NotFound();
            item.FullName = input.FullName;
            item.Tin = input.Tin;
            item.Birthdate = input.Birthdate;
            item.EmployeeTypeId = input.TypeId;

            _context.SaveChanges();

            return Ok(item);
        }

        /// <summary>
        /// Refactor this method to go through proper layers and insert employees to the DB.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post(CreateEmployeeDto input)
        {
            Employee employee = new Employee();
            employee.FullName = input.FullName;
            employee.Tin = input.Tin;
            employee.Birthdate = input.Birthdate;
            employee.EmployeeTypeId = input.TypeId;

            _context.Employees.Add(employee);
            _context.SaveChanges();

            var id = await Task.FromResult(_context.Employees.Max(x => x.Id));

            return Created($"/api/employees/{id}", id);
        }


        /// <summary>
        /// Refactor this method to go through proper layers and perform soft deletion of an employee to the DB.
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await Task.FromResult(_context.Employees.FirstOrDefault(x => x.Id == id));

            if (result == null) return NotFound();
            result.IsDeleted = true;

            _context.SaveChanges();

            return Ok(id);
        }



        /// <summary>
        /// Refactor this method to go through proper layers and use Factory pattern
        /// </summary>
        /// <returns></returns>
        [HttpPost("{id}/calculate")]
        public async Task<IActionResult> Calculate(Temp temp)
        {
            var result = await Task.FromResult(_context.Employees.FirstOrDefault(x => x.Id == temp.Id));
            if (result == null) return NotFound();

            EmployeeFactory employeeFactory = new EmployeeFactory();
            IEmployee employee = employeeFactory.GetEmployeeType(result.EmployeeTypeId);

            var type = (EmployeeType)result.EmployeeTypeId;
            decimal pay = 0;


            if (type == EmployeeType.Regular)
            {
                pay = employee.CalculatePay(temp.absentDays);
            }
            else
            {
                pay = employee.CalculatePay(temp.workedDays);
            }

            return Ok(pay);
        }

    }
}
