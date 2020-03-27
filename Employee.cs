using System;
using System.Collections.Generic;
using System.Text;

namespace CHIPSQuickLog
{
    class Employee
    {
        public string EmployeeName;
        public string Username;

        public Employee(string EmployeeName, string Username)
        {
            this.EmployeeName = EmployeeName;
            this.Username = Username;
        }

        // a parameterless constructor is required to deserialize Employees from json (current limitation of serializer, could make custom solution)
        // https://github.com/dotnet/runtime/issues/30854
        public Employee()
        {
        }
    }
}
