using System;
using System.Collections.Generic;
using System.Text;

namespace CHIPSQuickLog
{
    class Consultation
    {
        DateTime LogDate;
        string EmployeeName;
        string Username;

        public Consultation(DateTime dateTime, string employeeName, string username)
        {
            this.LogDate = dateTime;
            this.EmployeeName = employeeName;
            this.Username = username;
        }
    }
}
