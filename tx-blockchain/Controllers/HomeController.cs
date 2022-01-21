using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using tx_blockchain.Models;

namespace tx_blockchain.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Blockchain()
        {
            Blockchain bcDocument = new Blockchain("App_Data/Blockchains/documents.bc");

            return View(bcDocument);
        }

        public IActionResult Sign()
        {
            SignViewModel model = new SignViewModel()
            {
                Document = Convert.ToBase64String(
                System.IO.File.ReadAllBytes("App_Data/nda.tx"))
            };

            return View(model);
        }
        public IActionResult Validate(string blockHash)
        {
            ValidationModel model = new ValidationModel() { BlockHash = blockHash };

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}