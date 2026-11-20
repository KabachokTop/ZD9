using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EmployeeManager
{
    public class Employee
    {
        public string FullName { get; set; }
        public string Position { get; set; }
        public decimal Salary { get; set; }
        public string Email { get; set; }

        public Employee(string fullName, string position, decimal salary, string email)
        {
            FullName = fullName;
            Position = position;
            Salary = salary;
            Email = email;
        }

        public override string ToString()
        {
            return $"{FullName,-30} | {Position,-20} | {Salary,12:N2} | {Email}";
        }
    }

    public class EmployeeService
    {
        private readonly List<Employee> _employees = new List<Employee>();

        public IReadOnlyList<Employee> All => _employees;

        public void Add(Employee employee) => _employees.Add(employee);

        public bool Remove(int index)
        {
            if (index < 0 || index >= _employees.Count) return false;
            _employees.RemoveAt(index);
            return true;
        }

        public bool Update(int index, string fullName, string position, decimal? salary, string email)
        {
            if (index < 0 || index >= _employees.Count) return false;
            var e = _employees[index];
            if (!string.IsNullOrWhiteSpace(fullName)) e.FullName = fullName;
            if (!string.IsNullOrWhiteSpace(position)) e.Position = position;
            if (salary.HasValue) e.Salary = salary.Value;
            if (!string.IsNullOrWhiteSpace(email)) e.Email = email;
            return true;
        }

        public List<Employee> SearchByName(string text) =>
            _employees.Where(e => e.FullName.Contains(text, StringComparison.OrdinalIgnoreCase)).ToList();

        public List<Employee> SearchByPosition(string text) =>
            _employees.Where(e => e.Position.Contains(text, StringComparison.OrdinalIgnoreCase)).ToList();

        public List<Employee> SearchBySalary(decimal min, decimal max) =>
            _employees.Where(e => e.Salary >= min && e.Salary <= max).ToList();

        public List<Employee> SearchByEmail(string text) =>
            _employees.Where(e => e.Email.Contains(text, StringComparison.OrdinalIgnoreCase)).ToList();

        public void Sort(string field, bool descending)
        {
            Comparison<Employee> cmp = field switch
            {
                "name" => (a, b) => string.Compare(a.FullName, b.FullName, StringComparison.CurrentCultureIgnoreCase),
                "position" => (a, b) => string.Compare(a.Position, b.Position, StringComparison.CurrentCultureIgnoreCase),
                "salary" => (a, b) => a.Salary.CompareTo(b.Salary),
                "email" => (a, b) => string.Compare(a.Email, b.Email, StringComparison.OrdinalIgnoreCase),
                _ => (a, b) => 0
            };
            _employees.Sort(cmp);
            if (descending) _employees.Reverse();
        }
    }

    public static class Program
    {
        private static readonly EmployeeService Service = new EmployeeService();

        public static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            Service.Add(new Employee("Іваненко Іван Іванович", "Розробник", 45000, "ivanenko@company.com"));
            Service.Add(new Employee("Петренко Олена Сергіївна", "Тестувальник", 32000, "petrenko@company.com"));
            Service.Add(new Employee("Коваль Андрій Петрович", "Менеджер", 38500.50m, "koval@company.com"));

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("===== Облік співробітників =====");
                Console.WriteLine("1. Показати всіх");
                Console.WriteLine("2. Додати співробітника");
                Console.WriteLine("3. Видалити співробітника");
                Console.WriteLine("4. Змінити інформацію");
                Console.WriteLine("5. Пошук");
                Console.WriteLine("6. Сортування");
                Console.WriteLine("0. Вихід");
                Console.Write("Ваш вибір: ");

                switch (Console.ReadLine()?.Trim())
                {
                    case "1": PrintList(Service.All); break;
                    case "2": AddEmployee(); break;
                    case "3": RemoveEmployee(); break;
                    case "4": EditEmployee(); break;
                    case "5": SearchMenu(); break;
                    case "6": SortMenu(); break;
                    case "0": return;
                    default: Console.WriteLine("Невірний пункт меню."); break;
                }
            }
        }

        private static void PrintList(IReadOnlyList<Employee> list)
        {
            if (list.Count == 0)
            {
                Console.WriteLine("Список порожній.");
                return;
            }
            Console.WriteLine($"{"№",-3} | {"ПІБ",-30} | {"Посада",-20} | {"Зарплата",12} | Email");
            Console.WriteLine(new string('-', 95));
            for (int i = 0; i < list.Count; i++)
                Console.WriteLine($"{i + 1,-3} | {list[i]}");
        }

        private static void AddEmployee()
        {
            string name = ReadRequired("ПІБ: ");
            string position = ReadRequired("Посада: ");
            decimal salary = ReadDecimal("Заробітна плата: ");
            string email = ReadEmail("Робочий email: ");

            Service.Add(new Employee(name, position, salary, email));
            Console.WriteLine("Співробітника додано.");
        }

        private static void RemoveEmployee()
        {
            if (Service.All.Count == 0) { Console.WriteLine("Список порожній."); return; }
            PrintList(Service.All);
            int number = ReadInt("Введіть номер співробітника для видалення: ");
            Console.WriteLine(Service.Remove(number - 1) ? "Співробітника видалено." : "Невірний номер.");
        }

        private static void EditEmployee()
        {
            if (Service.All.Count == 0) { Console.WriteLine("Список порожній."); return; }
            PrintList(Service.All);
            int number = ReadInt("Введіть номер співробітника для зміни: ");
            if (number < 1 || number > Service.All.Count) { Console.WriteLine("Невірний номер."); return; }

            Console.WriteLine("Залиште поле порожнім, щоб не змінювати його.");

            Console.Write("Новий ПІБ: ");
            string name = Console.ReadLine();

            Console.Write("Нова посада: ");
            string position = Console.ReadLine();

            decimal? salary = null;
            while (true)
            {
                Console.Write("Нова зарплата: ");
                string s = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(s)) break;
                if (TryParseDecimal(s, out decimal val) && val >= 0) { salary = val; break; }
                Console.WriteLine("Некоректне число.");
            }

            string email = null;
            while (true)
            {
                Console.Write("Новий email: ");
                string s = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(s)) break;
                if (IsValidEmail(s)) { email = s.Trim(); break; }
                Console.WriteLine("Некоректний email.");
            }

            Service.Update(number - 1, name, position, salary, email);
            Console.WriteLine("Інформацію оновлено.");
        }

        private static void SearchMenu()
        {
            Console.WriteLine("Пошук за: 1 - ПІБ, 2 - Посада, 3 - Зарплата (діапазон), 4 - Email");
            Console.Write("Ваш вибір: ");
            List<Employee> result;

            switch (Console.ReadLine()?.Trim())
            {
                case "1": result = Service.SearchByName(ReadRequired("Введіть ПІБ (або частину): ")); break;
                case "2": result = Service.SearchByPosition(ReadRequired("Введіть посаду (або частину): ")); break;
                case "3":
                    decimal min = ReadDecimal("Мінімальна зарплата: ");
                    decimal max = ReadDecimal("Максимальна зарплата: ");
                    result = Service.SearchBySalary(min, max);
                    break;
                case "4": result = Service.SearchByEmail(ReadRequired("Введіть email (або частину): ")); break;
                default: Console.WriteLine("Невірний вибір."); return;
            }

            Console.WriteLine($"Знайдено: {result.Count}");
            PrintList(result);
        }

        private static void SortMenu()
        {
            Console.WriteLine("Сортувати за: 1 - ПІБ, 2 - Посада, 3 - Зарплата, 4 - Email");
            Console.Write("Ваш вибір: ");
            string field = Console.ReadLine()?.Trim() switch
            {
                "1" => "name",
                "2" => "position",
                "3" => "salary",
                "4" => "email",
                _ => null
            };
            if (field == null) { Console.WriteLine("Невірний вибір."); return; }

            Console.Write("Порядок: 1 - за зростанням, 2 - за спаданням: ");
            bool desc = Console.ReadLine()?.Trim() == "2";

            Service.Sort(field, desc);
            Console.WriteLine("Список відсортовано.");
            PrintList(Service.All);
        }

        private static string ReadRequired(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string s = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(s)) return s.Trim();
                Console.WriteLine("Поле не може бути порожнім.");
            }
        }

        private static int ReadInt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out int v)) return v;
                Console.WriteLine("Введіть ціле число.");
            }
        }

        private static decimal ReadDecimal(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (TryParseDecimal(Console.ReadLine(), out decimal v) && v >= 0) return v;
                Console.WriteLine("Введіть коректне невід'ємне число.");
            }
        }

        private static bool TryParseDecimal(string input, out decimal value)
        {
            input = (input ?? "").Replace(',', '.');
            return decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
        }

        private static string ReadEmail(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string s = Console.ReadLine();
                if (IsValidEmail(s)) return s.Trim();
                Console.WriteLine("Некоректний email (приклад: name@company.com).");
            }
        }

        private static bool IsValidEmail(string email) =>
            !string.IsNullOrWhiteSpace(email) &&
            Regex.IsMatch(email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }
}