using ActivityInstantReceiver.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ActivityInstantReceiver.Controllers
{
    public class serverTimeController : ApiController
    {
        // GET: api/serverTime
        public IEnumerable<string> Get()
        {
            return new string[] {DateTime.Now.ToUniversalTime()
                         .ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'") };
        }
        /*
        // GET: api/serverTime/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/serverTime
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/serverTime/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/serverTime/5
        public void Delete(int id)
        {
        }*/
    }
}
