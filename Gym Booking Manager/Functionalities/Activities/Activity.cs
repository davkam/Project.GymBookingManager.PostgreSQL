using Gym_Booking_Manager.ActivityExtenstion;
using Gym_Booking_Manager.Dates;
using Gym_Booking_Manager.Reservables;
using Gym_Booking_Manager.Reservations;
using Gym_Booking_Manager.Users;
using System.Globalization;

namespace Gym_Booking_Manager.Activities
{
    public class Activity
    {
        public static int getActivityID;
        public static List<Activity>? activities = new List<Activity>();

        public int id { get; set; }
        public string? name { get; set; }
        public string? description { get; set; }
        public bool open { get; set; }
        public int limit { get; set; }
        public Staff? instructor { get; set; }
        public Date? date { get; set; }
        public Reservation? reservation { get; set; }
        public List<Customer>? participants { get; set; }

        public Activity(int id, string name, string description, bool open, int limit,
            Staff instructor, Date date, Reservation reservation, List<Customer>? participants = default(List<Customer>))
        {
            this.id = id;
            this.name = name;
            this.description = description;
            this.open = open;
            this.limit = limit;
            this.instructor = instructor;
            this.date = date;
            this.reservation = reservation;
            this.participants = participants;
        }
        public Activity() { }
        public static void LoadActivities()
        {
            try
            {
                string[] lines = File.ReadAllLines("Functionalities/Activities/Activities.txt");
                getActivityID = int.Parse(lines[0]);

                for (int i = 1; i < lines.Length; i++)
                {
                    string[] stringsA = lines[i].Split(";");
                    string[] stringsB = stringsA[9].Split(",");

                    var participants = new List<Customer>();

                    if (stringsB.Length > 0)
                    {
                        foreach (string strB in stringsB)
                        {
                            participants.Add((Customer)User.users.Find(user => user.id == int.Parse(strB)));
                        }
                    }
                    var staff = (Staff)User.users.Find(u => u.id == int.Parse(stringsA[5]));
                    var schedule = new Date(DateTime.Parse(stringsA[6]), DateTime.Parse(stringsA[7]));
                    var reservation = Reservation.reservations.Find(r => r.id == int.Parse(stringsA[8]));
                    var activity = new Activity(int.Parse(stringsA[0]), stringsA[1], stringsA[2], bool.Parse(stringsA[3]), int.Parse(stringsA[4]), staff, schedule, reservation, participants);
                    activities.Add(activity);
                }
                Program.logger.LogActivity("INFO: LoadActivities() - Read data (\"Activities/Activities.txt\") successful.");
            }
            catch { Program.logger.LogActivity("ERROR: LoadActivities() - Read data (\"Activities/Activities.txt\") unsuccessful."); }
        }
        public static void SaveActivities()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter("Functionalities/Activities/Activities.txt", false))
                {
                    writer.WriteLine(getActivityID);
                    foreach (Activity activity in activities)
                    {
                        string participants = string.Empty;
                        if (activity.participants.Count > 0)
                        {
                            foreach (Customer customer in activity.participants)
                            {
                                participants += $"{customer.id},";
                            }
                        }
                        participants = participants[0..^1];
                        writer.WriteLine($"{activity.id};{activity.name};{activity.description};{activity.open};{activity.limit};{activity.instructor.id};{activity.date.timeFrom};" +
                            $"{activity.date.timeTo};{activity.reservation.id};{participants}");
                    }
                }
                Program.logger.LogActivity("INFO: SaveActivities() - Write data (\"Activities/Activities.txt\") successful.");
            }
            catch { Program.logger.LogActivity("ERROR: SaveActivities() - Write data (\"Activities/Activities.txt\") unsuccessful."); }
        }
        private static int GetActivityID()
        {
            int id = getActivityID;
            getActivityID++;
            return id;    
        }
        static void ChooseReservables(List<int> reservableToList)
        {
            int PTcheck = 0;
            while (true)
            {
                Console.WriteLine("Available Equipment/PT: ");
                for (int i = 0; i < reservableToList.Count(); i++)
                {
                    Console.WriteLine($"{i + 1} {Reservable.reservables[reservableToList[i]].name}");
                }
                Console.WriteLine("Type in which Equipment/PT you wish to reserve (number) type 0 to exit when done with selections (you must pick at least on PT): ");
                string input = Console.ReadLine();
                bool isNumber;
                isNumber = int.TryParse(input, out int number);
                if (isNumber && number > 0 && number < reservableToList.Count() + 1)
                {
                    if (Reservable.reservables[reservableToList[number - 1]] is PTrainer) PTcheck++;
                    Reservation.reservations[Reservation.reservations.Count() - 1].reservables.Add(Reservable.reservables[reservableToList[number - 1]]);
                    Console.WriteLine("You have booked " + Reservable.reservables[reservableToList[number - 1]].name);
                    reservableToList.RemoveAt(number - 1);
                }
                else if (number == 0 || PTcheck > 0) break;
                else if (number == 0) Console.WriteLine("You must select at least one PT!");
                else
                {
                    Console.WriteLine("Incorrect input!");
                }
            }
            Reservation.SaveReservations();
        }
        public static void NewActivity(Staff staff) // TBD: Parameter to Staff!
        {
            int id = GetActivityID(); string Name; string Description; bool Open = true; int participantsNo = 0; Staff Instructor = staff; List<Customer>? Participants=new List<Customer>();
            bool overlap = false;
            List<int> reservableToList = new List<int>();
            DateTime[] date = new DateTime[2];
            Date.DateSelecter(date);
            for (int i = 0; i < Reservable.reservables.Count(); i++)
            {
                overlap = false;
                for (int j = 0; j < Reservation.reservations.Count(); j++)
                {
                    for (int k = 0; k < Reservation.reservations[j].reservables.Count(); k++)
                    {
                        if (i == Reservation.reservations[j].reservables[k].id) overlap = date[0] < Reservation.reservations[j].date.timeTo && Reservation.reservations[j].date.timeFrom < date[1];
                        if (overlap) break;
                    }
                    if (overlap) break;
                }
                if (!overlap && Reservable.reservables[i] is Space) reservableToList.Add(Reservable.reservables[i].id);
            }
            ChooseSpace(staff, date, reservableToList);
            reservableToList.Clear();
            for (int i = 0; i < Reservable.reservables.Count(); i++)
            {
                overlap = false;
                for (int j = 0; j < Reservation.reservations.Count(); j++)
                {
                    for (int k = 0; k < Reservation.reservations[j].reservables.Count(); k++)
                    {
                        if (i == Reservation.reservations[j].reservables[k].id) overlap = date[0] < Reservation.reservations[j].date.timeTo && Reservation.reservations[j].date.timeFrom < date[1];
                        if (overlap) break;
                    }
                    if (overlap) break;
                }
                if (!overlap && Reservable.reservables[i] is not Space) reservableToList.Add(Reservable.reservables[i].id);
            }
            ChooseReservables(reservableToList);
            Console.Write("Enter name of activity: ");
            Name = Console.ReadLine();
            Console.WriteLine();
            Console.Write("Enter description of activity: ");
            Description = Console.ReadLine();
            Console.WriteLine();
            int Limit=0;
            for(int i=0; i<Reservation.reservations[Reservation.reservations.Count() - 1].reservables.Count(); i++)
            {
                if (Reservation.reservations[Reservation.reservations.Count() - 1].reservables[i] is Space)
                {
                    Space room = (Space)Reservation.reservations[Reservation.reservations.Count() - 1].reservables[i];
                    Limit = room.capacity;
                }
            }
            while (true)
            {            
            Console.Write($"Enter participant limit(max={Limit}): ");
            Console.WriteLine();
            string? input = Console.ReadLine();
            int.TryParse( input, out participantsNo);
            if(participantsNo > 0 && participantsNo<= Limit)
                {
                    break;
                }
            else Console.WriteLine("Incorrect input!");
            }
            Reservation Reservationn = Reservation.reservations[Reservation.reservations.Count() - 1];
            Date datee = new Date(date[0], date[1]); 
            Activity.activities.Add(new Activity(id, Name, Description, Open, participantsNo, Instructor, datee, Reservationn, Participants));
            SaveActivities();
        }
        static void ChooseSpace(Staff staff, DateTime[] date, List<int> reservableToList) // TBD: Parameter to Staff!
        {
            List<Reservable> list = new List<Reservable>();
            Console.WriteLine("Available Spaces: ");
            for (int i = 0; i < reservableToList.Count(); i++)
            {
                Console.WriteLine($"{i + 1} {Reservable.reservables[reservableToList[i]].name}");
            }

            while (true)
            {
                Console.WriteLine("Type in which space you wish to reserve (number): ");
                string input = Console.ReadLine();
                bool isNumber;
                isNumber = int.TryParse(input, out int number);
                if (isNumber && number > 0 && number < reservableToList.Count() + 1)
                {
                    list.Add(Reservable.reservables[reservableToList[number - 1]]);
                    Console.WriteLine("You have booked " + Reservable.reservables[reservableToList[number - 1]].name);
                    Reservation.reservations.Add(new Reservation(Reservation.GetReservationID(), staff, new Date(date[0], date[1]), list));
                    Reservation.SaveReservations();
                    break;
                }
                else
                {
                    Console.WriteLine("Incorrect input!");
                }
            }
        }
        public void EditActivity()  // LOW PRIO!
        {
            // Staff edits activites.
        }
        public static void DeleteActivity(Staff staff) // TBD: Add Staff parameter.
        {
            int i = 0;
            Console.WriteLine("Which activity do you want to delete? (0 to exit)");
            foreach(Activity a in activities)
            {
                i++;
                Console.WriteLine(i+" "+a.name+" date/time "+a.date.timeFrom);
            }
            string input=Console.ReadLine();
            int.TryParse(input, out int number);
            if (number > 0 && number < activities.Count())
            {
                Console.WriteLine("You have deleted " + activities[number - 1].name);
                activities.RemoveAt(number - 1);
                SaveActivities();
            }
            else Console.WriteLine("Incorrect input");
            Thread.Sleep(2000);
        }
        public static void RegisterActivity(Customer customer)
        {
            Console.Clear();
            Console.WriteLine("<< ACTIVITY REGISTRATION >>\n");

            ActivityExt.ActivityCalendarMenu(false);
            
            Console.Write("\n>> Enter ID of activity to register: ");
            string? input = Console.ReadLine();
            int.TryParse(input, out var result);

            var getActivity = new Activity();
            foreach (Activity a in activities)
            {
                if (a.id == result && a.limit > a.participants.Count() && customer.subEnd > a.date.timeFrom && a.participants.Contains(customer) == false)
                {
                    Console.WriteLine($">> Register for activity \"{a.name}\"?");
                    Console.WriteLine(">> Press any key to continue or [ESC] to exit.");
                    ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                    if (keyInfo.Key == ConsoleKey.Escape)
                    {
                        Console.WriteLine(">> Activity registration cancelled!");
                        Task.Delay(1500).Wait();
                        return;
                    }
                    a.participants.Add(customer);
                    Console.WriteLine(">> Activity registration successful!");
                }
                else if (a.id == result && a.limit <= a.participants.Count() && customer.subEnd > a.date.timeFrom && a.participants.Contains(customer) == false)
                {
                    Console.WriteLine(">> Activity full, registration cancelled!");
                }
                else if (a.id == result && a.participants.Contains(customer) == true)
                {
                    Console.WriteLine(">> Activity already registered, registration cancelled!");
                }
                else if (a.id == result && customer.subEnd < a.date.timeFrom)
                {
                    Console.WriteLine(">> Activity date is later than subscription, registration cancelled!");
                }
            }
            SaveActivities();
            Task.Delay(1500).Wait();
        }
        public static void DeregisterActivity(Customer customer)   // CHECK!!
        {
            int result;
            int index = 1;
            List<int>? registeredActivityIDs = new();
            Activity? deregisterActivity = new();

            Console.Clear();
            Console.WriteLine("<< ACTIVITY DEREGISTRATION >>\n");
            Console.WriteLine($">> Registered activities: {customer.firstName} {customer.firstName}\n");
            
            foreach (Activity a in activities)
            {
                if (a.participants.Contains(customer))
                {
                    Console.WriteLine($"- {index}. ACTIVITY: {a.name}  DATE: {a.date.timeFrom}");
                    registeredActivityIDs.Add(a.id);
                    index++;
                }
            }
            Console.Write("\n>> Enter number of activity to deregister (\"0\" to exit): ");
            string? input = Console.ReadLine();
            int.TryParse(input, out result);
            deregisterActivity = activities.Find(a => a.id == registeredActivityIDs[result - 1]);

            if (result == 0) Console.WriteLine(">> Activity deregistration cancelled!");
            else if (result > 0 && result < registeredActivityIDs.Count())
            {
                deregisterActivity.participants.Remove(customer);

                Console.WriteLine($">> Activity {deregisterActivity.name} successfully deregistered!");
                SaveActivities();
            }
            else Console.WriteLine(">> Invalid input, activity deregistration cancelled!");
            Task.Delay(1500).Wait();
        }
        public static void ViewRegisteredActivities(Customer customer) // CHECK!!
        {
            Console.Clear();
            Console.WriteLine("<< REGISTERED ACTIVITIES >>\n");
            Console.WriteLine($">> {customer.firstName} {customer.lastName}'s registered activities:");
            
            foreach (Activity a in activities)
            {
                if (a.participants.Contains(customer))
                {
                    Console.WriteLine($"\n- ID:           {a.id}");
                    Console.WriteLine($"- NAME:         {a.name}");
                    Console.WriteLine($"- DESCRIPTION:  {a.description}");
                    Console.WriteLine($"- INSTRUCTOR:   {a.instructor.firstName} {a.instructor.lastName}");
                    Console.WriteLine($"- PARTICIPANTS: {a.participants.Count()}({a.limit})");
                    Console.WriteLine($"- DATE:         {a.date.timeFrom} - {a.date.timeTo}");

                }
            }
            Console.WriteLine("\n>> Press any key to continue.");
            Console.ReadKey(true);
        }
    }
}
