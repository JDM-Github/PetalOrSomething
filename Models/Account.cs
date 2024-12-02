using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetalOrSomething.Models
{
	public class Account
	{
		public int Id {  get; set; }
		public string FirstName { get; set; }
		public string MiddleName { get; set; }
		public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
		public string Password { get; set; }
		public string PhoneNumber { get; set; }
		public string Location { get; set; }
		public Account()
        {
        }
    }
}
