using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Configuration;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace ESIAppTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;
        public static IConfiguration _configuration;
        public WeatherForecastController(ILogger<WeatherForecastController> logger,IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }


        //[HttpPost("ApplyDiamondESITestAPI", Name = "ApplyDiamondESITestAPI")]
        //public async Task<IActionResult> ApplyDiamondESITestAPI(ApplyDiamondAndPrintLabelIMSRequestDto applyDiamondAndPrintLabelIMSRequest)
        //{
        //    var response = new ApplyDiamondPrintLabelResponse();
        //    try
        //    {
        //        response = await _serviceManager.ApplyDiamondLogicService.ApplyDiamondESITestAPI(applyDiamondAndPrintLabelIMSRequest);

        //        return Ok(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex.InnerException.Message);
        //        // response.Message = ex.ToString();
        //        //response.Status = "Error";
        //        return BadRequest(ex.InnerException);
        //    }






        //}

        [HttpPost("ApplyDiamondESITestAPI", Name = "ApplyDiamondESITestAPI")]
        public async Task<IActionResult> ApplyDiamondESITestAPI(ApplyDiamondAndPrintLabelIMSRequestDto applyDiamondAndPrintLabelIMSRequest)
        {
            var apiResponse = new HttpResponseMessage();
            var httpResponseMessage = new ApplyDiamondPrintLabelResponse();
            try
            {
                
                string token = GetESIAuthToken();
                string apiCall = "applyStones";
                string baseApiUrl = "https://esi.test.cloud.jewels.com/custom-import/dds-ims/ims/v1/";
                if (baseApiUrl != null)
                {   
                    JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    };
                    var json = JsonConvert.SerializeObject(applyDiamondAndPrintLabelIMSRequest, Formatting.Indented, jsonSerializerSettings);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    using (var client = new HttpClient())
                    {
                        //_logger.LogInformation($"DDS: Request URL:{baseApiUrl + apiCall}, Request: {json}");
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        Console.WriteLine($"DDS: Request URL:{baseApiUrl + apiCall}, Request: {json} , StartTime : {DateTime.Now}");
                        apiResponse = await client.PostAsync(baseApiUrl + apiCall, content);
                        
                        if (apiResponse != null)
                        {
                            string jsonContent = await apiResponse.Content.ReadAsStringAsync();
                             httpResponseMessage = JsonConvert.DeserializeObject<ApplyDiamondPrintLabelResponse>(jsonContent);
                            Console.WriteLine($"DDS: Response URL:{baseApiUrl + apiCall}, Response: {httpResponseMessage}, EndTime: {DateTime.Now}");
                        }
                    }
                }
                return Ok(httpResponseMessage);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException);
            }

        }
        private static string GetESIAuthToken()
        {
            //IConfiguration configuration;
            string retval;
            try
            {
               
                string ApiUrl = _configuration.GetSection("IMSAuthentication")["JwtBearerAuthority"];
                var ESIAuthTokenContent = new Dictionary<string, string>()
                {
                    {"client_id", _configuration.GetSection("IMSAuthentication")["JwtBearerAudience"] },
                    {"client_secret", _configuration.GetSection("IMSAuthentication")["ApiSecret"] },
                    {"grant_type", _configuration.GetSection("IMSAuthentication")["GrantType"] },
                    {"scope", _configuration.GetSection("IMSAuthentication")["Scope"] }
                };
                using (var httpClient = new HttpClient())
                {
                    using (var response = httpClient.PostAsync(ApiUrl, new FormUrlEncodedContent(ESIAuthTokenContent)))
                    {
                        string apiResponse = response.Result.Content.ReadAsStringAsync().Result;
                        var obj = JsonConvert.DeserializeObject<Dictionary<string, string>>(apiResponse);
                        retval = obj["access_token"];
                    }

                }

            }
            catch (Exception ex)
            { retval = default(string); }
            return retval;
        }
        public class ApplyDiamondAndPrintLabelIMSRequestDto
        {
            public string? Brand { get; set; }
            public decimal? CaratWeight { get; set; }
            public long? EnvelopeNumber { get; set; }
            public string? ImsUserId { get; set; }
            // public string? OrderDueDate { get; set; }
            public string? OrderSubType { get; set; }
            public string? OrderType { get; set; }
            public int? Quantity { get; set; }
            public long? Sku { get; set; }
            public string SourceOrder { get; set; }
            public string? TransactionId { get; set; }
            public string? WorkOrder { get; set; }
            public string? WorkOrderLine { get; set; }
            public string? ComputerName { get; set; }
            public string? Size { get; set; }
            public string? Shape { get; set; }
            public string? Clarity { get; set; }
            public string? Color { get; set; }
            public decimal? Cost { get; set; }
            public long WorkOrderId { get; set; }
            public long? ShipToLocationId { get; set; }
        }
        public class CommonAPIResponseBase
        {
            public string Error { get; set; }
            public bool IsSuccessful { get; set; } = false;
            public string Message { get; set; } = string.Empty;
            public string Path { get; set; }
            public string Status { get; set; }
            public DateTime TimeStamp { get; set; }
            public HttpStatusCode StatusCode { get; set; }
        }
        public class ApplyDiamondPrintLabelResponse : CommonAPIResponseBase
        {
            public long? ParcelNumber { get; set; }
            public String? TransactionId { get; set; }
            public String? OrderType { get; set; }
            public String? OrderSubType { get; set; }
            public String? SourceOrder { get; set; }
            public String? WorkOrder { get; set; }
            public long? WorkOrderLine { get; set; }
            public long? PrintShipToLocation { get; set; }
            public String? ImsUserId { get; set; }
            public String? computerName { get; set; }
            public string? Status { get; set; }
            public string? Response { get; set; }

            public string? Quantity { get; set; }
            public long? EnvelopNumber { get; set; }
            public string? Color { get; set; }
            public string? Clarity { get; set; }
            public string? Shape { get; set; }
            public string? Size { get; set; }
        }
    }
}