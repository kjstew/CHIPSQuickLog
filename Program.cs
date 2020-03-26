using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Collections.Specialized;
using System.Configuration;
using System.Collections.Generic;

namespace CHIPSQuickLog
{
    class Program
    {
        const int MAX_COUNT = 9;
        bool canAdd = false;
        bool canRemove = false;
        const ConsoleKey KEY_EMP_ADD = ConsoleKey.A;
        const ConsoleKey KEY_EMP_REMOVE = ConsoleKey.R;

        public static List<string> employees;

        static void Main(string[] args)
        {
            employees = LoadEmployees();
            ConsoleKey userInput;
            do
            {
                userInput = MainMenu();
                if (GetCommands().Contains(userInput))
                {
                    switch (userInput)
                    {
                        case ConsoleKey.A:
                            AddEmployee();
                            break;
                        default:
                            Console.WriteLine("Not a valid command");
                            break;
                    }
                }
            } while (userInput != ConsoleKey.Escape);



            //// idenfify state to present
            //if (employees.Count == 0)
            //{
            //    // display message "no employees found" and only command to add employee
            //    prompt.Append("No employees found.");
            //}
            //else if (employees.Count >= MAX_COUNT)
            //{
            //    if (employees.Count > MAX_COUNT)
            //    {
            //        // display message "too many employees found; displaying only the first MAX_COUNT"
            //        prompt.Append($"More than {MAX_COUNT} employees were found and have been ignored.");

            //    }
            //    // display only remove
            //}

            //Console.WriteLine();
            //string options = "";
            //for (int i = 0; i < employees.Count; i++)
            //{
            //    options += $"{i + 1} : {employees[i]}\n";
            //}

            //string instructions =
            //    "CHIPS Quick Log\n\n" +
            //    "Helped a client who didn't check in a device?\n" +
            //    "Select your number to log the current date and time:\n" +
            //    options +
            //    "\nPress Esc to Cancel\n";

            //Console.SetWindowSize(80, 25);

            //Console.Write(instructions);
            //ConsoleKeyInfo pressedKey = Console.ReadKey();


            //SubmitLocalLog("Kyle");

            //Console.WriteLine("Press any key to close...");
            //Console.ReadKey();
        }

        static List<ConsoleKey> GetCommands()
        {
            List<ConsoleKey> commands = new List<ConsoleKey>();
            if (employees.Count < MAX_COUNT) commands.Add(KEY_EMP_ADD);
            if (employees.Count > 0) commands.Add(KEY_EMP_REMOVE);
            for (int i = 1; i <= Math.Min(employees.Count, MAX_COUNT); i++)
            {
                //add ConsoleKey D1-D9 to list
                commands.Add((ConsoleKey)Enum.Parse(typeof(ConsoleKey), "D" + i.ToString()));
            }
            return commands;
        }

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
                    prompt.Append($"[{i + 1}] {employees[i]}\n");
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
            Console.WriteLine("Enter new employee name and press [Enter]. \n" +
                "Press [Esc] to cancel.\n");

            StringBuilder newEmployee = new StringBuilder();

            // get first input
            ConsoleKeyInfo userInput = Console.ReadKey(true);

            // loop so long as key is not Enter or Escape
            while (userInput.Key != ConsoleKey.Enter && userInput.Key != ConsoleKey.Escape)
            {
                // if backspace, delete char and move indicator back
                if (userInput.Key == ConsoleKey.Backspace && newEmployee.Length > 0)
                {
                    Console.Write("\b \b");
                    newEmployee.Remove(newEmployee.Length - 1, 1);
                }
                // otherwise append char to name and write char to console
                else
                {
                    Console.Write(userInput.KeyChar);
                    newEmployee.Append(userInput.KeyChar);
                }
                // accept next input
                userInput = Console.ReadKey(true);
            }

            // if key to escape loop was enter, add to list of employees and save
            if (userInput.Key == ConsoleKey.Enter)
            {
                employees.Add(newEmployee.ToString());
                SaveEmployees(employees);
            }
        }

        static void RemoveEmployee()
        {
            Console.Clear();
            Console.WriteLine("Select the number of the employee to delete. \n");

            // list employees
            if (employees.Count > 0)
            {
                for (int i = 0; i < Math.Min(employees.Count, MAX_COUNT); i++)
                {
                    Console.WriteLine($"[{i + 1}] {employees[i]}\n");
                }
                Console.WriteLine("\n");
            }
            else
            {
                Console.WriteLine("No employees to remove. (If you can see this, Kyle did a bad job!)");
            }


        }

        static void SubmitLog(string empName)
        {

        }

        static void SubmitLocalLog(string empName)
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
                output.Append($"{logDateTime},{empName}\n");

                // write to file
                logSw.Write(output);

                // show confirmation
                Console.WriteLine($"Logged client consultation with {empName} @ {logDateTime}");
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

        static List<string> LoadEmployees()
        {   
            FileStream empFs = null;
            StreamReader empSr = null;
            List<string> employees = new List<string>();
            try
            {
                // init emp IO
                string empPath = ConfigurationManager.AppSettings.Get("EmpPath");
                empFs = new FileStream(empPath, FileMode.OpenOrCreate, FileAccess.Read);
                empSr = new StreamReader(empFs);

                string jsonEmp = empSr.ReadToEnd();
                if (jsonEmp == "") jsonEmp = "[]";

                employees = JsonSerializer.Deserialize<List<string>>(jsonEmp);
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

        static void SaveEmployees(List<string> employees)
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
