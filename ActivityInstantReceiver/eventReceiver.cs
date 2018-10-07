using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

using System.Data.SqlClient;
using System.Web.Configuration;

using static ActivityInstantReceiver.eventTimer;

namespace ActivityInstantReceiver
{
    public class eventReceiver : Hub
    {
        timeQueue objoftimeQueue = new timeQueue();
        public void postEvent(string eventName,string postType,string content, string postTime, int validSecond)
        {
            try { 
                DateTime validTime = DateTime.Now;
                validTime = validTime.AddSeconds((double)validSecond);
                int resultState = -1;
                while (resultState < 0) {
                    resultState = postSQLEventDetail(eventName, postType, content, validTime);
                }
                // 2018/05/24
                if (postType == "timer"){
                    objoftimeQueue.push_back(new eventTimer(validSecond, resultState));
                }
                Clients.All.sendEvent(resultState , eventName, postType, postTime, 
                    validTime.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"));
            }
            catch(Exception ex){
                System.Diagnostics.Debug.WriteLine("SignalR錯誤： " + ex.Message.ToString());
            }
        }

        string Database = WebConfigurationManager.ConnectionStrings["Database"].ConnectionString;
        private int postSQLEventDetail(string eventName, string postType, string content, DateTime validTime)
        {
            SqlConnection Conn = new SqlConnection(Database);
            string[] param = { "eventName", "activityId", "postType", "content", "validTime" };
            string command = makingSQLInsertCmd(param, "EventDetail");
            command += ";SELECT SCOPE_IDENTITY();";
            SqlCommand cmd = new SqlCommand(command, Conn);
            int resultValue = -1;
            try
            {
                cmd.Parameters.AddWithValue("@" + param[0], eventName);
                cmd.Parameters.AddWithValue("@" + param[1], 0);
                cmd.Parameters.AddWithValue("@" + param[2], postType);
                cmd.Parameters.AddWithValue("@" + param[3], content);
                cmd.Parameters.AddWithValue("@" + param[4], validTime);
                Conn.Open();
                resultValue = (int)(decimal)cmd.ExecuteScalar();
                // System.Diagnostics.Debug.WriteLine("ExecuteScalar returns " + resultValue);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("資料庫錯誤: " + ex.Message.ToString());
            }
            finally
            {
                cmd.Cancel(); cmd.Dispose();
                if (Conn.State == System.Data.ConnectionState.Open)
                {
                    Conn.Close();
                    Conn.Dispose();
                }
            }
            return resultValue;
        }

        static public string makingSQLInsertCmd(string[] param, string tableName)
        {
            string result = "INSERT INTO [" + tableName + "] (";
            bool startComma = false;
            foreach(string temp in param)
            {
                if (startComma)
                    result += ", [" + temp + "]";
                else
                {
                    startComma = true;
                    result += "[" + temp + "]";
                }
            }
            result += ") VALUES (";
            startComma = false;
            foreach (string temp in param)
            {
                if (startComma)
                    result += ", @" + temp;
                else
                {
                    startComma = true;
                    result += "@" + temp;
                }
            }
            result += ")";
            return result;
        }
    }
}