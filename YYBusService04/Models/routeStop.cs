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
    using System.ComponentModel.DataAnnotations;
    
    public partial class routeStop
    {
        public int routeStopId { get; set; }
        public string busRouteCode { get; set; }
        public Nullable<int> busStopNumber { get; set; }

        [Required]
        public Nullable<int> offsetMinutes { get; set; }
    
        public virtual busRoute busRoute { get; set; }
        public virtual busStop busStop { get; set; }
    }
}