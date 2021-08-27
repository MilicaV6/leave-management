﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Models
{
    public class LeaveTypeVM
    {
      
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Display(Name="Date Created")]
        public DateTime? DateCreated { get; set; }
        [Required]
        [Range(1,25,ErrorMessage ="Please Enter A Valid Number")]
        [Display(Name="Default Days")]
        public int DefaultDays { get; set; }
    }
   
}
