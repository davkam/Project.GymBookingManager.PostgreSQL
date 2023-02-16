using Gym_Booking_Manager.Reservations;
using Gym_Booking_Manager.Users;

namespace Gym_Booking_Manager.Reservables
{
    public class Reservable
    {
        public static int getReservableID;
        public static List<Reservable> reservables = new List<Reservable>();

        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public static List<Reservation> reservations { get; set; }
        public Reservable(int id, string name, string description)
        {
            this.id = id;
            this.name = name;
            this.description = description;
        }
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
                        var equipment = new Equipment(int.Parse(strings[1]), strings[2], strings[3], bool.Parse(strings[4]));
                        reservables.Add(equipment);
                    }
                    if (strings[0] == "Space")
                    {
                        var space = new Space(int.Parse(strings[1]), strings[2], strings[3], int.Parse(strings[4]));
                        reservables.Add(space);
                    }
                    if (strings[0] == "PTrainer")
                    {
                        Staff staff = (Staff)User.users.Find(u => u.id == int.Parse(strings[2]));
                        var ptrainer = new PTrainer(int.Parse(strings[1]), staff);
                        reservables.Add(ptrainer);
                    }
                }
                Program.logger.LogActivity("INFO: LoadReservables() - Read data (\"Reservations/Reservables.txt\") successful.");
            }
            catch { Program.logger.LogActivity("ERROR: LoadReservables() - Read data (\"Reservations/Reservables.txt\") unsuccessful."); }
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
                            writer.WriteLine($"Equipment;{equipment.id};{equipment.name};{equipment.description};{equipment.bookable}");
                        }
                        if (reservables[i] is Space)
                        {
                            Space space = (Space)reservables[i];
                            writer.WriteLine($"Space;{space.id};{space.name};{space.description};{space.capacity}");
                        }
                        if (reservables[i] is PTrainer)
                        {
                            PTrainer ptrainer = (PTrainer)reservables[i];
                            writer.WriteLine($"PTrainer;{ptrainer.id};{ptrainer.name};{ptrainer.description};{ptrainer.instructor.id}");
                        }
                    }
                }
                Program.logger.LogActivity("INFO: SaveReservables() - Write data (\"Reservations/Reservables.txt\") successful.");
            }
            catch { Program.logger.LogActivity("ERROR: SaveReservables() - Write data (\"Reservations/Reservables.txt\") unsuccessful."); }
        }
        private static int GetReservableID()
        {
            int id = getReservableID;
            getReservableID++;
            return id;
        }
        public static void NewReservable()
        {
            bool go = true;
            while (go == true)
            {
                Console.WriteLine("Write 1 to registrer Equipment, 2 to register space, 3 to register PT PT, 4 to Quit");
                string input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        NewEquipment();
                        break;
                    case "2":
                        NewSpace();
                        break;
                    case "3":
                        NewPT();
                        break;
                    case "4":
                        go = false;
                        break;
                    default:
                        Console.WriteLine("Incorrect input!");
                        break;
                }
            }
        }
        public static void NewEquipment()
        {
            string[] input = new string[3];
            Console.Write("Write the space name: ");
            input[0] = Console.ReadLine();
            Console.Write("Write the space description: ");
            input[1] = Console.ReadLine();
            Console.Write("Should non-members be able to book this? (yes or no)");
            input[2] = Console.ReadLine();
            if (input[2] == "Yes" || input[2] == "yes" || input[2] == "YES") input[2] = "true";
            else input[2] = "false";
            Console.WriteLine();
            Console.WriteLine("Do you want to save:" + input[0] + " " + input[1] + " write yes if so");
            string spara = Console.ReadLine();
            if (spara == "ja" || spara == "Ja" || spara == "JA")
            {
                int ID = GetReservableID();
                reservables.Add(new Equipment(ID, input[0], input[1], bool.Parse(input[2])));
                SaveReservables();
            }
        }
        public static void NewSpace()
        {
            string[] input = new string[3];
            Console.Write("Write the space name: ");
            input[0] = Console.ReadLine();
            Console.Write("Write the space description: ");
            input[1] = Console.ReadLine();
            while (true)
            {
                Console.Write("Write the space capacity: ");
                input[2] = Console.ReadLine();
                int.TryParse(input[2], out int no);
                if (no > 0) break;
                else Console.WriteLine("Incorrect input!");
            }
            Console.WriteLine();
            Console.WriteLine("Do you want to save:" + input[0] + " " + input[1] + " " + input[2] + " write yes if so");
            string spara = Console.ReadLine();
            if (spara == "yes" || spara == "Yes" || spara == "YES")
            {
                int ID = GetReservableID();
                reservables.Add(new Space(ID, input[0], input[1], int.Parse(input[2])));
                SaveReservables();
            }
        }
        public static void NewPT(Staff staff = null)
        {
            if (staff != null)
            {
                int id = GetReservableID();
                var ptrainer = new PTrainer(id, staff);

                reservables.Add(ptrainer);
                SaveReservables();
            }
            else
            {
                Console.Write("Write ID of PT: ");
                int trainerID = int.Parse(Console.ReadLine());
                Console.WriteLine();
                Console.WriteLine("Do you want to save " + trainerID + "? write yes if so");
                string spara = Console.ReadLine();
                if (spara == "yes" || spara == "Yes" || spara == "YES")
                {
                    Staff ptrainer = (Staff)User.users[trainerID];
                    int ID = GetReservableID();
                    reservables.Add(new PTrainer(ID, ptrainer));
                    SaveReservables();
                }
            }
        }
        public static void UpdateReservable()
        {
            // Staff updates existing reservables.
        }
        public void DeleteReservable()
        {
            // Staff deletes existing reservables.
        }
    }
    public class Equipment : Reservable
    {
        public bool bookable { get; set; }
        public Equipment(int id, string name, string description, bool bookable)
            : base(id, name, description)
        {
            this.bookable = bookable;
        }
    }
    public class Space : Reservable
    {
        public int capacity { get; set; }
        public Space(int id, string name, string description, int capacity)
            : base(id, name, description)
        {
            this.capacity = capacity;
        }
    }
    public class PTrainer : Reservable
    {
        public Staff instructor { get; set; }
        public PTrainer(int id, Staff PTrainer)
            : base(id, $"{PTrainer.firstName} {PTrainer.lastName}", $"{PTrainer.phone}, {PTrainer.email}")
        {
            this.instructor = PTrainer;
        }
    }
}
