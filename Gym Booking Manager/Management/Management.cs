using Gym_Booking_Manager.Users;

namespace Gym_Booking_Manager.ManagementFunctions
{
    public class Management
    {
        public static int UserLogin()   // NYI: SYS. ADMIN LOGIN!
        {
            int id = -1;
            int tries = 3;
            string? loginName = string.Empty;

            try
            {
                Console.Clear();
                Console.WriteLine("<< LOG-IN >>\n");
                while (id == -1)
                {
                    Console.Write(">> Enter username: ");
                    loginName = Console.ReadLine();

                    foreach (User user in User.users)
                    {
                        if (loginName == user.loginName)
                        {
                            id = user.id;
                        }
                    }
                    if (id == -1) Console.WriteLine(">> Username does not exist!");
                    else Console.Clear();
                }
                Program.logger.LogActivity($"INFO: Login() - Username entry successful. USER: {loginName}");
            }
            catch { Program.logger.LogActivity($"ERROR: Login() - Username entry unsuccessful. USER: {loginName}"); }

            try
            {
                Console.WriteLine("<< LOG-IN >>\n");
                Console.WriteLine($">> Username: {loginName}");
                while (true)
                {
                    User? user = User.users.Find(u => u.id == id);
                    Console.Write(">> Enter password: ");
                    string loginPass = MaskPassword();

                    if (loginPass == user.loginPass)
                    {
                        Console.WriteLine($"\n>> Welcome {user.firstName} {user.lastName}!");
                        Task.Delay(1000).Wait();
                        break;
                    }
                    else
                    {
                        tries--;
                        Console.WriteLine("\n>> Incorrect password, " + tries + " tries left.");
                    }

                    if (tries == 0)
                    {
                        Console.WriteLine("\n>> Maximum tries reached, contact staff for support.");
                        Task.Delay(1000).Wait();
                        return -1;
                    }
                }
                Program.logger.LogActivity($"INFO: Login() - Password entry successful. USER: {loginName}");
            }
            catch { Program.logger.LogActivity($"ERROR: Login() - Password entry unsuccessful. USER: {loginName}"); }

            return id;
        }
        public static bool CheckLoginNames(string loginName)
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
