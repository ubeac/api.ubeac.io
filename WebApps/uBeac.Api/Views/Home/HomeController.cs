using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace uBeac.Api.Views.Home
{
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            ViewData["Message"] = "Default page.";
            return View();
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "oidc")]
        public IActionResult Secure()
        {
            ViewData["Message"] = "Secure page.";
            return View();
        }

        [HttpGet]
        public async Task Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            await HttpContext.SignOutAsync("oidc");
        }


        public string GetUserId()
        {
            ClaimsPrincipal principal = User;
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
