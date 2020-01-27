using System.Threading.Tasks;
using Home.App.Models;
using Home.App.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home.App.Controllers
{

    public class AccountController : Controller
    {

		private readonly AuthService _authService;

		public AccountController(AuthService authService)
		{
			_authService = authService;
		}

		public IActionResult Index()
		{
			if (HttpContext.User.Identity.IsAuthenticated)
			{
				return RedirectToAction("Logout");
			} else
			{
				return View("Login");
			}
		}

		[HttpGet]
		[AllowAnonymous]
		public IActionResult Login([FromQuery] string returnUrl = null)
        {
			ViewData["ReturnUrl"] = returnUrl;

			if (HttpContext.User.Identity.IsAuthenticated)
			{
				return RedirectToAction(controllerName: "Home", actionName: "Index");
			}

            return View();
        }

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login([FromForm] LoginViewModel viewModel, [FromQuery] string returnUrl = null)
		{
			ViewData["ReturnUrl"] = returnUrl;

			if (ModelState.IsValid)
			{
				 string msg = await _authService.Auth(HttpContext, viewModel);

				if (!string.IsNullOrEmpty(msg))
				{
					ModelState.AddModelError(string.Empty, msg);
					return View(viewModel);
				}

				return LocalRedirect(returnUrl ?? "/");
			}

			return View(viewModel);
		}

		[HttpGet]
		[Authorize]
		public IActionResult Logout()
		{
			return View(HttpContext.User.FindFirst("name"));
		}

		[HttpPost]
		[Authorize]
		public async Task<IActionResult> Logout([FromQuery] string returnUrl = null)
		{
			await _authService.Deauth(HttpContext);

			return LocalRedirect(returnUrl ?? "/");
		}

		[HttpGet]
		[AllowAnonymous]
		public IActionResult Register([FromQuery] string returnUrl = null)
		{
			ViewData["ReturnUrl"] = returnUrl;

			return View();
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Register([FromForm] RegisterViewModel viewModel, [FromQuery] string returnUrl = null)
		{
			ViewData["ReturnUrl"] = returnUrl;

			if (ModelState.IsValid)
			{
				string msg = await _authService.Register(viewModel.Email, viewModel.Password, "Samuel Spaghetti Hammersley", "Customer");

				if (!string.IsNullOrEmpty(msg))
				{
					ModelState.AddModelError(string.Empty, msg);
					return View(viewModel);
				}

				return LocalRedirect(returnUrl ?? "/");
			}

			return View(viewModel);
		}

	}
}