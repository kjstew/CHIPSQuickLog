using System;
using System.Collections.Generic;
using System.Text;

namespace CHIPSQuickLog
{
    class Consultation
    {
        public int Id;
        public DateTime Time;
        public string UserName;

        public Consultation(DateTime time, string username)
        {
            this.Time = time;
            // tried adding new field to db table but api requests failed -- investigate later
            // this.EmployeeName = employeeName;
            this.UserName = username;
        }
    }
}
