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
    
    public partial class driver
    {
        public driver()
        {
            this.trips = new HashSet<trip>();
        }
        
        [Display (Name="Driver")]
        public int driverId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string fullName { get; set; }
        public string homePhone { get; set; }
        public string workPhone { get; set; }
    
        public virtual ICollection<trip> trips { get; set; }
    }
}
