using System.ComponentModel.DataAnnotations;

namespace Home.App.Models
{
	public class RegisterViewModel
	{

		[DataType(DataType.EmailAddress)]
		public string Email { get; set; }

		[DataType(DataType.Password)]
		[StringLength(50, ErrorMessage = "Must be at least 8 characters long", MinimumLength = 6)]
		public string Password { get; set; }

		[Compare("Password", ErrorMessage = "Passwords must match")]
		public string ReTypePassword { get; set; }

	}
}