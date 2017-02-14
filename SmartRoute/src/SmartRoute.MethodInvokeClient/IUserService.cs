using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartRoute
{
	public interface IUserService
	{
		DateTime Register(string name, string email);
		void ChangePWD(string name, string oldpwd, string newpwd);
	}
}
