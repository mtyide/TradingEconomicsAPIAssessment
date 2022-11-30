using Microsoft.AspNetCore.Mvc;
using MVCAssessment.Models;
using Newtonsoft.Json;
using System.Diagnostics;

namespace MVCAssessment.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private string? EndpointUrl { get; set; }

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var key = config.GetValue<string>("API:Key");
            EndpointUrl = $"https://api.tradingeconomics.com/forecast/country/mexico/?client={key}";

            using var client = new HttpClient();
            var view = new ViewModel();
            var request = new HttpRequestMessage(HttpMethod.Get, $"{EndpointUrl}");

            request.Headers.Add("Accept", "application/json");
            request.Content = new StringContent(string.Empty, null, "application/json");

            var response = client.SendAsync(request).Result;
            var result = response.Content.ReadAsStringAsync().Result;
            dynamic? items = JsonConvert.DeserializeObject(result);

            if (items != null)
            {
                var list = new List<DataModel>();
                foreach (var item in items)
                {
                    var country = item.Country;
                    var category = item.Category;
                    var title = item.Title;
                    var latestValue = item.LatestValue;
                    var frequency = item.Frequency;
                    var latestValueDate = item.LatestValueDate;

                    list.Add(new DataModel
                    {
                        Category = category,
                        Country = country,
                        Frequency = frequency,
                        LatestValue = latestValue,
                        Title = title,
                        LatestValueDate = latestValueDate
                    });
                }

                _logger.Log(LogLevel.Information, "TradingEconomics: Fetch Task Completed");
                view.Result = list;
                return View(view);
            }

            _logger.Log(LogLevel.Information, "TradingEconomics: Fetch Task Completed - No Result");
            view.Result = null;
            return View(view);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}