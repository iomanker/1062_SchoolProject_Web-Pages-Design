using ActivityInstantReceiver.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Configuration;
using System.Data.SqlClient;
using Newtonsoft.Json.Linq;

namespace ActivityInstantReceiver.Controllers
{
    public class accountController : ApiController
    {
        string Database = WebConfigurationManager.ConnectionStrings["Database"].ConnectionString;
        // GET: api/account
        public IEnumerable<account> Get()
        {
            string command = "SELECT * FROM [User] ORDER BY Id ASC";
            SqlConnection Conn = new SqlConnection(Database);
            SqlDataReader dr = null;
            SqlCommand cmd = new SqlCommand(command, Conn);
            List<account> models = new List<account>();
            try
            {
                Conn.Open();
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var detail = new account();
                    detail.Id = (int)dr["Id"];
                    detail.username = dr["username"].ToString();
                    detail.password = dr["password"].ToString();
                    detail.authority = (int)dr["authority"];
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
        }

        // GET: api/account/5
        /*public string Get(int id)
        {
            return "value";
        }*/

        // POST: api/account
        public HttpResponseMessage Post([FromBody]JObject item)
        {
            string Action = item["action"].ToString();
            string Username = item["username"].ToString();
            string Password = item["password"].ToString();
            string Authority = "";
            if (Action == "signup")
                Authority = item["authority"].ToString();
            string command = "";
            account temp = null;
            try
            {
                bool isFound = checkAccount(Username, ref temp);
                switch (Action)
                {
                    case "login":
                        if (temp != null && temp.password == Password)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new {
                                Id = temp.Id,
                                authority = temp.authority
                            });
                        }else{
                            if (temp != null)
                            {
                                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "密碼錯誤");
                            }else{
                                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "尚未申請的帳號");
                            }
                        }
                        break;
                    case "signup":
                        if (temp != null)
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "帳號已註冊");
                        }else{
                            command = "INSERT INTO [User] ([username],[password],[authority]) VALUES ('" + Username + "','" + Password + "'," + Authority + ")";
                            int tempId = addAccount(command);
                            return Request.CreateResponse(HttpStatusCode.OK, new
                            {
                                Id = tempId
                            });
                        }
                        break;
                    default:
                        return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "非合法方式");
                }
            }
            catch(Exception ex){
                System.Diagnostics.Debug.WriteLine(ex.Message.ToString());
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }
            // return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "不明原因發生");
        }

        // PUT: api/account/5
        public HttpResponseMessage Put(int id, [FromBody]JObject item)
        {
            string command = "UPDATE [User] SET [password] = @password, [authority] = @authority WHERE [Id] = " + id;
            string password = item["password"].ToString();
            string authority = item["authority"].ToString();
            SqlConnection Conn = new SqlConnection(Database);
            SqlCommand cmd = new SqlCommand(command, Conn);
            string str_err = "";
            int result = 0;
            try
            {
                Conn.Open();
                cmd.Parameters.AddWithValue("@password", password);
                cmd.Parameters.AddWithValue("@authority", authority);
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
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadGateway, str_err);
            }
        }

        // DELETE: api/account/5
        public HttpResponseMessage Delete(int id)
        {
            string command = "DELETE FROM [User] WHERE [Id] = " + id;

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
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadGateway, str_err);
            }
        }

        private bool checkAccount(string username, ref account item)
        {
            string command = command = "SELECT [Id],[password],[authority] FROM [User] WHERE [username] = @username";
            SqlConnection Conn = new SqlConnection(Database);
            SqlCommand cmd = new SqlCommand(command, Conn);
            SqlDataReader dr = null;
            bool found = false;
            try
            {
                Conn.Open();
                cmd.Parameters.AddWithValue("@username", username);
                dr = cmd.ExecuteReader();
                if (dr.Read()){
                    found = true;
                    item = new account();
                    item.Id = (int)dr["Id"];
                    item.password = dr["password"].ToString();
                    item.authority = (int)dr["authority"];
                }
                else{
                    item = null;
                    found = false;
                }
            }catch(Exception ex){
                throw new Exception("確認帳號時發生錯誤: " + ex.Message);
            }finally{
                if (dr != null){
                    cmd.Cancel(); cmd.Dispose(); dr.Close();
                }
                if (Conn.State == System.Data.ConnectionState.Open){
                    Conn.Close(); Conn.Dispose();
                }
            }
            return found;
        }

        private int addAccount(string command)
        {
            command += ";SELECT SCOPE_IDENTITY();";
            SqlConnection Conn = new SqlConnection(Database);
            SqlCommand cmd = new SqlCommand(command, Conn);
            int resultValue = -1;
            try
            {
                Conn.Open();
                resultValue = (int)(decimal)cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw new Exception("新增帳號發生錯誤: " + ex.Message);
            }
            finally{
                cmd.Cancel(); cmd.Dispose();
                if (Conn.State == System.Data.ConnectionState.Open)
                {
                    Conn.Close();
                    Conn.Dispose();
                }
            }
            if (resultValue == -1)
                throw new Exception("資料庫發生不明錯誤: " + resultValue);
            return resultValue;
        }
    }
}
