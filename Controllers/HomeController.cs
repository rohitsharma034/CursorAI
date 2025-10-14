using Microsoft.AspNetCore.Mvc;
using InmateSearchWebApp.Models;
using InmateSearchWebApp.Services;

namespace InmateSearchWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly InmateSearchService _inmateSearchService;
        private readonly ILogger<HomeController> _logger;

        private static readonly string appId = "YOUR_APP_ID";
        private static readonly string secret = "YOUR_SECRET";
        private static readonly string tppCertificatePath = @"path\to\your\certificate.pfx";
        private static readonly string tppCertificatePassword = "your_certificate_password";
        private static readonly string apiUrl = "https://www.saltedge.com/api/payments/v1/payments/connect";

        public HomeController(InmateSearchService inmateSearchService, ILogger<HomeController> logger)
        {
            _inmateSearchService = inmateSearchService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(new InmateSearchRequest());
        }

        [HttpPost]
        public async Task<IActionResult> SearchInmate(InmateSearchRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("Index", request);
                }

                _logger.LogInformation("Starting inmate search for {LastName}", request.LastName);
                
                var result = await _inmateSearchService.RunAsync(
                    request.LastName, 
                    request.Username, 
                    request.Address, 
                    request.FirstName
                );

                ViewBag.Result = result;
                ViewBag.Success = true;
                
                _logger.LogInformation("Inmate search completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during inmate search");
                ViewBag.Result = $"Error: {ex.Message}";
                ViewBag.Success = false;
            }

            return View("Index", request);
        }
    }
}
