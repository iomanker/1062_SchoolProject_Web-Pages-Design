using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Timers;

using System.Data.SqlClient;
using System.Web.Configuration;

namespace ActivityInstantReceiver
{
    public class eventTimer
    {
        private int eventId;
        private Timer timer;
        private int counter;
        public eventTimer(int timelong, int id)
        {
            this.timer = new Timer();
            this.timer.Enabled = true;
            this.timer.Interval = 1000;
            this.counter = timelong;
            this.eventId = id;
            this.timer.Elapsed += addSecond;
            handleTimingEvent(0, id, 1);
        }

        public int getCounter()
        {
            return counter;
        }

        public int getId()
        {
            return eventId;
        }

        private void addSecond(Object source, ElapsedEventArgs e)
        {
            --this.counter;
            if (this.counter <= 0){
                handleTimingEvent(1, this.eventId, 0);
                this.counter = -1000;
                this.timer.Enabled = false;
                IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<eventReceiver>();
                hubContext.Clients.All.timerEventTimesup(eventId);
            }
        }
        static public void handleTimingEvent(int type, int eventId, int processing)
        {
            string Database = WebConfigurationManager.ConnectionStrings["Database"].ConnectionString;
            SqlConnection Conn = new SqlConnection(Database);
            string command = "";
            // type = 1:update, 0:Insert
            if (type == 0)
                command = "INSERT INTO [eventProcessing] ([processing],[eventId]) VALUES (" + processing + "," + eventId + ")";
            else
                if (type == 1)
                command = "UPDATE [eventProcessing] SET [processing] = " + processing + " WHERE [eventId] = " + eventId;
            if (command != ""){
                SqlCommand cmd = new SqlCommand(command, Conn);
                try
                {
                    Conn.Open();
                    cmd.ExecuteNonQuery();
                }catch (Exception ex){
                    System.Diagnostics.Debug.WriteLine("handleTimingEvent錯誤: " + ex.Message.ToString());
                }
                finally{
                    cmd.Cancel(); cmd.Dispose();
                    if (Conn.State == System.Data.ConnectionState.Open)
                    {
                        Conn.Close();
                        Conn.Dispose();
                    }
                }
            }
        }
    }

    public class timeQueue
    {
        private List<eventTimer> queue;
        private Timer timer;
        public timeQueue()
        {
            this.queue = new List<eventTimer>();
            this.timer = new Timer();
            this.timer.Enabled = true;
            this.timer.Interval = 1000;
            this.timer.Elapsed += check_eachItem;
        }

        public void push_back(eventTimer item)
        {
            queue.Add(item);
        }

        private void check_eachItem(Object source, ElapsedEventArgs e)
        {
            List<int> deleteIndex = new List<int>();
            for(int i = 0; i < queue.Count; ++i)
            {
                if (queue[i].getCounter() <= -1000)
                {
                    deleteIndex.Add(i);
                }
            }
            for(int i = 0; i < deleteIndex.Count; ++i)
            {
                queue.RemoveAt(deleteIndex[i]);
            }
        }

        public void deleteEventTrigger(int eventId)
        {
            int findIndex = queue.FindIndex(x => x.getId() == eventId);
            if (findIndex != -1){
                queue.RemoveAt(findIndex);
            }
        }
    }
}