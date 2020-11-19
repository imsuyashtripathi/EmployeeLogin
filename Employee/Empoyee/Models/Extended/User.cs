using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Employee.Models.Extended
{
    [MetadataType(typeof(UserMetaData))]
    public partial class User
    {
        public string ConfirmPassword { get; set; }
    }
    public class UserMetaData
    {
        [Display(Name="First Name")]
        [Required(AllowEmptyStrings =false,ErrorMessage ="First Name Required")]
        public string FirstName { get; set; }

        [Display(Name ="Last Name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Last Name Required")]
        public string LastName { get; set; }

        [Display(Name = "Email Id")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Email Id Required")]
        [DataType(DataType.EmailAddress)]
        public string EmailID { get; set; }

        [Display(Name = "Date Of Birth")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode =true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime DateOfBirth { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Password is Required")]
        [DataType(DataType.Password)]
        [MinLength(6,ErrorMessage ="Minimum 6 character required")]
        public string Password { get; set; }

        [Display(Name ="Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password",ErrorMessage ="Confirm password and password do not match")]
        public string ConfirmPassword { get; set; }
    }
}