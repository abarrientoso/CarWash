//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Carwash.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class product_stock
    {
        public int id_stock { get; set; }
        public Nullable<int> id_vendor { get; set; }
        public string product_name { get; set; }
        public string description { get; set; }
        public Nullable<int> quantity { get; set; }
        public string size { get; set; }
        public Nullable<decimal> price { get; set; }
        public string vendor_name { get; set; }
        public byte[] product_image { get; set; }
    
        public virtual Vendors Vendors { get; set; }
    }
}
