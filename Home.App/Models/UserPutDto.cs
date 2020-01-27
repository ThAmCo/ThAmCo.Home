using System.Collections.Generic;

namespace Home.App.Models
{
	public class UserPutDto
	{
		public string Email { get; set; }

		public string Password { get; set; }

		public string FullName { get; set; }

		public IEnumerable<string> Roles { get; set; }

	}
}