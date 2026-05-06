{
    // ============================================================
    // 1. INTERFACE - Defines basic operations for any employee
    // ============================================================
    public interface IEmployeeActions
    {
        double CalculateSalary();
        void DisplayInfo();
        void Work();
    }

    // ============================================================
    // 2. ABSTRACT CLASS - Implements the interface
    // ============================================================
    public abstract class Employee : IEmployeeActions
    {
        // Encapsulation: all fields are private
        private string _name;
        private int _age;
        private double _baseSalary;

        // Static properties to track all employees
        public static int Count { get; private set; } = 0;
        public static double TotalSalaryBudget { get; private set; } = 0;

        // Delegate and Event for high salary alert
        public delegate void SalaryAlertHandler(string employeeName, double salary);
        public event SalaryAlertHandler OnHighSalary;

        // Read-only property - cannot be changed after creation
        public int Id { get; private set; }

        public string Name
        {
            get { return _name; }
            private set
            {
                if (!DataValidator.IsValidName(value))
                    throw new ArgumentException("Invalid name!");
                _name = value;
            }
        }

        public int Age
        {
            get { return _age; }
            private set
            {
                if (!DataValidator.IsValidAge(value))
                    throw new ArgumentException("Age must be between 18 and 65!");
                _age = value;
            }
        }

        public double BaseSalary
        {
            get { return _baseSalary; }
            protected set
            {
                if (!DataValidator.IsValidSalary(value))
                    throw new ArgumentException("Salary must be greater than zero!");
                _baseSalary = value;
            }
        }

        // Constructor
        protected Employee(int id, string name, int age, double baseSalary)
        {
            Id = id;
            Name = name;
            Age = age;
            BaseSalary = baseSalary;
            Count++;
        }

        // Abstract methods - must be implemented in subclasses
        public abstract double CalculateSalary();
        public abstract void Work();

        // Shared method for all employees
        public void DisplayInfo()
        {
            Console.WriteLine($"  ID         : {Id}");
            Console.WriteLine($"  Name       : {Name}");
            Console.WriteLine($"  Age        : {Age}");
            Console.WriteLine($"  Salary     : {CalculateSalary():F2} $");
        }

        //  CheckSalary now only fires the event, does NOT add to TotalSalaryBudget
        // TotalSalaryBudget is calculated once at the end via CalculateTotalBudget()
        public void CheckSalary()
        {
            double salary = CalculateSalary();

            if (salary > 10000)
            {
                OnHighSalary?.Invoke(Name, salary);
            }
        }

        // New static method to calculate total budget once (avoids accumulation bug)
        public static void CalculateTotalBudget(List<Employee> employees)
        {
            TotalSalaryBudget = 0;
            foreach (var emp in employees)
            {
                TotalSalaryBudget += emp.CalculateSalary();
            }
        }
    }

// ============================================================
    // 3. INHERITANCE & POLYMORPHISM - Subclasses
    // ============================================================

    // Manager class
    public class Manager : Employee
    {
        private double _bonus;

        public double Bonus
        {
            get { return _bonus; }
            private set
            {
                if (value < 0) throw new ArgumentException("Bonus cannot be negative!");
                _bonus = value;
            }
        }

        public Manager(int id, string name, int age, double baseSalary, double bonus)
            : base(id, name, age, baseSalary)
        {
            Bonus = bonus;
        }

        public override double CalculateSalary()
        {
            return BaseSalary + Bonus;
        }

        public override void Work()
        {
            Console.WriteLine($"  {Name} is managing the team and making strategic decisions.");
        }
    }

    // Developer class
    public class Developer : Employee
    {
        private int _hoursWorked;
        private double _hourlyRate;

        public int HoursWorked
        {
            get { return _hoursWorked; }
            private set
            {
                // FIX: Now actually uses IsValidHours() from DataValidator (was dead code before)
                if (!DataValidator.IsValidHours(value))
                    throw new ArgumentException("Hours must be between 1 and 744!");
                _hoursWorked = value;
            }
        }

        public double HourlyRate
        {
            get { return _hourlyRate; }
            private set
            {
                if (value <= 0) throw new ArgumentException("Hourly rate must be greater than zero!");
                _hourlyRate = value;
            }
        }

        // Removed the redundant hoursWorked * hourlyRate from base() call.
        // BaseSalary is set to hourlyRate only as a base reference,
        // and the actual salary is always calculated via CalculateSalary().
        public Developer(int id, string name, int age, int hoursWorked, double hourlyRate)
            : base(id, name, age, hourlyRate)
        {
            HoursWorked = hoursWorked;
            HourlyRate = hourlyRate;
        }

        public override double CalculateSalary()
        {
            return HoursWorked * HourlyRate;
        }

        public override void Work()
        {
            Console.WriteLine($"  {Name} is writing code and developing software.");
        }
    }

    // Intern class
    public class Intern : Employee
    {
        // Read-only property - set internally
        public string Department { get; private set; }

        public Intern(int id, string name, int age, double stipend, string department)
            : base(id, name, age, stipend)
        {
            Department = department;
        }

        public override double CalculateSalary()
        {
            return BaseSalary;
        }

        public override void Work()
        {
            Console.WriteLine($"  {Name} is learning and assisting in the {Department} department.");
        }
    }
// ============================================================
    // 4. Department class - shows Polymorphism with List
    // ============================================================
    public class Department
    {
        private List<Employee> _employees;
        public string DepartmentName { get; private set; }

        public Department(string name)
        {
            DepartmentName = name;
            _employees = new List<Employee>();
        }

        public void AddEmployee(Employee emp)
        {
            _employees.Add(emp);
        }

        // Returns the internal list for external use (e.g., budget calculation)
        public List<Employee> GetEmployees()
        {
            return _employees;
        }

        // Polymorphism: same method behaves differently for each employee
        public void ShowAllEmployees()
        {
            Console.WriteLine($"\n========== Department: {DepartmentName} ==========");
            foreach (var emp in _employees)
            {
                emp.Work();
                emp.DisplayInfo();
                emp.CheckSalary(); // FIX: no longer adds to budget here
                Console.WriteLine("  ----------------------------");
            }
        }
    }
// ============================================================
    // 5. STATIC CLASS - Data validation
    // ============================================================
    public static class DataValidator
    {
        public static bool IsValidName(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && name.Length >= 2;
        }

        public static bool IsValidAge(int age)
        {
            return age >= 18 && age <= 65;
        }

        public static bool IsValidSalary(double salary)
        {
            return salary > 0;
        }

        // IsValidHours is now used inside Developer.HoursWorked setter
        public static bool IsValidHours(int hours)
        {
            return hours > 0 && hours <= 744;
        }
    }

