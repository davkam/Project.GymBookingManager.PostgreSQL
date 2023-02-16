using Gym_Booking_Manager.Reservations;
using Gym_Booking_Manager.Users;
using System.Diagnostics;
using System.Globalization;
using System.Transactions;

namespace Gym_Booking_Manager.Reservables
{
    public class Reservable
    {
        public static int getReservableID;
        public static List<Reservable> reservables = new List<Reservable>();

        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public bool isAvailable { get; set; }
        public List<Reservation> reservations { get; set; } // NYI!
        public Reservable(int id, string name, string description, bool isAvailable)
        {
            this.id = id;
            this.name = name;
            this.description = description;
            this.isAvailable = isAvailable;
            reservations = new List<Reservation>();
        }
        public Reservable() { }
        public static void LoadReservables()
        {
            try
            {
                string[] lines = File.ReadAllLines("Functionalities/Reservables/Reservables.txt");
                getReservableID = int.Parse(lines[0]);

                for (int i = 1; i < lines.Length; i++)
                {
                    string[] strings = lines[i].Split(";");
                    if (strings[0] == "Equipment")
                    {
                        var equipment = new Equipment(int.Parse(strings[1]), strings[2], strings[3], bool.Parse(strings[4]), bool.Parse(strings[5]));
                        reservables.Add(equipment);
                    }
                    if (strings[0] == "Space")
                    {
                        var space = new Space(int.Parse(strings[1]), strings[2], strings[3], bool.Parse(strings[4]), int.Parse(strings[5]));
                        reservables.Add(space);
                    }
                    if (strings[0] == "PTrainer")
                    {
                        Staff staff = (Staff)User.users.Find(u => u.id == int.Parse(strings[3]));
                        var ptrainer = new PTrainer(int.Parse(strings[1]), bool.Parse(strings[2]), staff);
                        reservables.Add(ptrainer);
                    }
                }
                Program.logger.LogActivity("INFO: LoadReservables() - Read data (\"Functionalities/Reservables/Reservables.txt\") successful.");
            }
            catch { Program.logger.LogActivity("ERROR: LoadReservables() - Read data (\"Functionalities/Reservables/Reservables.txt\") unsuccessful."); }
        }
        public static void SaveReservables()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter("Functionalities/Reservables/Reservables.txt", false))
                {
                    writer.WriteLine(getReservableID);
                    for (int i = 0; i < reservables.Count; i++)
                    {
                        if (reservables[i] is Equipment)
                        {
                            Equipment equipment = (Equipment)reservables[i];
                            writer.WriteLine($"Equipment;{equipment.id};{equipment.name};{equipment.description};{equipment.isAvailable};{equipment.membersOnly}");
                        }
                        if (reservables[i] is Space)
                        {
                            Space space = (Space)reservables[i];
                            writer.WriteLine($"Space;{space.id};{space.name};{space.description};{space.isAvailable};{space.capacity}");
                        }
                        if (reservables[i] is PTrainer)
                        {
                            PTrainer ptrainer = (PTrainer)reservables[i];
                            writer.WriteLine($"PTrainer;{ptrainer.id};{ptrainer.isAvailable};{ptrainer.instructor.id}");
                        }
                    }
                }
                Program.logger.LogActivity("INFO: SaveReservables() - Write data (\"Functionalities/Reservables/Reservables.txt\") successful.");
            }
            catch { Program.logger.LogActivity("ERROR: SaveReservables() - Write data (\"Functionalities/Reservables/Reservables.txt\") unsuccessful."); }
        }
        private static int GetReservableID()
        {
            int id = getReservableID;
            getReservableID++;
            return id;
        }
        public static void NewReservable(Staff staff)   // NYI: LOG ACTIVITY!
        {
            bool run = true;
            while (run == true)
            {
                Console.Clear();
                Console.WriteLine("<< NEW RESERVABLE >>\n");
                Console.WriteLine(">> Select an option!");
                Console.WriteLine("- [1]   New equipment.");
                Console.WriteLine("- [2]   New space.");
                Console.WriteLine("- [3]   New personal trainer.");
                Console.WriteLine("- [ESC] Exit.");

                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.D1 || keyInfo.Key == ConsoleKey.NumPad1)
                {
                    NewEquipment();
                    run = false;
                }
                else if (keyInfo.Key == ConsoleKey.D2 || keyInfo.Key == ConsoleKey.NumPad2)
                {
                    NewSpace();
                    run = false;
                }
                else if (keyInfo.Key == ConsoleKey.D3 || keyInfo.Key == ConsoleKey.NumPad3)
                {
                    NewPT();
                    run = false;
                }
                else if (keyInfo.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine(">> New reservable cancelled!");
                    Task.Delay(1000).Wait();
                    return;
                }
                else
                {
                    Console.WriteLine($">> INVALID KEY: [KEY.{keyInfo.Key}]");
                    Task.Delay(1000).Wait();
                    return;
                }
            }
        }
        private static void NewEquipment()
        {
            ConsoleKeyInfo keyInfo;

            int id = GetReservableID();
            Console.Write("\n>> Enter new equipment name: ");
            string? name = Console.ReadLine();
            Console.Write(">> Enter new equipment description: ");
            string? description = Console.ReadLine();
            Console.WriteLine(">> New equipment available? Yes[Y] or No[N]");
            keyInfo = Console.ReadKey(true);

            bool isAvailable = true;
            switch (keyInfo.Key)
            {
                case ConsoleKey.Y: isAvailable = true; break;
                case ConsoleKey.N: isAvailable = false; break;
                default: Console.WriteLine(">> New equipment set to available by default."); break;
            }

            Console.WriteLine(">> Members only equipment? Yes[Y] or No[N]");
            keyInfo = Console.ReadKey(true);

            bool membersOnly = true;
            switch (keyInfo.Key)
            {
                case ConsoleKey.Y: membersOnly = true; break;
                case ConsoleKey.N: membersOnly = false; break;
                default: Console.WriteLine(">> New equipment set to members only by default."); break;
            }

            var newEquipment = new Equipment(id, name, description, isAvailable, membersOnly);
            Console.WriteLine("\n>> New equipment:");
            Console.WriteLine($"- ID: {id}");
            Console.WriteLine($"- NAME: {name}");
            Console.WriteLine($"- AVAILABILITY: {isAvailable}");
            Console.WriteLine($"- MEMBERS ONLY: {membersOnly}");
            Console.WriteLine($"- DESCRIPTION: {description}");
            Console.WriteLine("\n>> Press any key to add new equipment, or [ESC] to cancel.");
            keyInfo = Console.ReadKey(true);
            if (keyInfo.Key == ConsoleKey.Escape)
            {
                Console.WriteLine(">> New space registration cancelled!");
                Task.Delay(1500).Wait();
                return;
            }

            reservables.Add(newEquipment);
            SaveReservables();

            Console.WriteLine($">> New equipment ({name}) successfully added to reservables!");
            Task.Delay(1500).Wait();
        }
        private static void NewSpace()
        {
            ConsoleKeyInfo keyInfo;

            int id = GetReservableID();
            Console.Write("\n>> Enter new space name: ");
            string? name = Console.ReadLine();
            Console.Write(">> Enter new space description: ");
            string? description = Console.ReadLine();
            Console.WriteLine(">> New space available? Yes[Y] or No[N]");
            keyInfo = Console.ReadKey(true);

            bool isAvailable = true;
            switch (keyInfo.Key)
            {
                case ConsoleKey.Y: isAvailable = true; break;
                case ConsoleKey.N: isAvailable = false; break;
                default: Console.WriteLine(">> New space set to available by default."); break;
            }
            Console.Write(">> Enter new space capacity: ");
            int capacity = 0;
            try { capacity = int.Parse(Console.ReadLine()); }
            catch { Console.WriteLine(">> Invalid format, new space capacity set to 0 by default."); }

            var newSpace = new Space(id, name, description, isAvailable, capacity);
            Console.WriteLine("\n>> New equipment:");
            Console.WriteLine($"- ID: {id}");
            Console.WriteLine($"- NAME: {name}");
            Console.WriteLine($"- AVAILABILITY: {isAvailable}");
            Console.WriteLine($"- CAPACITY: {capacity}");
            Console.WriteLine($"- DESCRIPTION: {description}");
            Console.WriteLine("\n>> Press any key to add new space, or [ESC] to cancel.");
            keyInfo = Console.ReadKey(true);
            if (keyInfo.Key == ConsoleKey.Escape)
            {
                Console.WriteLine(">> New space registration cancelled!");
                Task.Delay(1500).Wait();
                return;
            }

            reservables.Add(newSpace);
            SaveReservables();

            Console.WriteLine($">> New space ({name}) successfully added to reservables!");
            Task.Delay(1500).Wait();
        }
        public static void NewPT(Staff? staff = null, bool footer = true)
        {
            ConsoleKeyInfo keyInfo;
            Staff staffToPT;

            if (staff == null)
            {
                int x, y;
                Console.WriteLine();
                (x, y) = Console.GetCursorPosition();

                foreach (User u in User.users.OfType<Staff>())
                {
                    if (x >= Console.BufferWidth)
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
                }
                Console.Write("\n>> Enter id of staff to add as a personal trainer: ");
                int input;
                try 
                { 
                    input = int.Parse(Console.ReadLine());
                    staffToPT = (Staff)User.users.Find(u => u.id == input);
                }
                catch
                {
                    Console.WriteLine(">> Invalid format or out of range, new personal trainer registration cancelled!");
                    Task.Delay(1500).Wait();
                    return;
                }
                if (CheckStaffInPTs(staffToPT))
                {
                    Console.WriteLine(">> Staff already registered as a personal trainer.");
                    Console.WriteLine(">> New personal trainer registration cancelled!");
                    Task.Delay(1500).Wait();
                    return;
                }
            }
            else { staffToPT = staff; }

            int id = GetReservableID();
            bool isAvailable = true;
            var newPT = new PTrainer(id, isAvailable, staffToPT);

            if (footer)
            {
                Console.WriteLine(">> New personal trainer available? Yes[Y] or No[N]");
                keyInfo = Console.ReadKey(true);

                switch (keyInfo.Key)
                {
                    case ConsoleKey.Y: isAvailable = true; break;
                    case ConsoleKey.N: isAvailable = false; break;
                    default: Console.WriteLine(">> New personal trainer set to available by default."); break;
                }

                Console.WriteLine("\n>> New personal trainer:");
                Console.WriteLine($"- ID: {newPT.id}");
                Console.WriteLine($"- NAME: {newPT.name}");
                Console.WriteLine($"- AVAILABILITY: {newPT.isAvailable}");
                Console.WriteLine($"- DESCRIPTION: {newPT.description}");
                Console.WriteLine("\n>> Press any key to add new personal trainer, or [ESC] to cancel.");
                keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine(">> New personal trainer registration cancelled!");
                    Task.Delay(1500).Wait();
                    return;
                }
            }
            reservables.Add(newPT);
            SaveReservables();
        }
        private static bool CheckStaffInPTs(Staff staff)
        {
            foreach (Reservable r in reservables.OfType<PTrainer>())
            {
                PTrainer? pt = (PTrainer)r;
                if (pt.instructor == staff) return true;
            }
            return false;
        }
        public static void DeleteReservable(Staff staff)    // NYI: LOG ACTIVITY!
        {
            Console.Clear();
            Console.WriteLine("<< DELETE RESERVABLE >>\n");

            ViewReservables(false, false);

            int input = -1;
            Console.Write("\n>> Enter ID of the reservable to delete: ");
            try { input = int.Parse(Console.ReadLine()); }
            catch { /* NYI: ADD LOGGER */ }

            int index = -1;
            for (int i = 0; i < reservables.Count; i++)
            {
                if (reservables[i].id == input && input != -1)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1 && !CheckReservableInReservations(reservables[index]))
            {
                Console.WriteLine($"\n>> Press any key to delete ({reservables[index].name}), or [ESC] to cancel.");
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine(">> Delete reservable cancelled!");
                    Task.Delay(1500).Wait();
                    return;
                }
                reservables.RemoveAt(index);
                Console.WriteLine(">> Reservable successfully deleted.");
                SaveReservables();
            }
            else Console.WriteLine(">> Could not find reservable or reservable is used in a reservation, try again!");
            Task.Delay(1500).Wait();
        }
        private static bool CheckReservableInReservations(Reservable reservable)
        {
            foreach (Reservation r in Reservation.reservations)
            {
                if (r.reservables.Contains(reservable)) return true;
            }
            return false;
        }
        public static void EditReservable(Staff staff)      // NYI: LOG ACTIVITY!
        {
            // Staff updates existing reservables.
        }
        public static void ViewReservables(bool header = true, bool footer = true)
        {
            reservables.Sort((x, y) => x.GetType().Name.CompareTo(y.GetType().Name));

            string typeReservable = "Equipment";
            int x, y;
            int addY = 0;

            if (header)
            {
                Console.Clear();
                Console.WriteLine("<< VIEW RESERVABLES >>\n");
            }

            (x, y) = Console.GetCursorPosition();
            foreach (Reservable rsvb in reservables)
            {
                if (typeReservable != rsvb.GetType().Name || x >= 120)
                {
                    x = 0;
                    y += 8;
                }

                Console.SetCursorPosition(x, y);
                Console.Write($"- TYPE:          {rsvb.GetType().Name}");
                Console.SetCursorPosition(x, y + 1);
                Console.Write($"- ID:            {rsvb.id}");
                Console.SetCursorPosition(x, y + 2);
                Console.Write($"- NAME:          {rsvb.name}");
                Console.SetCursorPosition(x, y + 3);
                Console.Write($"- AVAILABILITY:  {rsvb.isAvailable}");
                if (rsvb.GetType().Name == "Equipment")
                {
                    Equipment equipment = (Equipment)rsvb;
                    Console.SetCursorPosition(x, y + 4);
                    Console.Write($"- MEMBERS ONLY:  {equipment.membersOnly}");
                    addY++;
                }
                Console.SetCursorPosition(x, y + 4 + addY);
                Console.Write($"- DESCRIPTION:");
                Console.SetCursorPosition(x, y + 5 + addY);
                Console.Write($"  \"{rsvb.description}\"");
                addY = 0;


                x += 40;
                typeReservable = rsvb.GetType().Name;
            }
            Console.WriteLine();

            if (footer)
            {
                int id = -1;
                Console.WriteLine("\n>> Press any key to continue, or [V] to view a reservable.");
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.V)
                {
                    try
                    {
                        Console.Write(">> Enter ID of reservable to view: ");
                        id = int.Parse(Console.ReadLine());
                    }
                    catch { return; }
                }
                if (id != -1) ViewReservable(id);
            }
        }
        private static void ViewReservable(int id)
        {
            bool found = false;
            int x, y;

            Console.Clear();
            Console.WriteLine("<< VIEW RESERVABLE >>\n");
            foreach (Reservable rsvb in reservables)
            {
                if (rsvb.id == id)
                {
                    found = true;
                    Console.WriteLine($"- TYPE:         {rsvb.GetType().Name}");
                    Console.WriteLine($"- ID:           {rsvb.id}");
                    Console.WriteLine($"- NAME:         {rsvb.name}");
                    Console.WriteLine($"- AVAILABILITY: {rsvb.isAvailable}");
                    if (rsvb.GetType().Name == "Equipment")
                    {
                        Equipment equipment = (Equipment)rsvb;
                        Console.WriteLine($"- MEMBERS ONLY: {equipment.membersOnly}");
                    }
                    Console.WriteLine($"- DESCRIPTION:  \"{rsvb.description}\"");
                    Console.WriteLine($"- RESERVED IN:  (NYI)");
                    (x, y) = Console.GetCursorPosition();

                    foreach (Reservation rsv in rsvb.reservations)
                    {
                        if (x > Console.BufferWidth)
                        {
                            x = 0;
                            y++;
                        }
                        Console.SetCursorPosition(x, y);
                        Console.Write($"  .ID:         {rsv.id}");
                        Console.SetCursorPosition(x, y + 1);
                        Console.Write($"  .DATE(FROM): {rsv.date.timeFrom}");
                        Console.SetCursorPosition(x, y + 2);
                        Console.Write($"  .DATE(TO):   {rsv.date.timeTo}");

                        x += 40;
                    }
                }
            }
            if (!found) Console.WriteLine(">> Could not find reservable, try again!");
            Console.WriteLine("\n>> Press any key to continue.");
            Console.ReadKey(true);
        }
    }
    public class Equipment : Reservable
    {
        public bool membersOnly { get; set; }
        public Equipment(int id, string name, string description, bool isAvailable, bool membersOnly)
            : base(id, name, description, isAvailable)
        {
            this.membersOnly = membersOnly;
        }
    }
    public class Space : Reservable
    {
        public int capacity { get; set; }
        public Space(int id, string name, string description, bool isAvailable, int capacity)
            : base(id, name, description, isAvailable)
        {
            this.capacity = capacity;
        }
    }
    public class PTrainer : Reservable
    {
        public Staff instructor { get; set; }
        public PTrainer(int id, bool isAvailable, Staff PTrainer)
            : base(id, $"{PTrainer.firstName} {PTrainer.lastName}", $"{PTrainer.phone}, {PTrainer.email}", isAvailable)
        {
            this.instructor = PTrainer;
        }
        public PTrainer() : base() { }
    }
}
