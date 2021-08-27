using leave_management.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Models
{
    public class LeaveAllocationVM
    {
    
        public int Id { get; set; }
        [Display(Name = "Number Of Days")]
        public int NumberOfDays { get; set; }
        public DateTime DateCreated { get; set; }

        public int Period { get; set; }

        public EmployeeVM Employee { get; set; }
        public string EmployeeId { get; set; }
     
        public LeaveTypeVM LeaveType { get; set; }
        public int LeaveTypeId { get; set; }

      

    }

    public class CreateLeaveAllocationVM
    {
        public int NumberUpdated { get; set; }
        public List<LeaveTypeVM> LeaveTypeVMs { get; set; }
    }
    public class EditLeaveAllocationVM
    {
        
        public int Id { get; set; }
        public EmployeeVM Employee { get; set; }
        public string EmployeeId { get; set; }
        [Display(Name = "Number Of Days")]
        public int NumberOfDays { get; set; }
        [Display(Name = "Leave Type")]
        public LeaveTypeVM LeaveType { get; set; }


    }
    public class ViewLeaveAllocationVM
    {
        public EmployeeVM Employee { get; set; }
        public string EmployeeId { get; set; }
        public List<LeaveAllocationVM> LeaveAllocations { get; set; }
    }
}
