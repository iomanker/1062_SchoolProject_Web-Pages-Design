using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActivityInstantReceiver.Models
{
    public class eventSummary
    {
        public int Id { get; set; }
        public string eventName { get; set; }
        public DateTime postTime { get; set; }
        public string postType { get; set; }
        public DateTime validTime { get; set; }
    }

    public class eventDetail
    {
        // public int Id { get; set; }
        public string eventName { get; set; }
        public DateTime postTime { get; set; }
        public string postType { get; set; }
        public DateTime validTime { get; set; }
        public string content { get; set; }
    }
}