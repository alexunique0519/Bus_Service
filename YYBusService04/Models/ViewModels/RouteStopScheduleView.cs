using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YYBusService04.Models
{
    public class RouteStopScheduleView
    {
        public int routeScheduleId { set; get; }
        public string busRouteCode { set; get; }
        public string routeName { set; get; }
        public int busStopNumber { set; get; }
        public string location { set; get; }
        public TimeSpan arriveTime { set; get; }
        public bool isWeekday { set; get; }
    }
}