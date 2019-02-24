using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ProMa.Controllers
{
	public class GamesController : Controller
	{
		public IActionResult Tetris()
		{
			return View();
		}
	}
}
