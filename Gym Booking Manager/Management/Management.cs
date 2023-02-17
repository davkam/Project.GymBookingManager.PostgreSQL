using Gym_Booking_Manager.Users;

namespace Gym_Booking_Manager.Managements
{
    public class Login
    {
        public static User UserLogin()   // NYI: SYS. ADMIN LOGIN!
        {
            int id = -1;
            int tries = 3;
            string? loginName = string.Empty;
            User? user;
            ConsoleKeyInfo keyInfo;
            while (true)
            {
                Console.Clear();
                Console.WriteLine("<< LOGIN MENU >>\n");
                Console.WriteLine(">> Select an option!");
                Console.WriteLine("- [1]   Member Login.");
                Console.WriteLine("- [2]   Guest Login.");
                Console.WriteLine("- [ESC] Exit");

                keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.D1 || keyInfo.Key == ConsoleKey.NumPad1)
                {
                    Console.Clear();
                    Console.WriteLine("<< LOGIN MENU >>");
                    Console.WriteLine("(Type quit to exit.)");
                    while (id == -1)
                    {
                        Console.Write("\n>> Enter username: ");

                        loginName = Console.ReadLine();

                        if (loginName.ToLower() == "quit") return null;

                        foreach (User u in User.users)
                        {
                            if (loginName == u.loginName) id = u.id;
                        }

                        if (id == -1) Console.WriteLine(">> Username does not exist!");
                        else Console.Clear();
                    }
                    user = User.users.Find(u => u.id == id);

                    Console.Clear();
                    Console.WriteLine("<< LOGIN MENU >>");
                    Console.WriteLine("(Type quit to exit.)");
                    Console.WriteLine($"\n>> Username: {loginName}");
                    while (true)
                    {
                        Console.Write("\n>> Enter password: ");
                        string loginPass = MaskPassword();

                        if (loginPass.ToLower() == "quit") return null;

                        if (loginPass == user.loginPass)
                        {
                            Console.WriteLine($"\n\n>> Welcome {user.firstName} {user.lastName}!");
                            Program.logger.LogActivity($"INFO: Login() - Login attempt successful. LOGIN USER: {loginName}");
                            Task.Delay(1500).Wait();
                            return user;
                        }
                        else
                        {
                            tries--;
                            Console.WriteLine("\n>> Incorrect password, " + tries + " tries left.");
                        }

                        if (tries == 0)
                        {
                            Console.WriteLine("\n>> Maximum tries reached, contact staff for support.");
                            Program.logger.LogActivity($"INFO: Login() - Login attempt unsuccessful. LOGIN USER: {loginName}");
                            Task.Delay(1000).Wait();
                            return null;
                        }
                    }
                }
                else if (keyInfo.Key == ConsoleKey.D2 || keyInfo.Key == ConsoleKey.NumPad2)
                {
                    Customer guest = new Customer();
                    guest.firstName = "Guest";
                    guest.lastName = "Login";
                    guest.isGuest = true;

                    Console.WriteLine("\n>> Welcome guest!");
                    Program.logger.LogActivity($"INFO: Login() - Guest login attempt successful.");
                    Task.Delay(1500).Wait();

                    return guest;
                }
                else if (keyInfo.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine(">> Login Cancelled!");
                    Program.logger.LogActivity($"INFO: Login() - Login attempt cancelled.");
                    Task.Delay(1000).Wait();
                    return null;
                }
                else
                {
                    Console.WriteLine(">> Invalid option!");
                    Task.Delay(1000).Wait();
                }
            }
        }
        public static bool CheckLoginName(string loginName)
        {
            foreach (User user in User.users)
            {
                if (loginName == user.loginName) return false;
            }
            return true;
        }
        public static string MaskPassword()
        {
            ConsoleKeyInfo keyInfo;
            string pass = string.Empty;

            do
            {
                keyInfo = Console.ReadKey(true);

                if (keyInfo.Key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    Console.Write("\b \b");
                    pass = pass[0..^1];
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write("*");
                    pass += keyInfo.KeyChar;
                }
                else if (keyInfo.Key == ConsoleKey.Escape) break;

            } while (keyInfo.Key != ConsoleKey.Enter);

            return pass;
        }
    }
    public class RunAsync
    {
        // BACKGROUND DATE CHECKING METHODS:
            // CUSTOMER SUBSCRIPTION CHECKER!
            // RESERVATION/ACTIVITIES REMOVER!
            // RESERVABLE AVAILABILITY CHECKER!
            // ...
    }
    public class Additions
    {
        public static void ClearLines(int lines)
        {
            int run = 0;
            int y = Console.CursorTop;
            while (run < lines && Console.CursorTop > 0)
            {
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.Write(new string(' ', Console.BufferWidth));
                Console.SetCursorPosition(0, Console.CursorTop);
                run++;
            }
        }
    }
}
