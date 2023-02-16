using Gym_Booking_Manager.Activities;
using Gym_Booking_Manager.ManagementFunctions;
using Gym_Booking_Manager.Reservables;
using Gym_Booking_Manager.Reservations;
using Gym_Booking_Manager.Schedules;

namespace Gym_Booking_Manager.Users
{
    public abstract class User
    {
        // NYI: Add system admin rights!
        //private const string adminUsername = "admin";
        //private const string adminPassword = "admin123";

        public static int getUserID;
        public static List<User> users = new List<User>();

        public int id { get; set; } // TBD: USE GUID?
        public string? firstName { get; set; }
        public string? lastName { get; set; }
        public string? ssn { get; set; }
        public string? phone { get; set; }
        public string? email { get; set; }
        public string? loginName { get; set; }
        public string? loginPass { get; set; }
        public User(int id, string firstName, string lastName, string ssn, string phone, string email, string loginName, string loginPass)
        {
            this.id = id;
            this.firstName = firstName;
            this.lastName = lastName;
            this.ssn = ssn;
            this.phone = phone;
            this.email = email;
            this.loginName = loginName;
            this.loginPass = loginPass;
        }
        public User() { }
        public static void LoadUsers()
        {
            try
            {
                string[] lines = File.ReadAllLines("Users/Users.txt");
                getUserID = int.Parse(lines[0]);
                for (int i = 1; i < lines.Length; i++)
                {
                    string[] strings = lines[i].Split(";");
                    if (strings[0] == "Admin") users.Add(new Admin(int.Parse(strings[1]), strings[2], strings[3], (strings[4]), strings[5], strings[6], strings[7], strings[8]));
                    if (strings[0] == "Staff") users.Add(new Staff(int.Parse(strings[1]), strings[2], strings[3], (strings[4]), strings[5], strings[6], strings[7], strings[8]));
                    if (strings[0] == "Customer") users.Add(new Customer(int.Parse(strings[1]), strings[2], strings[3], (strings[4]), strings[5], strings[6], strings[7], strings[8], DateTime.Parse(strings[9]), DateTime.Parse(strings[10]), bool.Parse(strings[11])));
                }
                Program.logger.LogActivity("INFO: LoadUsers() - Read data (\"Users/Users.txt\") successful.");
            }
            catch { Program.logger.LogActivity("ERROR: LoadUsers() - Read data (\"Users/Users.txt\") unsuccessful."); }
        }
        public static void SaveUsers()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter("Users/Users.txt", false))
                {
                    writer.WriteLine(getUserID);
                    foreach (User user in users)
                    {
                        if (user is Admin)
                        {
                            writer.WriteLine($"Admin;{user.id};{user.firstName};{user.lastName};{user.ssn};{user.phone};{user.email};{user.loginName};{user.loginPass}");
                        }
                        if (user is Staff)
                        {
                            writer.WriteLine($"Staff;{user.id};{user.firstName};{user.lastName};{user.ssn};{user.phone};{user.email};{user.loginName};{user.loginPass}");
                        }
                        if (user is Customer)
                        {
                            Customer saveUser = (Customer)user;
                            writer.WriteLine($"Customer;{saveUser.id};{user.firstName};{saveUser.lastName};{saveUser.ssn};{saveUser.phone};{saveUser.email};{saveUser.loginName};{saveUser.loginPass};{saveUser.subStart};{saveUser.subEnd};{saveUser.isSub}");
                        }
                    }
                }
                Program.logger.LogActivity("INFO: SaveUsers() - Write data (\"Users/Users.txt\") successful.");
            }
            catch { Program.logger.LogActivity("ERROR: SaveUsers() - Write data (\"Users/Users.txt\") unsuccessful."); }
        }
        private static int GetUserID()
        {
            int id = getUserID;
            getUserID++;
            return id;
        }
        protected void RegisterUser() // NYI: ADD LOGGER!
        {
            User user = new Customer();
            Console.Clear();
            Console.WriteLine("<< NEW ACCOUNT REGISTRATION >>\n");

            if (this is Admin)
            {
                Console.WriteLine($">> Select account type: \n{"- [1]",-8}New admin account.\n{"- [2]",-8}New staff account.\n{"- [3]",-8}New customer account.");
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.D1 || keyInfo.Key == ConsoleKey.NumPad1) { user = new Admin(); }
                else if (keyInfo.Key == ConsoleKey.D2 || keyInfo.Key == ConsoleKey.NumPad2) { user = new Staff(); }
                else if (keyInfo.Key == ConsoleKey.D3 || keyInfo.Key == ConsoleKey.NumPad3) { user = new Customer(); }
                else
                {
                    Console.WriteLine($">> INVALID KEY: [{keyInfo.Key}]");
                    return;
                }
            }
            user.id = GetUserID();
            Console.Write(">> Enter first name: ");
            user.firstName = Console.ReadLine();
            Console.Write(">> Enter last name: ");
            user.lastName = Console.ReadLine();
            Console.Write(">> Enter social security number: ");
            user.ssn = Console.ReadLine();
            Console.Write(">> Enter phone number: ");
            user.phone = Console.ReadLine();
            Console.Write(">> Enter email address: ");
            user.email = Console.ReadLine();
            Console.Write(">> Enter login name: ");
            user.loginName = Console.ReadLine();
            while (!Login.CheckLoginName(user.loginName))
            {
                Console.Write(">> Login name unavailable, try again: ");
                user.loginName = Console.ReadLine();
            }
            string loginPassA, loginPassB;
            do
            {
                Console.Write(">> Enter login password: ");
                loginPassA = Login.MaskPassword();
                Console.Write("\n>> Confirm password: ");
                loginPassB = Login.MaskPassword();
                Console.WriteLine();
                if (loginPassA != loginPassB) Console.WriteLine(">> Confirm password failed!");
            } while (loginPassA != loginPassB);
            user.loginPass = loginPassA;

            if (user is Staff)
            {
                Console.WriteLine(">> Add new staff as a personal trainer?");
                Console.WriteLine($"{"- [Y]",-8}Yes.\n{"- [N]",-8}No.");

                ConsoleKeyInfo keyPressed = Console.ReadKey(true);

                if (keyPressed.Key == ConsoleKey.Y)
                {
                    Reservable.NewPT((Staff)user, false);
                    Console.WriteLine("\n>> Added new staff as a personal trainer.");
                }
                else if (keyPressed.Key == ConsoleKey.N) Console.WriteLine("\n>> New staff not added as a personal trainer.");
                else Console.WriteLine($"\n>> INVALID KEY: [{keyPressed.Key}], new staff not added as a personal trainer.");
            }
            if (user is Customer)
            {
                Customer customer = (Customer)user;
                Console.WriteLine("\n>> New customer account registered, add a subscription plan?");
                Console.WriteLine($"{"- [Y]",-8}Yes.\n{"- [N]",-8}No.");

                ConsoleKeyInfo keyPressed = Console.ReadKey(true);

                if (keyPressed.Key == ConsoleKey.Y) customer.AddSubscription();
                else if (keyPressed.Key == ConsoleKey.N) Console.WriteLine("\n>> No subscription plan added.");
                else Console.WriteLine($"\n>> INVALID KEY: [{keyPressed.Key}], no subscription plan added.");
            }

            Console.WriteLine($"\n>> New account ({user.loginName}) successfully created!");
            Console.WriteLine($"{"- TYPE:",-14}{user.GetType().Name}");
            Console.WriteLine($"{"- FIRST NAME:",-14}{user.firstName}");
            Console.WriteLine($"{"- LAST NAME:",-14}{user.lastName}");
            Console.WriteLine($"{"- SSN:",-14}{user.ssn}");
            Console.WriteLine($"{"- PHONENR.:",-14}{user.phone}");
            Console.WriteLine($"{"- EMAIL:",-14}{user.email}\n");
            Console.WriteLine("\n>> Press any key to continue.");
            Console.ReadKey(true);

            users.Add(user);
            SaveUsers();

            Program.logger.LogActivity($"INFO: RegisterUser() - New account registration successful. USER: {user.loginName}");

            Program.logger.LogActivity($"ERROR: RegisterUser() - New account registration unsuccessful.");
        }
        protected void DeregisterUser() // NYI: ADD LOGGER!
        {
            int userID = -1;
            int userIndex = -1;
            Console.Clear();
            Console.WriteLine("<< ACCOUNT DEREGISTRATION >>\n");
            Console.WriteLine($">> Select an option: \n{"- [1]",-8}View users.\n{"- [2]",-8}Search user.\n{"- [ESC]",-8}Exit");
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            if (keyInfo.Key == ConsoleKey.D1 || keyInfo.Key == ConsoleKey.NumPad1)
            {
                Console.WriteLine();
                this.ViewUsers(false, false);
                Console.Write("\n>> Enter id of user to deregister: ");
                try
                {
                    userID = int.Parse(Console.ReadLine());
                }
                catch
                {
                    Console.WriteLine(">> Invalid format! Account deregistration cancelled.");
                    Task.Delay(1500).Wait();
                    return;
                }
            }
            else if (keyInfo.Key == ConsoleKey.D2 || keyInfo.Key == ConsoleKey.NumPad2)
            {
                try
                {
                    userID = this.SearchUser(false, false);
                }
                catch
                {
                    Console.WriteLine(">> Search users unsuccessful!");
                    Task.Delay(1500).Wait();
                    return;
                }
            }
            else if (keyInfo.Key == ConsoleKey.Escape)
            {
                Console.WriteLine(">> Account deregistration cancelled!");
                Task.Delay(1500).Wait();
                return;
            }
            else
            {
                Console.WriteLine($">> INVALID KEY: [{keyInfo.Key}]");
                Task.Delay(1500).Wait();
                return;
            }

            for (int i = 0; i < users.Count; i++) if (users[i].id == userID) userIndex = i;

            if (userIndex > 0 && userIndex < users.Count)
            {
                Console.WriteLine($">> Deregister account ({users[userIndex].loginName})?");
                Console.WriteLine($"{"- [Y]",-8}Yes");
                Console.WriteLine($"{"- [N]",-8}No");
                Console.WriteLine($"{"- [ESC]",-8}Exit");
                keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Y)
                {
                    Console.WriteLine($">> User ({users[userIndex].loginName}) deregistration successful!");
                    users.RemoveAt(userIndex);
                }
                else if (keyInfo.Key == ConsoleKey.N || keyInfo.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine($">> User ({users[userIndex].loginName}) deregistration cancelled!");
                    Task.Delay(1500).Wait();
                }
                else
                {
                    Console.WriteLine($">> INVALID KEY: [{keyInfo.Key}], deregistration unsuccessful!");
                    Task.Delay(1500).Wait();
                }
                SaveUsers();
            }
            else
            {
                Console.WriteLine(">> Account deregistration unsuccesful!");
                Task.Delay(1500).Wait();
            }
        }
        protected void UpdateInfo() // NYI: ADD LOGGER!
        {
            Console.Clear();
            Console.WriteLine("<< Update Information >>\n");
            Console.WriteLine(">> Select an option!");
            Console.WriteLine("- [1]   First Name.");
            Console.WriteLine("- [2]   Last Name.");
            Console.WriteLine("- [3]   Email.");
            Console.WriteLine("- [4]   Phone.");
            Console.WriteLine("- [ESC] Exit.");

            string preUpdate, postUpdate;
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            if (keyInfo.Key == ConsoleKey.D1 || keyInfo.Key == ConsoleKey.NumPad1)
            {
                Console.Write(">> New first name: ");
                postUpdate = Console.ReadLine();
                preUpdate = firstName;
                Console.WriteLine($">> Update information from ({preUpdate}) to ({postUpdate})?");
                Console.WriteLine(">> Press any key to accept, or [ESC] to cancel!");

                keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Escape) return;
                else firstName = postUpdate;
            }
            else if (keyInfo.Key == ConsoleKey.D2 || keyInfo.Key == ConsoleKey.NumPad2)
            {
                Console.Write(">> New last name: ");
                postUpdate = Console.ReadLine();
                preUpdate = lastName;
                Console.WriteLine($">> Update information from ({preUpdate}) to ({postUpdate})?");
                Console.WriteLine(">> Press any key to accept, or [ESC] to cancel!");

                keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Escape) return;
                else lastName = postUpdate;
            }
            else if (keyInfo.Key == ConsoleKey.D3 || keyInfo.Key == ConsoleKey.NumPad3)
            {
                Console.Write(">> New email: ");
                postUpdate = Console.ReadLine();
                preUpdate = email;
                Console.WriteLine($">> Update information from ({preUpdate}) to ({postUpdate})?");
                Console.WriteLine(">> Press any key to accept, or [ESC] to cancel!");

                keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Escape) return;
                else email = postUpdate;
            }
            else if (keyInfo.Key == ConsoleKey.D4 || keyInfo.Key == ConsoleKey.NumPad4)
            {
                Console.Write(">> New phone: ");
                postUpdate = Console.ReadLine();
                preUpdate = phone;
                Console.WriteLine($">> Update information from ({preUpdate}) to ({postUpdate})?");
                Console.WriteLine(">> Press any key to accept, or [ESC] to cancel!");

                keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Escape) return;
                else phone = postUpdate;
            }
            else if (keyInfo.Key == ConsoleKey.Escape)
            {
                Console.Write(">> Update information cancelled!");
                Task.Delay(1500).Wait();
                return;
            }
            else
            {
                Console.WriteLine($">> INVALID KEY: [{keyInfo.Key}]");
                Task.Delay(1500).Wait();
                return;
            }
            Console.WriteLine($">> Update information successful!");
            Task.Delay(1500).Wait();
            SaveUsers();
        }
        protected void UpdateLogin() // NYI: ADD LOGGER!
        {
            Console.Clear();
            Console.WriteLine("<< Update Login >>\n");
            Console.WriteLine(">> Select an option!");
            Console.WriteLine("- [1]   Username.");
            Console.WriteLine("- [2]   Password.");
            Console.WriteLine("- [ESC] Exit.");

            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            if (keyInfo.Key == ConsoleKey.D1 || keyInfo.Key == ConsoleKey.NumPad1)
            {
                Console.Write(">> New login name: ");
                string newUsername = Console.ReadLine();
                while (!Login.CheckLoginName(newUsername))
                {
                    Console.Write(">> Login name unavailable, try again: ");
                    newUsername = Console.ReadLine();
                }

                loginName = newUsername;
                Console.WriteLine($">> New login name ({loginName}) successfully updated!");
            }
            else if (keyInfo.Key == ConsoleKey.D2 || keyInfo.Key == ConsoleKey.NumPad2)
            {
                string loginPassA, loginPassB;
                do
                {
                    Console.Write(">> New password: ");
                    loginPassA = Login.MaskPassword();
                    Console.Write("\n>> Confirm password: ");
                    loginPassB = Login.MaskPassword();
                    Console.WriteLine();
                    if (loginPassA != loginPassB) Console.WriteLine(">> Confirm password failed!");
                } while (loginPassA != loginPassB);

                loginPass = loginPassA;
                Console.WriteLine(">> New login password successfully updated!");
            }
            else if (keyInfo.Key == ConsoleKey.Escape) Console.Write(">> Update login cancelled!");
            else Console.WriteLine($">> INVALID KEY: [{keyInfo.Key}]");

            Task.Delay(1500).Wait();
            SaveUsers();
        }
        protected void ViewInfo()
        {
            Console.Clear();
            Console.WriteLine("<< VIEW INFORMATION >>\n");
            Console.WriteLine($"- TYPE:       {GetType().Name}");
            Console.WriteLine($"- ID:         {id}");
            Console.WriteLine($"- FIRST NAME: {firstName}");
            Console.WriteLine($"- LAST NAME:  {lastName}");
            Console.WriteLine($"- SSN:        {ssn}");
            Console.WriteLine($"- PHONE.NR:   {phone}");
            Console.WriteLine($"- EMAIL:      {email}");
            Console.WriteLine($"- LOGIN NAME: {loginName}");
            Console.WriteLine($"- LOGIN PASS: {loginPass}");
            Console.WriteLine("\n>> Press any key to continue.");
            Console.ReadKey(true);
        }
        protected int SearchUser(bool header = true, bool footer = true)
        {
            List<User> searchedUsers = new List<User>();
            int returnID = -1;

            if (header)
            {
                Console.Clear();
                Console.WriteLine("<< SEARCH USER >>\n");
            }
            Console.Write(">> Enter first name, last name or both: ");
            string[] input = Console.ReadLine().ToLower().Trim().Split(" ");

            List<User> primarySearch = new List<User>();
            foreach (User user in users)
            {
                if (input[0] == user.firstName.ToLower() || input[0] == user.lastName.ToLower())
                {
                    primarySearch.Add(user);
                }
            }

            searchedUsers = primarySearch;
            List<User> secondarySearch = new List<User>();
            if (input.Length > 1)
            {
                foreach (User user in primarySearch)
                {
                    if (input[1] == user.firstName.ToLower() || input[1] == user.lastName.ToLower())
                    {
                        secondarySearch.Add(user);
                    }
                }
                searchedUsers = secondarySearch;
            }

            Console.WriteLine(">> Search result:\n");
            if (searchedUsers.Count > 0)
            {
                int[] selectUser = new int[searchedUsers.Count];
                for (int i = 0; i < searchedUsers.Count; i++)
                {
                    ViewUser(searchedUsers[i].id, false, footer);
                    selectUser[i] = searchedUsers[i].id;
                }
                try
                {
                    Console.Write("\n>> Enter searched user id: ");
                    returnID = int.Parse(Console.ReadLine());
                }
                catch
                {
                    Console.WriteLine("\n>> Invalid input!");
                    Environment.Exit(0);
                }
            }
            else
            {
                string searchInput = string.Empty;
                foreach (string s in input) searchInput += s + " ";
                Console.WriteLine($">> No results found for: {searchInput}");
            }
            return returnID;
        }
        protected void ViewUsers(bool header = true, bool footer = true)
        {
            List<User> adminUsers = users.Where(u => u.GetType() == typeof(Admin)).ToList();
            List<User> staffUsers = users.Where(u => u.GetType() == typeof(Staff)).ToList();
            List<User> customerUsers = users.Where(u => u.GetType() == typeof(Customer)).ToList();
            adminUsers.Sort((x, y) => x.lastName.CompareTo(y.lastName));
            staffUsers.Sort((x, y) => x.lastName.CompareTo(y.lastName));
            customerUsers.Sort((x, y) => x.lastName.CompareTo(y.lastName));

            if (header)
            {
                Console.Clear();
                Console.WriteLine("<< VIEW USERS >>\n");
            }
            if (this is Admin)
            {
                string typeUser = "Admin";
                int x, y;
                (x, y) = Console.GetCursorPosition();

                List<User> allUsers = new();
                allUsers.AddRange(adminUsers);
                allUsers.AddRange(staffUsers);
                allUsers.AddRange(customerUsers);

                foreach (User u in allUsers)
                {
                    if (typeUser != u.GetType().Name || x >= 120)
                    {
                        x = 0;
                        y += 5;
                    }

                    Console.SetCursorPosition(x, y);
                    Console.Write($"{"- TYPE:",-15}{u.GetType().Name}");
                    Console.SetCursorPosition(x, y + 1);
                    Console.Write($"{"- ID:",-15}{u.id}");
                    Console.SetCursorPosition(x, y + 2);
                    Console.Write($"{"- FIRST NAME:",-15}{u.firstName}");
                    Console.SetCursorPosition(x, y + 3);
                    Console.Write($"{"- LAST NAME:",-15}{u.lastName}\n");

                    x += 30;
                    typeUser = u.GetType().Name;
                }
            }
            else
            {
                string typeUser = "Staff";
                int x, y;
                (x, y) = Console.GetCursorPosition();

                List<User> allUsers = new();
                allUsers.AddRange(staffUsers);
                allUsers.AddRange(customerUsers);
                foreach (User u in allUsers)
                {
                    if (typeUser != u.GetType().Name || x >= 120)
                    {
                        x = 0;
                        y += 5;
                    }

                    Console.SetCursorPosition(x, y);
                    Console.Write($"{"- TYPE:",-15}{u.GetType().Name}");
                    Console.SetCursorPosition(x, y + 1);
                    Console.Write($"{"- ID:",-15}{u.id}");
                    Console.SetCursorPosition(x, y + 2);
                    Console.Write($"{"- FIRST NAME:",-15}{u.firstName}");
                    Console.SetCursorPosition(x, y + 3);
                    Console.Write($"{"- LAST NAME:",-15}{u.lastName}\n");

                    x += 30;
                    typeUser = u.GetType().Name;
                }
            }
            if (footer)
            {
                Console.WriteLine("\n>> Press any key to continue, or [V] to view a user.");
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.V)
                {
                    try
                    {
                        Console.Write(">> Enter id of user: ");
                        int id = int.Parse(Console.ReadLine());
                        ViewUser(id, true);
                    }
                    catch
                    {
                        Console.WriteLine(">> Invalid input, view user cancelled!");
                        Task.Delay(1500).Wait();
                    }
                }
            }
        }
        protected void ViewUser(int id, bool header = true, bool footer = true)
        {
            if (header)
            {
                Console.Clear();
                Console.WriteLine("<< VIEW USER >>\n");
            }
            foreach (User user in users)
            {
                if (user.id == id)
                {
                    Console.WriteLine($"{"- TYPE:",-15}{user.GetType().Name}");
                    Console.WriteLine($"{"- ID:",-15}{user.id}");
                    Console.WriteLine($"{"- FIRST NAME:",-15}{user.firstName}");
                    Console.WriteLine($"{"- LAST NAME:",-15}{user.lastName}");
                    Console.WriteLine($"{"- SSN:",-15}{user.ssn}");
                    Console.WriteLine($"{"- EMAIL:",-15}{user.email}");
                    Console.WriteLine($"{"- PHONE:",-15}{user.phone}");
                    Console.WriteLine($"{"- LOGIN:",-15}{user.loginName}");

                    if (user is Customer)
                    {
                        Customer customer = (Customer)user;
                        Console.WriteLine($"{"- MEMBER:",-15}{customer.isSub}");
                        Console.WriteLine($"{"- SUB.START:",-15}{customer.subStart}");
                        Console.WriteLine($"{"- SUB.END:",-15}{customer.subStart}");
                    }
                }
            }
            if (footer)
            {
                Console.WriteLine("\n>> Press any key to continue.");
                Console.ReadKey(true);
            }
        }
        protected virtual void UserManagerMenu() { }
        protected virtual void ActivityManagerMenu() { }
        protected virtual void ReservationManagerMenu() { }
        protected virtual void ReservableManagerMenu() { }
        protected void AccountManagerMenu()
        {
            Console.Clear();
            Console.WriteLine("<< ACCOUNT MANAGER >>\n");
            Console.WriteLine("- [1]   Update personal information.");
            Console.WriteLine("- [2]   Update login details.");
            Console.WriteLine("- [3]   View personal information. (SENSITIVE INFORMATION)");
            Console.WriteLine("- [ESC] Exit.");
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

            if (keyInfo.Key == ConsoleKey.D1 || keyInfo.Key == ConsoleKey.NumPad1) UpdateInfo();
            else if (keyInfo.Key == ConsoleKey.D2 || keyInfo.Key == ConsoleKey.NumPad2) UpdateLogin();
            else if (keyInfo.Key == ConsoleKey.D3 || keyInfo.Key == ConsoleKey.NumPad3) ViewInfo();
            else if (keyInfo.Key == ConsoleKey.Escape) Console.WriteLine(">> Account manager cancelled!");
            else Console.WriteLine($">> INVALID KEY: [{keyInfo.Key}]");
            Task.Delay(1000).Wait();
        }
        public virtual void MainMenu()
        {

        }
    }
    public class Admin : User
    {
        public Admin(int id, string firstname, string lastname, string ssn, string phone, string email, string loginName, string loginPass)
            : base(id, firstname, lastname, ssn, phone, email, loginName, loginPass) { }
        public Admin() : base() { }
        protected override void UserManagerMenu()
        {
            Console.Clear();
            Console.WriteLine("<< USER MANAGER >>\n");
            Console.WriteLine("- [1]   Register a user.");
            Console.WriteLine("- [2]   Deregister a user.");
            Console.WriteLine("- [3]   View all users.");
            Console.WriteLine("- [ESC] Exit.");
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

            if (keyInfo.Key == ConsoleKey.D1 || keyInfo.Key == ConsoleKey.NumPad1) RegisterUser();
            else if (keyInfo.Key == ConsoleKey.D2 || keyInfo.Key == ConsoleKey.NumPad2) DeregisterUser();
            else if (keyInfo.Key == ConsoleKey.D3 || keyInfo.Key == ConsoleKey.NumPad3) ViewUsers();
            else if (keyInfo.Key == ConsoleKey.Escape) Console.WriteLine(">> User manager cancelled!");
            else Console.WriteLine($">> INVALID KEY: [{keyInfo.Key}]");
            Task.Delay(1000).Wait();
        }
        public override void MainMenu()
        {
            bool cancel = false;
            while (!cancel)
            {
                Console.Clear();
                Console.WriteLine("<< ADMIN MENU >>\n");
                Console.WriteLine($">> LOGGED IN: {firstName} {lastName}");
                Console.WriteLine("\n>> Select an option!");
                Console.WriteLine($"{"- [1]",-8}Manage users.\n{"- [2]",-8}View log activities.\n{"- [ESC]",-8}Log out.");
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                if (keyInfo.Key == ConsoleKey.D1 || keyInfo.Key == ConsoleKey.NumPad1) UserManagerMenu();
                else if (keyInfo.Key == ConsoleKey.D2 || keyInfo.Key == ConsoleKey.NumPad2) ; //ViewLog()
                else if (keyInfo.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine($"\n>> LOGGED OUT: {firstName} {lastName}");
                    Task.Delay(1000).Wait();
                    cancel = true;
                }
                else Console.WriteLine($">> INVALID KEY: [{keyInfo.Key}]");
            }
        }
    }
    public class Staff : User
    {
        public Staff(int id, string firstname, string lastname, string ssn, string phone, string email, string loginName, string loginPass)
            : base(id, firstname, lastname, ssn, phone, email, loginName, loginPass) { }
        public Staff() : base() { }
        protected override void UserManagerMenu()
        {
            Console.Clear();
            Console.WriteLine("<< CUSTOMER MANAGER >>\n");
            Console.WriteLine("- [1]   Register a customer.");
            Console.WriteLine("- [2]   Deregister a customer.");
            Console.WriteLine("- [3]   View all customers.");
            Console.WriteLine("- [ESC] Exit.");
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

            if (keyInfo.Key == ConsoleKey.D1 || keyInfo.Key == ConsoleKey.NumPad1) RegisterUser();
            else if (keyInfo.Key == ConsoleKey.D2 || keyInfo.Key == ConsoleKey.NumPad2) DeregisterUser();
            else if (keyInfo.Key == ConsoleKey.D3 || keyInfo.Key == ConsoleKey.NumPad3) ViewUsers();
            else if (keyInfo.Key == ConsoleKey.Escape) Console.WriteLine(">> Customer manager cancelled!");
            else Console.WriteLine($">> INVALID KEY: [{keyInfo.Key}]");
            Task.Delay(1000).Wait();
        }
        protected override void ActivityManagerMenu()
        {
            Console.Clear();
            Console.WriteLine("<< ACTIVITY MANAGER >>\n");
            Console.WriteLine("- [1]   Register an activity.");
            Console.WriteLine("- [2]   Deregister an activity. (NYI)");
            Console.WriteLine("- [3]   View all activities.");
            Console.WriteLine("- [ESC] Exit.");
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

            if (keyInfo.Key == ConsoleKey.D1 || keyInfo.Key == ConsoleKey.NumPad1) Activity.NewActivity(this.id);
            else if (keyInfo.Key == ConsoleKey.D2 || keyInfo.Key == ConsoleKey.NumPad2) return; // NYI: Activity.DeregisterActivity
            else if (keyInfo.Key == ConsoleKey.D3 || keyInfo.Key == ConsoleKey.NumPad3) Schedule.ViewScheduleMenu();
            else if (keyInfo.Key == ConsoleKey.Escape) Console.WriteLine(">> Activity manager cancelled!");
            else Console.WriteLine($">> INVALID KEY: [{keyInfo.Key}]");
            Task.Delay(1000).Wait();
        }
        protected override void ReservableManagerMenu()
        {
            Console.Clear();
            Console.WriteLine("<< RESERVABLE MANAGER >>\n");
            Console.WriteLine("- [1]   Register a reservable.");
            Console.WriteLine("- [2]   Deregister a reservable.");
            Console.WriteLine("- [3]   Edit a reservable.");
            Console.WriteLine("- [4]   View all reservables.");
            Console.WriteLine("- [ESC] Exit.");
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

            if (keyInfo.Key == ConsoleKey.D1 || keyInfo.Key == ConsoleKey.NumPad1) Reservable.NewReservable(this);
            else if (keyInfo.Key == ConsoleKey.D2 || keyInfo.Key == ConsoleKey.NumPad2) Reservable.DeleteReservable(this);
            else if (keyInfo.Key == ConsoleKey.D3 || keyInfo.Key == ConsoleKey.NumPad3) Reservable.EditReservable(this);
            else if (keyInfo.Key == ConsoleKey.D4 || keyInfo.Key == ConsoleKey.NumPad4) Reservable.ViewReservables();
            else if (keyInfo.Key == ConsoleKey.Escape) Console.WriteLine(">> Reservable manager cancelled!");
            else Console.WriteLine($">> INVALID KEY: [{keyInfo.Key}]");
            Task.Delay(1000).Wait();
        }
        public override void MainMenu()
        {
            bool cancel = false;
            while (!cancel)
            {
                Console.Clear();
                Console.WriteLine("<< STAFF MENU >>\n");
                Console.WriteLine($">> LOGGED IN: {firstName} {lastName}");
                Console.WriteLine("\n>> Select an option!");
                Console.WriteLine($"{"- [1]",-8}Manage customers.\n{"- [2]",-8}Manage activities.\n{"- [3]",-8}Manage reservations.\n{"- [4]",-8}Manage reservables.\n{"- [5]",-8}Manage account.\n{"- [ESC]",-8}Log out.");
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                if (keyInfo.Key == ConsoleKey.D1 || keyInfo.Key == ConsoleKey.NumPad1) UserManagerMenu();
                else if (keyInfo.Key == ConsoleKey.D2 || keyInfo.Key == ConsoleKey.NumPad2) ActivityManagerMenu();
                else if (keyInfo.Key == ConsoleKey.D3 || keyInfo.Key == ConsoleKey.NumPad3) ReservationManagerMenu();
                else if (keyInfo.Key == ConsoleKey.D4 || keyInfo.Key == ConsoleKey.NumPad4) ReservableManagerMenu();
                else if (keyInfo.Key == ConsoleKey.D5 || keyInfo.Key == ConsoleKey.NumPad5) AccountManagerMenu();
                else if (keyInfo.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine($"\n>> LOGGED OUT: {firstName} {lastName}");
                    Task.Delay(1000).Wait();
                    cancel = true;
                }
                else Console.WriteLine($">> INVALID KEY: [{keyInfo.Key}]");
            }
        }
    }
    public class Customer : User
    {
        public DateTime subStart { get; set; }
        public DateTime subEnd { get; set; }
        public bool isSub { get; set; }
        public Customer(int id, string firstname, string lastname, string ssn, string phone, string email, string loginName, string loginPass,
                        DateTime subStart = default(DateTime), DateTime subEnd = default(DateTime), bool isMember = false)
            : base(id, firstname, lastname, ssn, phone, email, loginName, loginPass)
        {
            this.subStart = subStart;
            this.subEnd = subEnd;
            this.isSub = isMember;
        }
        public Customer() : base() { }
        public void AddSubscription() // NYI: ADD LOGGER!
        {
            Console.WriteLine($"\n>> Select membership type:\n{"- [1]",-8}One Day membership.\n{"- [2]",-8}One Month membership.\n{"- [3]",-8}One Year membership.\n{"- [ESC]",-8}Cancel, no membership.");
            ConsoleKeyInfo keyPressed = Console.ReadKey(true);

            if (keyPressed.Key == ConsoleKey.D1 || keyPressed.Key == ConsoleKey.NumPad1)
            {
                if (this.subEnd < DateTime.Now) this.subStart = DateTime.Now.Date;
                this.subEnd = DateTime.Now.AddDays(1).Date;
                this.isSub = true;

                Console.WriteLine($"\n>> Successfully added a \"One-Day\" subscription plan to {this.lastName}.");
                Console.WriteLine($">> New subscription: {this.subStart.Date} - {this.subEnd.Date}");
            }
            if (keyPressed.Key == ConsoleKey.D2 || keyPressed.Key == ConsoleKey.NumPad2)
            {
                if (this.subEnd < DateTime.Now) this.subStart = DateTime.Now.Date;
                this.subEnd = DateTime.Now.AddMonths(1).Date;
                this.isSub = true;

                Console.WriteLine($"\n>> Successfully added a \"One-Month\" subscription plan to {this.lastName}.");
                Console.WriteLine($">> New subscription: {this.subStart.Date} - {this.subEnd.Date}");
            }
            if (keyPressed.Key == ConsoleKey.D3 || keyPressed.Key == ConsoleKey.NumPad3)
            {
                if (this.subEnd < DateTime.Now) this.subStart = DateTime.Now.Date;
                this.subEnd = DateTime.Now.AddYears(1).Date;
                this.isSub = true;

                Console.WriteLine($"\n>> Successfully added a \"One-Year\" subscription plan to {this.lastName}.");
                Console.WriteLine($">> New subscription: {this.subStart.Date} - {this.subEnd.Date}");
            }
            if (keyPressed.Key == ConsoleKey.Escape) Console.WriteLine(">> New subscription cancelled!");
        }
        protected override void ActivityManagerMenu()
        {
            Console.Clear();
            Console.WriteLine("<< ACTIVITY MANAGER >>\n");
            Console.WriteLine("- [1]   Register for an activity. (NYI)");
            Console.WriteLine("- [2]   Deregister for an activity.(NYI)");
            Console.WriteLine("- [3]   View all activities.");
            Console.WriteLine("- [ESC] Exit.");
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

            if (keyInfo.Key == ConsoleKey.D1 || keyInfo.Key == ConsoleKey.NumPad1) return;
            else if (keyInfo.Key == ConsoleKey.D2 || keyInfo.Key == ConsoleKey.NumPad2) return; 
            else if (keyInfo.Key == ConsoleKey.D3 || keyInfo.Key == ConsoleKey.NumPad3) Schedule.ViewScheduleMenu();
            else if (keyInfo.Key == ConsoleKey.Escape) Console.WriteLine(">> Activity manager cancelled!");
            else Console.WriteLine($">> INVALID KEY: [{keyInfo.Key}]");
            Task.Delay(1000).Wait();
        }
        protected override void ReservationManagerMenu()
        {
            Console.Clear();
            Console.WriteLine("<< RESERVATION MANAGER >>\n");
            Console.WriteLine("- [1]   Register a reservation.");
            Console.WriteLine("- [2]   Deregister a reservation. (NYI)");
            Console.WriteLine("- [3]   View all reservations. (NYI)");
            Console.WriteLine("- [ESC] Exit.");
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

            if (keyInfo.Key == ConsoleKey.D1 || keyInfo.Key == ConsoleKey.NumPad1) Reservation.NewReservation(this);
            else if (keyInfo.Key == ConsoleKey.D2 || keyInfo.Key == ConsoleKey.NumPad2) return; // NYI: Reservation.RegisterReservation;
            else if (keyInfo.Key == ConsoleKey.D3 || keyInfo.Key == ConsoleKey.NumPad3) return; // NYI: Reservation.ViewReservations.
            else if (keyInfo.Key == ConsoleKey.Escape) Console.WriteLine(">> Reservation manager cancelled!");
            else Console.WriteLine($">> INVALID KEY: [{keyInfo.Key}]");
            Task.Delay(1000).Wait();
        }
        public override void MainMenu()
        {
            bool cancel = false;
            while (!cancel)
            {
                Console.Clear();
                Console.WriteLine("<< CUSTOMER MENU >>\n");
                Console.WriteLine($">> LOGGED IN: {firstName} {lastName}");
                Console.WriteLine("\n>> Select an option!");
                Console.WriteLine($"{"- [1]",-8}Manage activities.\n{"- [2]",-8}Manage reservations.\n{"- [3]",-8}Manage account.\n{"- [ESC]",-8}Log out.");
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                if (keyInfo.Key == ConsoleKey.D1 || keyInfo.Key == ConsoleKey.NumPad1) ActivityManagerMenu();
                else if (keyInfo.Key == ConsoleKey.D2 || keyInfo.Key == ConsoleKey.NumPad2) ReservationManagerMenu();
                else if (keyInfo.Key == ConsoleKey.D3 || keyInfo.Key == ConsoleKey.NumPad3) AccountManagerMenu();
                else if (keyInfo.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine($"\n>> LOGGED OUT: {firstName} {lastName}");
                    Task.Delay(1000).Wait();
                    cancel = true;
                }
                else Console.WriteLine($">> INVALID KEY: [{keyInfo.Key}]");
            }
        }
    }
}