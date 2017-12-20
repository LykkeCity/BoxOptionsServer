using BoxOptions.Common.Interfaces;
using BoxOptions.Common.Models;
using BoxOptions.Core.Repositories;
using BoxOptions.Public.Models;
using BoxOptions.Public.ViewModels;
using BoxOptions.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoxOptions.Public.Controllers
{
    [Route("api/[controller]")]
    public class LogController : Controller
    {
        private readonly ILogRepository _logRepository;
        private readonly IGameDatabase _gameDatabase;        
        private static CultureInfo Ci = new CultureInfo("en-us");

        public LogController(ILogRepository logRepository, IGameDatabase gameDatabase)
        {
            _logRepository = logRepository;
            _gameDatabase = gameDatabase;
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] LogModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            double accountdelta = 0;
            if (model.EventCode == "8")// BetPlaced
            {
                //string test = "Coeff: 1.24830386982552, Bet: 1.0";
                //model.Message = test;

                int index = model.Message.IndexOf("Bet:");
                string betvalue = model.Message.Substring(index, model.Message.Length - index).Replace("Bet:", "").Trim();
                double.TryParse(betvalue, NumberStyles.AllowDecimalPoint, Ci, out accountdelta);
                if (accountdelta > 0)
                    accountdelta = -accountdelta;
            }
            else if (model.EventCode == "9")// BetWon
            {
                //string test = "Value: 1.24830386982552";
                //model.Message = test;

                string winvalue = model.Message.Replace("Value:", "").Trim();
                double.TryParse(winvalue, NumberStyles.AllowDecimalPoint, Ci, out accountdelta);
            }


            await _logRepository.InsertAsync(new LogItem
            {
                ClientId = model.ClientId,
                EventCode = model.EventCode,
                Message = model.Message,
                AccountDelta = accountdelta
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
                Timestamp = e.Date.ToString("u")
            }).ToArray();
        }

       
        [HttpGet]
        [Route("getall")]
        public async Task<ActionResult> GetAll([FromQuery] string dateFrom, [FromQuery] string dateTo)
        {
            const string format = "yyyyMMdd";
            var entities = await _logRepository.GetAll(DateTime.ParseExact(dateFrom, format, CultureInfo.InvariantCulture), DateTime.ParseExact(dateTo, format, CultureInfo.InvariantCulture).AddDays(1));

            var res = entities.Select(e => new LogModel
            {
                ClientId = e.ClientId,
                EventCode = e.EventCode,
                Message = e.Message,
                Timestamp = e.Date.ToString("u")
            }).ToArray();

            return View(res);

        }

        [HttpGet]
        [Route("clientlogs")]
        public async Task<ActionResult> ClientLogs()
        {
            try
            {
                ClientLogsViewModel model = new ClientLogsViewModel()
                {
                    EndDate = DateTime.UtcNow.Date,
                    StartDate = DateTime.UtcNow.Date.AddDays(-30)
                };

                var entities = await _logRepository.GetClients();
                model.ClientList = (from l in entities
                                    select l.Length > 36 ? l.Substring(0, 36) : l).Distinct().ToArray();
                return View(model);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost]
        [Route("clientlogs")]
        public async Task<ActionResult> ClientLogs(ClientLogsViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var entities = await _logRepository.GetRange(model.StartDate, model.EndDate.AddDays(1), model.Client);
                    if (entities.Count() > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine($"Timestamp;ClientId;EventCode;Message");
                        foreach (var item in entities)
                        {
                            sb.AppendLine($"{item.Date.ToString("u")};{item.ClientId};{item.EventCode}-{(Common.GameStatus)int.Parse(item.EventCode)};{item.Message.Replace(';', '|')}");
                        }
                        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"clientLogs_{model.Client}.csv");
                    }
                    else
                    {
                        return StatusCode(500, "Log file is empty");
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, ex.Message);
                }
            }
            else
                return StatusCode(500, "Invalid model");
        }

        [HttpGet]
        [Route("userhistory")]
        public async Task<ActionResult> UserHistory()
        {
            try
            {
                ClientLogsViewModel model = new ClientLogsViewModel()
                {
                    EndDate = DateTime.UtcNow.Date,
                    StartDate = DateTime.UtcNow.Date.AddDays(-30)
                };

                var entities = await _gameDatabase.GetUsers();
                model.ClientList = entities.Distinct().ToArray();
                return View(model);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost]
        [Route("userhistory")]
        public async Task<ActionResult> UserHistory(ClientLogsViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (model.GameHistory)
                    {
                        var entities = await _gameDatabase.GetGameBetsByUser(model.Client, model.StartDate, model.EndDate);
                        if (entities.Count() > 0)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine("Date;UserId;AssetPair;BetAmount;BoxId;BetStatus;Parameters;Box");
                            foreach (var item in entities.OrderBy(m=>m.Date))
                            {
                                sb.AppendLine($"{item.Date};{item.UserId};{item.AssetPair};{item.BetAmount};{item.BoxId};{item.BetStatus}-{(BetStates)item.BetStatus};{item.Parameters.Replace(';','|')};{item.Box.Replace(';', '|')}");
                            }
                            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"userHistory_{model.Client}.csv");
                        }
                        else
                        {
                            return StatusCode(500, "Log file is empty");
                        }
                    }
                    else
                    {
                        var entities = await _gameDatabase.LoadUserHistory(model.Client, model.StartDate, model.EndDate);
                        if (entities.Count() > 0)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine("Date;UserId;Status;Message");
                            foreach (var item in entities)
                            {
                                sb.AppendLine($"{item.Date.ToString("u")};{item.UserId};{item.GameStatus}-{(Common.GameStatus)item.GameStatus};{item.Message.Replace(';', '|')}");
                            }
                            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"userHistory_{model.Client}.csv");
                        }
                        else
                        {
                            return StatusCode(500, "Log file is empty");
                        }
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, ex.Message);
                }
            }
            else
                return StatusCode(500, "Invalid model");
        }
    }
}
