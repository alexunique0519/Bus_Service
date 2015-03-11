using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace YYBusService04.Models.ViewModels
{
    public class TripBusDriverView
    {
        public int tripId;
        
        public string tripDate;
        
        public TimeSpan startTime;
       
        public string driverName;
        
        public int busNumber;
       
        public string comments;
    }
}