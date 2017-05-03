﻿using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BoxOptions.Core;
using BoxOptions.Public.Models;
using Microsoft.AspNetCore.Mvc;

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
            var entities = await _logRepository.GetRange(DateTime.ParseExact(dateFrom, format, CultureInfo.InvariantCulture), DateTime.ParseExact(dateTo, format, CultureInfo.InvariantCulture).AddDays(1), clientId);
            return entities.Select(e => new LogModel
            {
                ClientId = e.ClientId,
                EventCode = e.EventCode,
                Message = e.Message,
                Timestamp = e.Timestamp
            }).ToArray();
        }
    }
}