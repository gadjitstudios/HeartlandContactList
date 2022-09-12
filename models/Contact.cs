
using System.ComponentModel.DataAnnotations;

namespace Heartland.models
{
    public class Contact{
        [Required]
        [Display(Name="First Name")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name="Last Name")]
        public string LastName { get; set; }
        [Display(Name="Middle Name")]
        public string MiddleName { get; set; }
        [Required]
        [Display(Name="Address 1")]
        public string Address1 { get; set; }
        [Display(Name="Address 2")]
        public string Address2 { get; set; }  
        [Required]      
        [Phone]
        [Display(Name="Telephone Number")]
        public string Phone { get; set; }
    }
}