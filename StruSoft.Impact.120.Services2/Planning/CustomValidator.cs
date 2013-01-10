using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;

namespace StruSoft.Impact.V120.Services
{
	/// <summary>
	/// Custom caller validation/Authorization
	/// </summary>
	public class CustomValidator : UserNamePasswordValidator
	{
		/// <summary>
		/// Custom caller validation/Authorization
		/// </summary>
		/// <param name="userName">The user name</param>
		/// <param name="password">The user password</param>
		public override void Validate(string userName, string password)
		{
			// validate arguments
			if (string.IsNullOrEmpty(userName))
				throw new ArgumentNullException("userName");
			if (string.IsNullOrEmpty(password))
				throw new ArgumentNullException("password");

			// check the user credentials from database
			//int userid = 0;
			//CheckUserNameAndPassword(userName, password, out userid);
			//if (0 == userid)
			//throw new SecurityTokenException("Unknown username or password");
		}
	}	
}