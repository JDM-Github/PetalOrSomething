using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetalOrSomething.Models
{
	public class EmailRequest
	{
		public string Email { get; set; }
	}
	public class Account
	{
		public int Id { get; set; }
		public string FirstName { get; set; }
		public string MiddleName { get; set; } = "";
		public string LastName { get; set; }

		[Required]
		public string Email { get; set; }
		public string Password { get; set; }
		public string PhoneNumber { get; set; }
		public string Location { get; set; }
		public string VerificationCode { get; set; } = "12345";
		public bool IsVerified { get; set; } = false;

		public string FullName
		{
			get
			{
				return string.Join(" ", new[] { FirstName, MiddleName, LastName }
								.Where(name => !string.IsNullOrWhiteSpace(name)));
			}
		}

		public Account() { }
	}
}
