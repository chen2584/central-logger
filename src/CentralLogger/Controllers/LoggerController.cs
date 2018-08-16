﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using System.Web;
using CentralLogger.Model;
using System.Globalization;

namespace CentralLogger.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class LoggerController : ControllerBase
    {

        private readonly CentralLoggerContext db;
        public LoggerController(CentralLoggerContext _db)
        {
            db = _db;
        }


        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> ShowAll()
        {
            try
            {
                var Logger = db.LogInfos.OrderBy(x => x.Id).ToList();
                return Ok(Logger);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }

        }
        [HttpPost]
        public ActionResult<List<LogInfo>> Search(SearchLog search)
        {
            search.StartDate = search.StartDate.ToLocalTime();
            search.EndDate = search.EndDate.ToLocalTime();
            var data = db.LogInfos.Where(x => x.DateTime >= search.StartDate && x.DateTime <= search.EndDate);

            if (!string.IsNullOrEmpty(search.IpNow))
            {
                data = data.Where(x => x.Ip.Equals(search.IpNow));
            }
            if (!string.IsNullOrEmpty(search.Appnow))
            {
                data = data.Where(x => x.Application.Equals(search.Appnow));
            }

            var result = data.OrderByDescending(x => x.DateTime).ToList();

            return result;
        }

        [HttpGet]
        public IEnumerable<string> getIP()
        {
            var Ip = db.LogInfos.Select(m => m.Ip).Distinct();
            return Ip.ToList();
        }

        [HttpGet]
        public IEnumerable<string> getApp()
        {
            var App = db.LogInfos.Select(m => m.Application).Distinct();
            return App.ToList();
        }
        [HttpPost]
        public ActionResult WriteLog([FromBody]GetLogInfos x)
        {
            //var requestIp = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList.GetValue(0).ToString();
            var requestIp = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            if (requestIp == "::1")
            {
                requestIp = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList.GetValue(0).ToString();
            }
            // var requestIp = System.Net.Dns.GetHostName();
            var date = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff");
            var time = DateTime.Now;

            db.LogInfos.Add(new LogInfo()
            {
                LogLevel = x.LogLevel,
                Message = x.Message,
                DateTime = DateTime.Now,
                Application = x.Application,
                Ip = requestIp
            });
            db.SaveChanges();
            return Ok();
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public async void Delete(int id)
        {
            await db.Database.EnsureDeletedAsync();
        }
    }
}
