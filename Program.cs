using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Collections.Specialized;
using System.Configuration;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Threading;

namespace CHIPSQuickLog
{
    class Program
    {
        const int MAX_COUNT = 9; // maximum number of employees that can be added (corresponds to keys 1-9)
        const ConsoleKey KEY_EMP_ADD = ConsoleKey.A;
        const ConsoleKey KEY_EMP_REMOVE = ConsoleKey.R;
        const string CONSOLE_HEADER = "   ____        _      __   __\n" +
                                      "   / __ \\__  __(_)____/ /__/ /  ____  ____ _\n" +
                                      "  / / / / / / / / ___/ //_/ /  / __ \\/ __ `/\n" +
                                      " / /_/ / /_/ / / /__/ ,< / /__/ /_/ / /_/ /\n" +
                                      " \\___\\_\\__,_/_/\\___/_/|_/_____\\____/\\__, /\n" +
                                      "                                   /____/\n";

        static List<Employee> employees;
        static ConsoleKeyInfo userInput;
        static HttpClient client = new HttpClient();
        static string apiKey = "";
        static void Main(string[] args)
        {
            // get api key
            FileStream apiFs = null;
            StreamReader apiSr = null;
            
            try
            {
                apiFs = new FileStream(ConfigurationManager.AppSettings.Get("ApiPath"), FileMode.OpenOrCreate, FileAccess.Read);
                apiSr = new StreamReader(apiFs);
                apiKey = apiSr.ReadToEnd();
            }
            catch (IOException ioe)
            {
                WriteConsole("API I/O Error: " + ioe.Message);
            }
            finally
            {
                if (apiSr != null) apiSr.Close();
                if (apiFs != null) apiFs.Close();
            }

            // set api connection
            client.BaseAddress = new Uri("http://chipsmgr.com/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("ApiKey", apiKey); // TODO change to random alphanumeric

            // set console window size
            Console.SetWindowSize(100, 30);
            Console.SetBufferSize(100, 30);

            // load employees
            employees = LoadEmployees();

            do
            {
                MainMenu();
            } while (userInput.Key != ConsoleKey.Escape);
        }

        static void MainMenu()
        {
            // create prompt
            Console.Title = "CHIPS QuickLog";
            Console.Clear();
            StringBuilder prompt = new StringBuilder();
            // notify user if api is empty (first run)
            if (apiKey == "")
            {
                WriteConsole("The file holding the API key (" + ConfigurationManager.AppSettings.Get("ApiPath") + ") is empty or was just created." );
                WriteConsole("Add the Chips Service Manager API key to this file and try running again.");
            }
            WriteConsole(CONSOLE_HEADER);
            WriteConsole($"(Employees: {employees.Count})\n");

            if (employees.Count > MAX_COUNT)
            {
                WriteConsole($"{employees.Count - MAX_COUNT} employee(s) will not be displayed.");
                WriteConsole($"Manually adding employees to {ConfigurationManager.AppSettings.Get("EmpPath")} is not recommended.");
                WriteConsole($"Please remove employees to clean up the list.\n");
            }
            // list employees
            if (employees.Count > 0)
            {
                WriteConsole("Press the number next to your name to log a consultation:\n");
                for (int i = 0; i < Math.Min(employees.Count, MAX_COUNT); i++)
                {
                    WriteConsole($"[{i + 1}] {employees[i].EmployeeName}");
                }
                WriteConsole();
            }
            // list additional commands
            if (CanAddEmployee())
                WriteConsole($"Press [{KEY_EMP_ADD}] to add an employee.");
            if (CanRemoveEmployee())
                WriteConsole($"Press [{KEY_EMP_REMOVE}] to remove an employee.");
            WriteConsole($"Press [Esc] to quit.\n");

            // accept input
            do
            {
                // Wait for key and store
                userInput = Console.ReadKey(true);

                // check add/removes 
                if (CanAddEmployee() && userInput.Key == KEY_EMP_ADD)
                {
                    AddEmployee();
                    // empty userInput to avoid Esc cascading and quitting app
                    userInput = new ConsoleKeyInfo();
                    break;
                }
                if (CanRemoveEmployee() && userInput.Key == KEY_EMP_REMOVE)
                {
                    RemoveEmployee();
                    // empty userInput to avoid Esc cascading and quitting app
                    userInput = new ConsoleKeyInfo();
                    break;
                }

                // check through available number keys
                for (int i = 1; i <= employees.Count; i++)
                {
                    // submit consultation log if keycode matches
                    if (userInput.Key.Equals(Enum.Parse(typeof(ConsoleKey), "D" + i.ToString())))
                    {
                        // submit consultation and get copy of consultation retrieved from api reponse
                        Consultation consultation = SubmitConsultation(employees[i - 1]).Result;
                        WriteConsole("Press any key to exit or wait five seconds.");
                        int waitCounter = 0;
                        while (!Console.KeyAvailable && waitCounter < 10) // counter increments every half second
                        {
                            Thread.Sleep(500);
                            waitCounter++;
                        }
                        Environment.Exit(0);
                    }
                }
            } while (userInput.Key != ConsoleKey.Escape);
        }

        static void AddEmployee()
        {
            Console.Clear();
            WriteConsole(CONSOLE_HEADER);
            WriteConsole("> Add Employee\n");
            WriteConsole("Enter new employee name and press [Enter].");
            WriteConsole("Press [Esc] to cancel.\n");

            StringBuilder newEmpName = new StringBuilder();
            StringBuilder newUserName = new StringBuilder();

            // loop until key is Enter or Esc
            do
            {
                // accept input
                userInput = Console.ReadKey(true);

                // Esc, return
                if (userInput.Key == ConsoleKey.Escape)
                    return;

                // if backspace, delete char and move indicator back
                if (userInput.Key == ConsoleKey.Backspace && newEmpName.Length > 0)
                {
                    Console.Write("\b \b");
                    newEmpName.Remove(newEmpName.Length - 1, 1);
                }
                // otherwise (if not Enter) append char to name and write char to console
                else if (userInput.Key != ConsoleKey.Enter)
                {
                    Console.Write(userInput.KeyChar);
                    newEmpName.Append(userInput.KeyChar);
                }
            } while (userInput.Key != ConsoleKey.Enter || newEmpName.Length == 0);

            // accept input for username
            WriteConsole("\n");
            WriteConsole("Please enter your CHIPSMgr username:\n");
            do
            {
                // accept input
                userInput = Console.ReadKey(true);

                // user cancels, return
                if (userInput.Key == ConsoleKey.Escape)
                    return;

                // if backspace, delete char and move indicator back
                if (userInput.Key == ConsoleKey.Backspace && newUserName.Length > 0)
                {
                    Console.Write("\b \b");
                    newUserName.Remove(newUserName.Length - 1, 1);
                }
                // otherwise (if not Enter) append char to username and write char to console
                else if (userInput.Key != ConsoleKey.Enter)
                {
                    Console.Write(userInput.KeyChar);
                    newUserName.Append(userInput.KeyChar);
                }
            } while (userInput.Key != ConsoleKey.Enter || newUserName.Length == 0);

            // new employee data entered, add employee to list and save
            if (userInput.Key == ConsoleKey.Enter)
            {
                employees.Add(new Employee(newEmpName.ToString(), newUserName.ToString()));
                SaveEmployees(employees);
            }

        }

        static void RemoveEmployee()
        {
            Console.Clear();
            WriteConsole(CONSOLE_HEADER);
            WriteConsole("> Remove Employee \n");

            // list employees
            if (employees.Count > 0)
            {
                WriteConsole("Select the number of the employee to delete.");
                WriteConsole("Press [Esc] to cancel. \n");
                for (int i = 0; i < Math.Min(employees.Count, MAX_COUNT); i++)
                {
                    WriteConsole($"[{i + 1}] {employees[i].EmployeeName}");
                }
                WriteConsole();
            }
            else
            {
                WriteConsole("No employees to remove. Press [Esc] to return. (If you can see this, Kyle did a bad job!)");
            }

            // loop unless key is Esc or a number key
            while (true)
            {
                // accept input
                userInput = Console.ReadKey(true);

                // user cancels, return
                if (userInput.Key == ConsoleKey.Escape)
                    return;

                // check through available number keys
                for (int i = 1; i <= employees.Count; i++)
                {
                    // if keycode matches a position in employees list, delete corresponding employee
                    if (userInput.Key.Equals(Enum.Parse(typeof(ConsoleKey), "D" + i.ToString())))
                    {
                        // provide confirmation
                        WriteConsole("Are you sure you want to delete employee " + employees[i - 1].EmployeeName + "? This cannot be undone.");
                        WriteConsole("Press [Enter] to confirm, [Esc] to cancel.\n");
                        while (true)
                        {
                            userInput = Console.ReadKey(true);
                            if (userInput.Key == ConsoleKey.Escape)
                                return;
                            if (userInput.Key == ConsoleKey.Enter)
                            {
                                employees.RemoveAt(i - 1);
                                SaveEmployees(employees);
                                return;
                            }
                        }
                    }
                }
            }
        }

        static async Task<Consultation> SubmitConsultation(Employee emp)
        {
            // save consultation locally to csv
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
                    output.Append("datetime,employeename,username\n");
                }

                string logDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // append datetime and employee name
                output.Append($"{logDateTime},{emp.EmployeeName},{emp.Username}\n");
                // write to file
                logSw.Write(output);
                // show confirmation
                WriteConsole($"Consultation logged locally.");
            }
            catch (IOException ioe)
            {
                WriteConsole("ERROR: Consultation failed to log locally.");
                WriteConsole(ioe.Message);
            }
            finally
            {
                if (logSw != null) logSw.Close();
                if (logFs != null) logFs.Close();
            }

            WriteConsole($"Connecting to database...");

            // upload consultation to db using api
            Consultation con = new Consultation(DateTime.Now, emp.Username);
            try
            {
                HttpResponseMessage response = await client.PostAsJsonAsync($"api/consultation", con);
                response.EnsureSuccessStatusCode();
                // return consultation from response
                con = await response.Content.ReadAsAsync<Consultation>();
                WriteConsole($"Consultation logged to database ({con.UserName} at {con.Time})");
            }
            catch (HttpRequestException)
            {
                WriteConsole("ERROR: Could not connect to database. Please check connection.");
            }
            
            return con;
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

                employees = JsonConvert.DeserializeObject<List<Employee>>(jsonEmp);
            }
            catch (IOException ioe)
            {
                WriteConsole(ioe.Message);
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

                string jsonEmp = JsonConvert.SerializeObject(employees);

                empSw.Write(jsonEmp);
            }
            catch (IOException ioe)
            {
                WriteConsole(ioe.Message);
            }
            finally
            {
                if (empSw != null) empSw.Close();
                if (empFs != null) empFs.Close();
            }
        }

        // utility methods
        static bool CanAddEmployee()
        {
            if (employees.Count < MAX_COUNT) return true;
            return false;
        }
        static bool CanRemoveEmployee()
        {
            if (employees.Count > 0) return true;
            return false;
        }
        static void WriteConsole() { Console.WriteLine(); }
        // currently only adds a space before writing to console for left padding
        static void WriteConsole(string str) { Console.WriteLine(" " + str); }
    }
}
