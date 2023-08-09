using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Carwash.Models.ViewModel
{
    public class AppointmentUserViewModel
    {
        public int appointment_id { get; set; }
        public System.DateTime date { get; set; }
        public System.TimeSpan time { get; set; }

        public int phone { get; set; }
        public string email { get; set; }

        public string name { get; set; }
    }
}