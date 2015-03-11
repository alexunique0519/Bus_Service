using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YYBusService04.Models.ViewModels
{
    public class StopRouteView
    {
        public int routeStopId { get; set; }
        public int busStopNumber { get; set; }
        public string Location { get; set; }
        public string busRouteCode { get; set; }
        public string routeName { get; set; }
    }
}