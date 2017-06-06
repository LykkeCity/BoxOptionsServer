using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BoxOptions.Core;
using BoxOptions.Public.Models;
using Microsoft.AspNetCore.Mvc;
using BoxOptions.Core.Models;

namespace BoxOptions.Public.Controllers
{
    [Route("api/[controller]")]
    public class LogController : Controller
    {
        private readonly ILogRepository _logRepository;

        public LogController(ILogRepository logRepository)
        {
            _logRepository = logRepository;
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] LogModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _logRepository.InsertAsync(new LogItem
            {
                ClientId = model.ClientId,
                EventCode = model.EventCode,
                Message = model.Message
            });

            return Ok();
        }

        [HttpGet]
        public async Task<LogModel[]> Get([FromQuery] string dateFrom, [FromQuery] string dateTo,
            [FromQuery] string clientId)
        {
            const string format = "yyyyMMdd";
            var entities = await _logRepository.GetRange(DateTime.ParseExact(dateFrom, format, CultureInfo.InvariantCulture), DateTime.ParseExact(dateTo, format, CultureInfo.InvariantCulture), clientId);
            return entities.Select(e => new LogModel
            {
                ClientId = e.ClientId,
                EventCode = e.EventCode,
                Message = e.Message,
                Timestamp = e.Timestamp
            }).ToArray();
        }

        //[HttpGet]
        //[Route("boxoptionlogclientlist")]
        //public async Task<IActionResult> ClientList([FromQuery] string dateFrom, [FromQuery] string dateTo)
        //{
        //    try
        //    {
        //        const string format = "yyyyMMdd";
        //        var entities = await _logRepository.GetClients(DateTime.ParseExact(dateFrom, format, CultureInfo.InvariantCulture), DateTime.ParseExact(dateTo, format, CultureInfo.InvariantCulture).AddDays(1));

        //        return Ok(entities);
        //    }
        //    catch (Exception ex) { return StatusCode(500, ex.Message); }
        //}

        //[HttpGet]
        //[Route("getall")]
        //public async Task<ActionResult> GetAll([FromQuery] string dateFrom, [FromQuery] string dateTo)
        //{
        //    const string format = "yyyyMMdd";
        //    var entities = await _logRepository.GetAll(DateTime.ParseExact(dateFrom, format, CultureInfo.InvariantCulture), DateTime.ParseExact(dateTo, format, CultureInfo.InvariantCulture).AddDays(1));
            
        //    var res = entities.Select(e => new LogModel
        //    {
        //        ClientId = e.ClientId,
        //        EventCode = e.EventCode,
        //        Message = e.Message,
        //        Timestamp = e.Timestamp
        //    }).ToArray();

        //    return View(res);

        //}
    }
}
