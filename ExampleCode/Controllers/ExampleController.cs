using ExampleCode.DTOs;
using ExampleCode.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace ExampleCode.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExampleController : ControllerBase
    {
        private readonly ILogger<ExampleController> _logger;
        private readonly ReportService _reportService;

        public ExampleController(ILogger<ExampleController> logger, ReportService reportService)
        {
            _logger = logger;
            _reportService = reportService;
        }

        [HttpGet]
        public ActionResult Get(string objectId, DateTime begin, DateTime end, TimeTrackingTypeBuild typeBuild)
        {
            try
            {
                //Проверки ...
                var viewModel = _reportService.GetTimeTrackingReport(objectId, begin, end, typeBuild);

                return Ok(viewModel);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
