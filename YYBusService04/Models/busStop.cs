//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace YYBusService04.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class busStop
    {
        public busStop()
        {
            this.routeStops = new HashSet<routeStop>();
            this.tripStops = new HashSet<tripStop>();
        }
    
        public int busStopNumber { get; set; }
        public string location { get; set; }
        public int locationHash { get; set; }
        public bool goingDowntown { get; set; }
    
        public virtual ICollection<routeStop> routeStops { get; set; }
        public virtual ICollection<tripStop> tripStops { get; set; }
    }
}
