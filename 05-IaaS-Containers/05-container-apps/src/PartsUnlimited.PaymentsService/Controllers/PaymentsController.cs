using Microsoft.AspNetCore.Mvc;

namespace PartsUnlimited.PaymentsService.Controllers;

[ApiController]
[Route("[controller]")]
public class PaymentsController : ControllerBase
{
    private const string PromoCode = "FREE"; // Paul

    private static readonly string[] Summaries = new[]
    {
        "Processing"
    };

    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(ILogger<PaymentsController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetPayments")]
    public IEnumerable<Payments> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new Payments
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        }).ToArray();
    }
    [HttpPost]
    public async Task <Boolean> Post()
    {
        Console.WriteLine("PaymentsController did it's job. Yeey!");
        return true;
    }
}
