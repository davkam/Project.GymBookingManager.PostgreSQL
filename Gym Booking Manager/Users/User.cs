using Gym_Booking_Manager.Activities;
using Gym_Booking_Manager.ActivityExtenstion;
using Gym_Booking_Manager.Dates;
using Gym_Booking_Manager.DBStorage;
using Gym_Booking_Manager.Managements;
using Gym_Booking_Manager.Reservables;
using Gym_Booking_Manager.Reservations;

namespace Gym_Booking_Manager.Users
{
    public abstract class User
    {
        public static int getUserID;
        public static List<User> users = new List<User>();
        protected static int oneDaySubFee = 5;
        protected static int oneMonthSubFee = 25;
        protected static int oneYearSubFee = 250;

        public int id { get; set; } // NYI: Use GUID
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
        public static void CreateMainAdmin()
        {
            Admin mainAdmin = new Admin()
            {
                id = 0,
                firstName = "ADMIN",
                lastName = "MAIN",
                loginName = "admin",
                loginPass = "abc123"
            };

            users.Add(mainAdmin);
            getUserID = 1;
        }
        private static int GetUserID()
        {
            int id = getUserID;
            getUserID++;
            return id;
        }
        public string GetUserType() => this switch
        {
            Admin => "admin",
            Staff => "staff",
            Customer => "customer",
            _ => throw new InvalidOperationException("Unknown user type")
        };
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

                if (keyPressed.Key == ConsoleKey.Y) customer.AddSubscription(false, false);
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
            Database.Instance.AddUser(user);

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
                    Database.Instance.RemoveUser(users[userIndex]);
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
            Database.Instance.UpdateUser(this);
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
            Database.Instance.RemoveUser(this);
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
        protected virtual void UserMenu() { }
        protected virtual void ActivitiesMenu() { }
        protected virtual void ReservationsMenu() { }
        protected virtual void ReservablesMenu() { }
        protected virtual void SubscriptionsMenu() { }
        protected void AccountMenu()
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
            else Console.WriteLine($">> INVALID KEY: [{keyInfo.Key}]. Account manager cancelled!");
            Task.Delay(1000).Wait();
        }
        public abstract void MainMenu();
    }
    public class Admin : User
    {
        public Admin(int id, string firstname, string lastname, string ssn, string phone, string email, string loginName, string loginPass)
            : base(id, firstname, lastname, ssn, phone, email, loginName, loginPass) { }
        public Admin() : base() { }
        protected override void UserMenu()
        {
            Console.Clear();
            Console.WriteLine("<< USER MANAGER >>\n");
            Console.WriteLine("- [1]   Register a user.");
            Console.WriteLine("- [2]   Deregister a user.");
            Console.WriteLine("- [3]   Search User.");
            Console.WriteLine("- [4]   View all users.");
            Console.WriteLine("- [ESC] Exit.");
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

            if (keyInfo.Key == ConsoleKey.D1 || keyInfo.Key == ConsoleKey.NumPad1) RegisterUser();
            else if (keyInfo.Key == ConsoleKey.D2 || keyInfo.Key == ConsoleKey.NumPad2) DeregisterUser();
            else if (keyInfo.Key == ConsoleKey.D3 || keyInfo.Key == ConsoleKey.NumPad3) SearchUser();
            else if (keyInfo.Key == ConsoleKey.D4 || keyInfo.Key == ConsoleKey.NumPad4) ViewUsers();
            else if (keyInfo.Key == ConsoleKey.Escape) Console.WriteLine(">> User manager cancelled!");
            else Console.WriteLine($">> INVALID KEY: [{keyInfo.Key}]. User manager cancelled!");
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
                Console.WriteLine($"{"- [1]",-8}User manager.\n{"- [2]",-8}Log manager.(NYI)\n{"- [ESC]",-8}Log out.");
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                if (keyInfo.Key == ConsoleKey.D1 || keyInfo.Key == ConsoleKey.NumPad1) UserMenu();
                else if (keyInfo.Key == ConsoleKey.D2 || keyInfo.Key == ConsoleKey.NumPad2) ; // NYI: LogMenu()
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
        protected override void UserMenu()
        {
            Console.Clear();
            Console.WriteLine("<< CUSTOMER MANAGER >>\n");
            Console.WriteLine("- [1]   Register a customer.");
            Console.WriteLine("- [2]   Deregister a customer.");
            Console.WriteLine("- [3]   Search customers.");
            Console.WriteLine("- [4]   View all customers.");
            Console.WriteLine("- [ESC] Exit.");
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

            if (keyInfo.Key == ConsoleKey.D1 || keyInfo.Key == ConsoleKey.NumPad1) RegisterUser();
            else if (keyInfo.Key == ConsoleKey.D2 || keyInfo.Key == ConsoleKey.NumPad2) DeregisterUser();
            else if (keyInfo.Key == ConsoleKey.D3 || keyInfo.Key == ConsoleKey.NumPad3) SearchUser();
            else if (keyInfo.Key == ConsoleKey.D4 || keyInfo.Key == ConsoleKey.NumPad4) ViewUsers();
            else if (keyInfo.Key == ConsoleKey.Escape) Console.WriteLine(">> Customer manager cancelled!");
            else Console.WriteLine($">> INVALID KEY: [{keyInfo.Key}]. Customer manager cancelled!");
            Task.Delay(1000).Wait();
        }
        protected override void ActivitiesMenu()    // TBD: Add options!
        {
            Console.Clear();
            Console.WriteLine("<< ACTIVITY MANAGER >>\n");
            Console.WriteLine("- [1]   New activity.");
            Console.WriteLine("- [2]   Delete activity.");
            Console.WriteLine("- [3]   View all activities.");
            Console.WriteLine("- [ESC] Exit.");
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

            if (keyInfo.Key == ConsoleKey.D1 || keyInfo.Key == ConsoleKey.NumPad1) Activity.NewActivity(this);
            else if (keyInfo.Key == ConsoleKey.D2 || keyInfo.Key == ConsoleKey.NumPad2) Activity.DeleteActivity(this);
            else if (keyInfo.Key == ConsoleKey.D3 || keyInfo.Key == ConsoleKey.NumPad3) ActivityExt.ActivityCalendarMenu();
            else if (keyInfo.Key == ConsoleKey.Escape) Console.WriteLine(">> Activity manager cancelled!");
            else Console.WriteLine($">> INVALID KEY: [{keyInfo.Key}]. Activity manager cancelled!");
            Task.Delay(1000).Wait();
        }
        protected override void ReservablesMenu()
        {
            Console.Clear();
            Console.WriteLine("<< RESERVABLE MANAGER >>\n");
            Console.WriteLine("- [1]   New reservable.");
            Console.WriteLine("- [2]   Delete reservable.");
            Console.WriteLine("- [3]   Edit reservable.");
            Console.WriteLine("- [4]   View all reservables.");
            Console.WriteLine("- [ESC] Exit.");
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

            if (keyInfo.Key == ConsoleKey.D1 || keyInfo.Key == ConsoleKey.NumPad1) Reservable.NewReservable(this);
            else if (keyInfo.Key == ConsoleKey.D2 || keyInfo.Key == ConsoleKey.NumPad2) Reservable.DeleteReservable(this);
            else if (keyInfo.Key == ConsoleKey.D3 || keyInfo.Key == ConsoleKey.NumPad3) Reservable.EditReservable(this);
            else if (keyInfo.Key == ConsoleKey.D4 || keyInfo.Key == ConsoleKey.NumPad4) Reservable.ViewAllReservables();
            else if (keyInfo.Key == ConsoleKey.Escape) Console.WriteLine(">> Reservable manager cancelled!");
            else Console.WriteLine($">> INVALID KEY: [{keyInfo.Key}]. Reservable manager cancelled!");
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
                Console.WriteLine($"{"- [1]",-8}Customer manager.\n{"- [2]",-8}Activity manager.\n{"- [3]",-8}Reservation manager.\n{"- [4]",-8}Reservable manager.\n{"- [5]",-8}Account manager.\n{"- [ESC]",-8}Log out.");
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                if (keyInfo.Key == ConsoleKey.D1 || keyInfo.Key == ConsoleKey.NumPad1) UserMenu();
                else if (keyInfo.Key == ConsoleKey.D2 || keyInfo.Key == ConsoleKey.NumPad2) ActivitiesMenu();
                else if (keyInfo.Key == ConsoleKey.D3 || keyInfo.Key == ConsoleKey.NumPad3) ReservationsMenu();
                else if (keyInfo.Key == ConsoleKey.D4 || keyInfo.Key == ConsoleKey.NumPad4) ReservablesMenu();
                else if (keyInfo.Key == ConsoleKey.D5 || keyInfo.Key == ConsoleKey.NumPad5) AccountMenu();
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
        public bool isGuest { get; set; }
        public Customer(int id, string firstname, string lastname, string ssn, string phone, string email, string loginName, string loginPass,
                        DateTime subStart = default(DateTime), DateTime subEnd = default(DateTime), bool isSub = false)
            : base(id, firstname, lastname, ssn, phone, email, loginName, loginPass)
        {
            this.subStart = subStart;
            this.subEnd = subEnd;
            this.isSub = isSub;
            isGuest = false;
        }
        public Customer() : base() { }
        public void AddSubscription(bool header = true, bool footer = true) // NYI: ADD LOGGER!
        {
            if (header)
            {
                Console.Clear();
                Console.WriteLine("<< ADD SUBSCRIPTION >>");
            }
            Console.WriteLine($"\n>> Select membership type:");
            Console.WriteLine("- [1]   One-Day membership.");
            Console.WriteLine("- [2]   One-Month membership.");
            Console.WriteLine("- [3]   One-Year membership.");
            Console.WriteLine("- [ESC] Exit.");

            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            if (keyInfo.Key == ConsoleKey.D1 || keyInfo.Key == ConsoleKey.NumPad1)
            {
                Console.WriteLine($"\n>> Confirm new day subscription and pay {User.oneDaySubFee} Euros?");
                Console.WriteLine(">> Press any key to accept, or [ESC] to cancel.");
                keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine(">> Add subscription cancelled!");
                    Task.Delay(1500).Wait();
                    return;
                }

                if (subEnd < DateTime.Now) subStart = DateTime.Now.Date;
                subEnd = DateTime.Now.AddDays(1).Date;
                isSub = true;

                Console.WriteLine($">> Successfully added a \"One-Day\" subscription plan to {lastName}.");
                Console.WriteLine($">> New subscription: {subStart.Date.ToString()[0..^9]} - {subEnd.Date.ToString()[0..^9]}");
            }
            if (keyInfo.Key == ConsoleKey.D2 || keyInfo.Key == ConsoleKey.NumPad2)
            {
                Console.WriteLine($"\n>> Confirm new month subscription and pay {User.oneMonthSubFee} Euros?");
                Console.WriteLine(">> Press any key to accept, or [ESC] to cancel.");
                keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine(">> Add subscription cancelled!");
                    Task.Delay(1500).Wait();
                    return;
                }

                if (subEnd < DateTime.Now) subStart = DateTime.Now.Date;
                subEnd = DateTime.Now.AddMonths(1).Date;
                isSub = true;

                Console.WriteLine($">> Successfully added a \"One-Month\" subscription plan to {lastName}.");
                Console.WriteLine($">> New subscription: {subStart.Date.ToString()[0..^9]} - {subEnd.Date.ToString()[0..^9]}");
            }
            if (keyInfo.Key == ConsoleKey.D3 || keyInfo.Key == ConsoleKey.NumPad3)
            {
                Console.WriteLine($"\n>> Confirm new subscription and pay {User.oneYearSubFee} Euros?");
                Console.WriteLine(">> Press any key to accept, or [ESC] to cancel.");
                keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine(">> Add subscription cancelled!");
                    Task.Delay(1500).Wait();
                    return;
                }

                if (subEnd < DateTime.Now) subStart = DateTime.Now.Date;
                subEnd = DateTime.Now.AddYears(1).Date;
                isSub = true;

                Console.WriteLine($">> Successfully added a \"One-Year\" subscription plan to {firstName} {lastName}.");
                Console.WriteLine($">> New subscription: {subStart.Date.ToString()[0..^9]} - {subEnd.Date.ToString()[0..^9]}");
            }
            if (keyInfo.Key == ConsoleKey.Escape) Console.WriteLine(">> New subscription cancelled!");
            if (footer)
            {
                Task.Delay(2000).Wait();
                Database.Instance.UpdateUser(this);
            }
        }
        private void ViewSubscription()
        {
            Console.Clear();
            Console.WriteLine($"<< VIEW SUBSCRIPTION >>\n");
            Console.WriteLine($">> {firstName} {lastName}'s subscription:");
            Console.WriteLine($"- SUBSCRIBED:         {isSub}");
            Console.WriteLine($"- SUBSCRIPTION START: {subStart.Date.ToString()[0..^9]}");
            Console.WriteLine($"- SUBSCRIPTION END:   {subEnd.Date.ToString()[0..^9]}");
            Console.WriteLine("\n>> Press any key to continue.");
            Console.ReadKey(true);
        }
        protected override void ActivitiesMenu()    // TBD: Add/Update Options!
        {
            Console.Clear();
            Console.WriteLine("<< ACTIVITY MANAGER >>\n");
            Console.WriteLine("- [1]   Register an activity. (Subscribers Only)");
            Console.WriteLine("- [2]   Deregister an activity. (Subscribers Only)");
            Console.WriteLine("- [3]   View registered activities. (Subscribers Only)");
            Console.WriteLine("- [4]   View all activities");
            Console.WriteLine("- [ESC] Exit.");
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

            if (keyInfo.Key == ConsoleKey.D1 || keyInfo.Key == ConsoleKey.NumPad1) { if (isSub) Activity.RegisterActivity(this); }
            else if (keyInfo.Key == ConsoleKey.D2 || keyInfo.Key == ConsoleKey.NumPad2) { if (isSub) Activity.DeregisterActivity(this); }
            else if (keyInfo.Key == ConsoleKey.D3 || keyInfo.Key == ConsoleKey.NumPad3) { if (isSub) Activity.ViewRegisteredActivities(this); }
            else if (keyInfo.Key == ConsoleKey.D4 || keyInfo.Key == ConsoleKey.NumPad4) ActivityExt.ActivityCalendarMenu();
            else if (keyInfo.Key == ConsoleKey.Escape) Console.WriteLine(">> Activity manager cancelled!");
            else Console.WriteLine($">> INVALID KEY: [{keyInfo.Key}]. Activity manager cancelled!");
            Task.Delay(1500).Wait();
        }
        protected override void ReservationsMenu()
        {
            Console.Clear();
            Console.WriteLine("<< RESERVATION MANAGER >>\n");
            Console.WriteLine("- [1]   New reservation.");
            Console.WriteLine("- [2]   Delete reservation.");
            Console.WriteLine("- [3]   View reservations.");
            Console.WriteLine("- [ESC] Exit.");
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

            if (keyInfo.Key == ConsoleKey.D1 || keyInfo.Key == ConsoleKey.NumPad1) Reservation.NewReservation(this);
            else if (keyInfo.Key == ConsoleKey.D2 || keyInfo.Key == ConsoleKey.NumPad2) Reservation.DeleteReservation(this);
            else if (keyInfo.Key == ConsoleKey.D3 || keyInfo.Key == ConsoleKey.NumPad3) Reservation.ViewReservations(this);
            else if (keyInfo.Key == ConsoleKey.Escape) Console.WriteLine(">> Reservation manager cancelled!");
            else Console.WriteLine($">> INVALID KEY: [{keyInfo.Key}]. Reservatin manager cancelled!");
            Task.Delay(1500).Wait();
        }
        protected override void ReservablesMenu()
        {
            Console.Clear();
            Console.WriteLine("<< RESERVABLE MANAGER >>\n");
            Console.WriteLine("- [1]   View available reservables.");
            Console.WriteLine("- [2]   View all reservables.");
            Console.WriteLine("- [ESC] Exit.");
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

            if (keyInfo.Key == ConsoleKey.D1 || keyInfo.Key == ConsoleKey.NumPad1) Reservable.ViewAvailableReservables();
            else if (keyInfo.Key == ConsoleKey.D2 || keyInfo.Key == ConsoleKey.NumPad2) Reservable.ViewAllReservables();
            else if (keyInfo.Key == ConsoleKey.Escape) Console.WriteLine(">> Reservable manager cancelled!");
            else Console.WriteLine($">> INVALID KEY: [{keyInfo.Key}]. Reservable manager cancelled!");
            Task.Delay(1500).Wait();
        }
        protected override void SubscriptionsMenu()
        {
            Console.Clear();
            Console.WriteLine("<< SUBSCRIPTION MANAGER >>\n");
            Console.WriteLine($">> LOGGED IN: {firstName} {lastName}");
            Console.WriteLine("\n>> Select an option!");
            Console.WriteLine($"{"- [1]",-8}Add subscription.\n{"- [2]",-8}View subscription.\n{"- [ESC]",-8}Log out.");
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

            if (keyInfo.Key == ConsoleKey.D1 || keyInfo.Key == ConsoleKey.NumPad1) AddSubscription();
            else if (keyInfo.Key == ConsoleKey.D2 || keyInfo.Key == ConsoleKey.NumPad2) ViewSubscription();
            else if (keyInfo.Key == ConsoleKey.Escape)
            {
                Console.WriteLine($">> Subcription manager cancelled!");
                Task.Delay(1500).Wait();
                return;
            }
            else
            {
                Console.WriteLine($">> INVALID KEY: [{keyInfo.Key}]. Subcription manager cancelled!");
                Task.Delay(1500).Wait();
                return;
            }
        }
        public override void MainMenu()
        {
            bool cancel = false;
            if (!isGuest)
            {
                while (!cancel)
                {
                    Console.Clear();
                    Console.WriteLine("<< CUSTOMER MENU >>\n");
                    Console.WriteLine($">> LOGGED IN: {firstName} {lastName}");
                    Console.WriteLine("\n>> Select an option!");
                    Console.WriteLine($"{"- [1]",-8}Activity manager.\n{"- [2]",-8}Reservation manager.\n{"- [3]",-8}Reservable manager.\n{"- [4]",-8}Subscription manager.\n{"- [5]",-8}Account manager.\n{"- [ESC]",-8}Log out.");
                    ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                    if (keyInfo.Key == ConsoleKey.D1 || keyInfo.Key == ConsoleKey.NumPad1) ActivitiesMenu();
                    else if (keyInfo.Key == ConsoleKey.D2 || keyInfo.Key == ConsoleKey.NumPad2) ReservationsMenu();
                    else if (keyInfo.Key == ConsoleKey.D3 || keyInfo.Key == ConsoleKey.NumPad3) ReservablesMenu();
                    else if (keyInfo.Key == ConsoleKey.D4 || keyInfo.Key == ConsoleKey.NumPad4) SubscriptionsMenu();
                    else if (keyInfo.Key == ConsoleKey.D5 || keyInfo.Key == ConsoleKey.NumPad5) AccountMenu();
                    else if (keyInfo.Key == ConsoleKey.Escape)
                    {
                        Console.WriteLine($"\n>> LOGGED OUT: {firstName} {lastName}");
                        Task.Delay(1000).Wait();
                        cancel = true;
                    }
                    else Console.WriteLine($">> INVALID KEY: [{keyInfo.Key}]");
                }
            }
            else
            {
                while (!cancel)
                {
                    Console.Clear();
                    Console.WriteLine("<< GUEST MENU >>\n");
                    Console.WriteLine(">> Select an option!");
                    Console.WriteLine($"{"- [1]",-8}View activies.\n{"- [2]",-8}View reservables.\n{"- [ESC]",-8}Log Out.");
                    ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                    if (keyInfo.Key == ConsoleKey.D1 || keyInfo.Key == ConsoleKey.NumPad1) ActivityExt.ActivityCalendarMenu();
                    else if (keyInfo.Key == ConsoleKey.D2 || keyInfo.Key == ConsoleKey.NumPad2) Reservable.ViewAllReservables();
                    else if (keyInfo.Key == ConsoleKey.Escape)
                    {
                        Console.WriteLine($"\n>> LOGGED OUT: Guest");
                        Task.Delay(1000).Wait();
                        cancel = true;
                    }
                    else Console.WriteLine($">> INVALID KEY: [{keyInfo.Key}]");
                }
            }
        }
    }
}
