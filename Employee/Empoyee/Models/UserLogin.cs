using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Employee.Models
{
    public class UserLogin
    {
        [Display(Name="Email ID")]
        [Required(AllowEmptyStrings =false,ErrorMessage ="Email ID Required")]
        public string EmailID { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Email ID Required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remeber Me")]
        public bool RemeberMe { get; set; }
    }
}