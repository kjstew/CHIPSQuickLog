using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Collections.Specialized;
using System.Configuration;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace CHIPSQuickLog
{
    class Program
    {
        const int MAX_COUNT = 9;
        bool canAdd = false;
        bool canRemove = false;
        const ConsoleKey KEY_EMP_ADD = ConsoleKey.A;
        const ConsoleKey KEY_EMP_REMOVE = ConsoleKey.R;
        static List<Command> commandList;

        public static List<Employee> employees;
        static HttpClient client = new HttpClient();

        static void Main(string[] args)
        {
            // api connection settings
            client.BaseAddress = new Uri("http://chipsmgr.com/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));


            // load employees list
            employees = LoadEmployees();
            ConsoleKey userInput;
            do
            {
                // key returned by main menu is checked
                userInput = MainMenu();
                if (GetCommands().Contains(userInput))
                {
                    // check add/removes
                    if (userInput == KEY_EMP_ADD)
                    {
                        AddEmployee();
                        continue;
                    }
                    if (userInput == KEY_EMP_REMOVE)
                    {
                        RemoveEmployee();
                        continue;
                    }

                    // check through available number keys
                    for (int i = 1; i <= employees.Count; i++)
                    {
                        // if keycode matches a position in employees list, submit with name
                        if (userInput.Equals(Enum.Parse(typeof(ConsoleKey), "D" + i.ToString())))
                        {
                            SubmitLocalLog(employees[i-1]);
                            continue;
                        }
                    }

                    // if no statements above run, 
                    Console.WriteLine("Not a valid command");
                }
            } while (userInput != ConsoleKey.Escape);
        }

        // Returns list of available commands based on number of employees
        static List<ConsoleKey> GetCommands()
        {
            List<ConsoleKey> commands = new List<ConsoleKey>();
            commands.Add(ConsoleKey.Escape);
            commands.Add(ConsoleKey.Enter);
            if (employees.Count < MAX_COUNT) commands.Add(KEY_EMP_ADD);
            if (employees.Count > 0) commands.Add(KEY_EMP_REMOVE);
            for (int i = 1; i <= Math.Min(employees.Count, MAX_COUNT); i++)
            {
                //add ConsoleKey D1-D9 to list
                commands.Add((ConsoleKey)Enum.Parse(typeof(ConsoleKey), "D" + i.ToString()));
            }
            return commands;
        }

        // Returns the key pressed on the main menu, used in Main to determine method to run
        static ConsoleKey MainMenu()
        {
            Console.Clear();
            StringBuilder prompt = new StringBuilder();
            List<ConsoleKey> commands = GetCommands();

            prompt.Append(
                "┌──────────────┐\n" +
                "│CHIPS QuickLog│\n" +
                "└──────────────┘\n\n");

            prompt.Append($"{employees.Count} employees found.\n\n");
            if (employees.Count > MAX_COUNT)
            {
                prompt.Append($"{employees.Count - MAX_COUNT} employee(s) will not be displayed. \n" +
                    $"Manually adding employees to {ConfigurationManager.AppSettings.Get("EmpPath")} is not advised. \n" +
                    $"Please remove employees to clean up the list.\n\n");
            }

            // list employees
            if (employees.Count > 0)
            {
                prompt.Append("Select your name:\n\n");
                for (int i = 0; i < Math.Min(employees.Count, MAX_COUNT); i++)
                {
                    prompt.Append($"[{i + 1}] {employees[i].EmployeeName}\n");
                }
                prompt.Append("\n");
            }

            // list additional commands
            if (commands.Contains(KEY_EMP_ADD))
                prompt.Append($"Press [{KEY_EMP_ADD}] to add an employee.\n");
            if (commands.Contains(KEY_EMP_REMOVE))
                prompt.Append($"Press [{KEY_EMP_REMOVE}] to remove an employee.\n");
            prompt.Append($"Press [Esc] to quit.\n");

            Console.WriteLine(prompt);

            // return key pressed
            var result = Console.ReadKey(true);
            return result.Key;
        }

        static void AddEmployee()
        {
            Console.Clear();
            Console.WriteLine("Add Employee \n" +
                "Enter new employee name and press [Enter]. \n" +
                "Press [Esc] to cancel.\n");

            StringBuilder newEmpName = new StringBuilder();
            StringBuilder newUserName = new StringBuilder();

            // get first input
            ConsoleKeyInfo userInput = Console.ReadKey(true);

            // loop so long as key is not Enter or Escape
            while (userInput.Key != ConsoleKey.Enter && userInput.Key != ConsoleKey.Escape)
            {
                // if backspace, delete char and move indicator back
                if (userInput.Key == ConsoleKey.Backspace && newEmpName.Length > 0)
                {
                    Console.Write("\b \b");
                    newEmpName.Remove(newEmpName.Length - 1, 1);
                }
                // otherwise append char to name and write char to console
                else
                {
                    Console.Write(userInput.KeyChar);
                    newEmpName.Append(userInput.KeyChar);
                }
                // accept next input
                userInput = Console.ReadKey(true);
            }

            // if key to escape loop was enter, now accept input for username
            if (userInput.Key == ConsoleKey.Enter)
            {
                Console.WriteLine("\nPlease enter your CHIPSMgr username: \n");

                // accept next input
                userInput = Console.ReadKey(true);

                while (userInput.Key != ConsoleKey.Enter && userInput.Key != ConsoleKey.Escape)
                {
                    // if backspace, delete char and move indicator back
                    if (userInput.Key == ConsoleKey.Backspace && newUserName.Length > 0)
                    {
                        Console.Write("\b \b");
                        newUserName.Remove(newUserName.Length - 1, 1);
                    }
                    // otherwise append char to name and write char to console
                    else
                    {
                        Console.Write(userInput.KeyChar);
                        newUserName.Append(userInput.KeyChar);
                    }
                    // accept next input
                    userInput = Console.ReadKey(true);
                }

                // if key to escape loop was enter, save new employee
                if (userInput.Key == ConsoleKey.Enter)
                {
                    employees.Add(new Employee(newEmpName.ToString(), newUserName.ToString()));
                    SaveEmployees(employees);
                }
            }
        }

        static void RemoveEmployee()
        {
            // TODO implement Esc to return


            Console.Clear();
            Console.WriteLine("Remove Employee \n");

            // list employees
            if (employees.Count > 0)
            {
                Console.WriteLine("Select the number of the employee to delete. \n" +
                    "Press [Esc] to cancel. \n");
                for (int i = 0; i < Math.Min(employees.Count, MAX_COUNT); i++)
                {
                    Console.WriteLine($"[{i + 1}] {employees[i].EmployeeName}");
                }
            }
            else
            {
                Console.WriteLine("No employees to remove. Press [Esc] to return. (If you can see this, Kyle did a bad job!)");
            }

            // get first input
            ConsoleKeyInfo userInput = Console.ReadKey(true);

            // loop so long as key is not Escape
            while (userInput.Key != ConsoleKey.Escape)
            {
            }


        }

        static async Task<Consultation> SubmitConsultation(Employee emp)
        {
            // collect information to post in consultation object
            Consultation con = new Consultation(DateTime.Now, emp.EmployeeName, emp.Username);
            // TODO unsure of URI
            HttpResponseMessage response = await client.PutAsJsonAsync($"api/consultations/create", con);
            response.EnsureSuccessStatusCode();
            // Deserialize the updated product from the response body.
            // TODO how is response differerent when creating vs updating? Is returning necessary?
            con = await response.Content.ReadAsAsync<Consultation>();
            return con;
        }

        static void SubmitLocalLog(Employee employee)
        {
            FileStream logFs = null;
            StreamWriter logSw = null;
            StringBuilder output;

            try
            {
                string logPath = ConfigurationManager.AppSettings.Get("LogPath");
                logFs = new FileStream(logPath, FileMode.Append, FileAccess.Write);
                logSw = new StreamWriter(logFs);

                output = new StringBuilder();

                // initialize csv if log is empty
                if (logFs.Length == 0)
                {
                    output.Append("datetime,employee\n");
                }

                string logDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // append datetime and employee name
                output.Append($"{logDateTime},{employee.EmployeeName}\n");

                // write to file
                logSw.Write(output);

                // show confirmation
                Console.WriteLine($"Logged client consultation with {employee.EmployeeName} @ {logDateTime}");
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                if (logSw != null) logSw.Close();
                if (logFs != null) logFs.Close();
            }
        }

        static List<Employee> LoadEmployees()
        {
            FileStream empFs = null;
            StreamReader empSr = null;
            List<Employee> employees = new List<Employee>();
            try
            {
                // init emp IO
                string empPath = ConfigurationManager.AppSettings.Get("EmpPath");
                empFs = new FileStream(empPath, FileMode.OpenOrCreate, FileAccess.Read);
                empSr = new StreamReader(empFs);

                string jsonEmp = empSr.ReadToEnd();
                if (jsonEmp == "") jsonEmp = "[]";

                employees = JsonSerializer.Deserialize<List<Employee>>(jsonEmp);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                if (empSr != null) empSr.Close();
                if (empFs != null) empFs.Close();
            }
            return employees;
        }

        static void SaveEmployees(List<Employee> employees)
        {
            FileStream empFs = null;
            StreamWriter empSw = null;
            try
            {
                // init emp IO
                string empPath = ConfigurationManager.AppSettings.Get("EmpPath");
                empFs = new FileStream(empPath, FileMode.Create, FileAccess.Write);
                empSw = new StreamWriter(empFs);

                string jsonEmp = JsonSerializer.Serialize(employees);

                empSw.Write(jsonEmp);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                if (empSw != null) empSw.Close();
                if (empFs != null) empFs.Close();
            }
        }
    }
}
