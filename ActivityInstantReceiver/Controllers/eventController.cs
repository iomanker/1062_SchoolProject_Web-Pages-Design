using ActivityInstantReceiver.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Configuration;
using Newtonsoft.Json.Linq;
using Microsoft.AspNet.SignalR;

namespace ActivityInstantReceiver.Controllers
{
    public class GetRange
    {
        public int offset { get; set; } = -1;
    }
    public class eventController : ApiController
    {
        string Database = WebConfigurationManager.ConnectionStrings["Database"].ConnectionString;
        // GET: api/event
        public IEnumerable<eventSummary> Get([FromUri]GetRange range)
        {
            string command = "SELECT [Id],[eventName],[postTime],[postType],[validTime] FROM [EventDetail] ORDER BY Id DESC";
            if (range != null && range.offset >= 0){
                command += " OFFSET " + range.offset + " ROWS FETCH FIRST 8 ROWS ONLY";
            }
            SqlConnection Conn = new SqlConnection(Database);
            SqlDataReader dr = null;
            SqlCommand cmd = new SqlCommand(command, Conn);
            List<eventSummary> models = new List<eventSummary>();
            try
            {
                Conn.Open();
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var detail = new eventSummary();
                    detail.Id = (int)dr["Id"];
                    detail.eventName = dr["eventName"].ToString();
                    detail.postTime = (DateTime)dr["postTime"];
                    detail.postType = dr["postType"].ToString();
                    if (dr["validTime"].ToString() == "")
                        detail.validTime = DateTime.Now;
                    else
                        detail.validTime = (DateTime)dr["validTime"];
                    models.Add(detail);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message.ToString());
            }
            finally
            {
                if (dr != null)
                {
                    cmd.Cancel();
                    cmd.Dispose();
                    dr.Close();
                }
                if (Conn.State == System.Data.ConnectionState.Open)
                {
                    Conn.Close();
                    Conn.Dispose();
                }
            }
            return models.ToArray();
            // return new string[] { "value1", "value2" };
        }

        // GET: api/event/5
        public IEnumerable<eventDetail> Get(int id)
        {
            SqlConnection Conn = new SqlConnection(Database);
            SqlDataReader dr = null;
            string command = "SELECT * FROM [EventDetail] WHERE [Id] = " + id;
            SqlCommand cmd = new SqlCommand(command, Conn);
            List<eventDetail> models = new List<eventDetail>();
            try{
                Conn.Open();
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var detail = new eventDetail();
                    detail.eventName = dr["eventName"].ToString();
                    detail.postTime = (DateTime)dr["postTime"];
                    detail.postType = dr["postType"].ToString();
                    if (dr["validTime"].ToString() == "")
                        detail.validTime = DateTime.Now;
                    else
                        detail.validTime = (DateTime)dr["validTime"];
                    detail.content = dr["content"].ToString();
                    models.Add(detail);
                }
            }catch (Exception ex){
                System.Diagnostics.Debug.WriteLine(ex.Message.ToString());
            }
            finally{
                if (dr != null)
                {
                    cmd.Cancel();
                    cmd.Dispose();
                    dr.Close();
                }
                if (Conn.State == System.Data.ConnectionState.Open)
                {
                    Conn.Close();
                    Conn.Dispose();
                }
            }
            return models.ToArray();
            // return "value";
        }

        // POST: api/event
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/event/5
        public HttpResponseMessage Put(int id, [FromBody]JObject item)
        {
            string command = "UPDATE [EventDetail] SET [eventName] = @eventName, [content] = @content WHERE [Id] = " + id;
            string eventName = item["eventName"].ToString();
            string content = item["content"].ToString();
            SqlConnection Conn = new SqlConnection(Database);
            SqlCommand cmd = new SqlCommand(command, Conn);
            string str_err = "";
            int result = 0;
            try
            {
                Conn.Open();
                cmd.Parameters.AddWithValue("@eventName", eventName);
                cmd.Parameters.AddWithValue("@content", content);
                result = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                str_err = ex.Message.ToString();
                System.Diagnostics.Debug.WriteLine(str_err);
            }
            finally
            {
                cmd.Cancel(); cmd.Dispose();
                if (Conn.State == System.Data.ConnectionState.Open)
                {
                    Conn.Close();
                    Conn.Dispose();
                }
                // return new string[] { "value1", "value2" };
            }
            if (result != 0)
            {
                IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<eventReceiver>();
                hubContext.Clients.All.updateEvent(id, eventName);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadGateway, str_err);
            }
        }

        // DELETE: api/event/5
        public HttpResponseMessage Delete(int id)
        {
            string command = "DELETE FROM [EventDetail] WHERE [Id] = " + id;

            SqlConnection Conn = new SqlConnection(Database);
            SqlCommand cmd = new SqlCommand(command, Conn);
            string str_err = "";
            int result = 0;
            try
            {
                Conn.Open();
                result = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                str_err = ex.Message.ToString();
            }
            finally
            {
                cmd.Cancel(); cmd.Dispose();
                if (Conn.State == System.Data.ConnectionState.Open)
                {
                    Conn.Close();
                    Conn.Dispose();
                }
                // return new string[] { "value1", "value2" };
            }
            if (result != 0)
            {
                IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<eventReceiver>();
                hubContext.Clients.All.deleteEvent(id);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadGateway, str_err);
            }
        }
    }
}
