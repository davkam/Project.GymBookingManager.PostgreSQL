using Gym_Booking_Manager.Reservables;
using Gym_Booking_Manager.Reservations;
using Gym_Booking_Manager.Schedules;
using Gym_Booking_Manager.Users;
using System.Globalization;

namespace Gym_Booking_Manager.Activities
{
    public class Activity
    {
        public static int getActivityID;
        public static List<Activity> activities = new List<Activity>();

        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public bool open { get; set; }
        public int limit { get; set; }
        public Staff instructor { get; set; }
        public Schedule date { get; set; }
        public Reservation reservation { get; set; }
        public List<Customer>? participants { get; set; }

        public Activity(int id, string name, string description, bool open, int limit,
            Staff instructor, Schedule date, Reservation reservation, List<Customer>? participants = default(List<Customer>))
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
                    var schedule = new Schedule(DateTime.Parse(stringsA[6]), DateTime.Parse(stringsA[7]));
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
        public static void NewActivity(int idStaff) // TBD: Parameter to Staff!
        {
            int id = GetActivityID(); string Name; string Description; bool Open = true; int participantsNo = 0; Staff Instructor = User.users[idStaff] as Staff; List<Customer>? Participants=new List<Customer>();
            bool overlap = false;
            List<int> reservableToList = new List<int>();
            DateTime[] date = new DateTime[2];
            Schedule.DateSelecter(date);
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
            ChooseSpace(idStaff, date, reservableToList);
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
            string input = Console.ReadLine();
            int.TryParse( input, out participantsNo);
            if(participantsNo > 0 && participantsNo<= Limit)
                {
                    break;
                }
            else Console.WriteLine("Incorrect input!");
            }
            Reservation Reservationn = Reservation.reservations[Reservation.reservations.Count() - 1];
            Schedule datee = new Schedule(date[0], date[1]); 
            Activity.activities.Add(new Activity(id, Name, Description, Open, participantsNo, Instructor, datee, Reservationn, Participants));
            SaveActivities();
        }
        static void ChooseSpace(int idStaff, DateTime[] date, List<int> reservableToList) // TBD: Parameter to Staff!
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
                    Reservation.reservations.Add(new Reservation(Reservation.GetReservationID(), User.users[idStaff], new Schedule(date[0], date[1]), list));
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
        public static void DeleteActivity() // TBD: Add Staff parameter.
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
        public static void BookActivity(int week, int ID)   //TBD: Parameter to Customer.
        {
            Customer customer = (Customer)User.users[ID];
            bool printedSomething = false;
            List<Activity> weekActivities = Activity.activities.Where(a => ISOWeek.GetWeekOfYear(a.date.timeFrom) == week).ToList();
            Console.WriteLine("Which activity do you want to book? (type in the number from the listed activites)");
            string input = Console.ReadLine();
            int.TryParse(input, out var which);
            foreach (Activity a in weekActivities)
            {
                if (a.id == which && a.limit > a.participants.Count() && customer.subStart < a.date.timeFrom && customer.subEnd > a.date.timeTo && Activity.activities[a.id].participants.Contains(User.users[ID]) == false)
                {
                    Console.WriteLine($"{a.name} booked!");
                    Activity.activities[a.id].participants.Add(customer);
                }
                else if (a.id == which && a.limit == a.participants.Count() && customer.subStart < a.date.timeFrom && customer.subEnd > a.date.timeTo && Activity.activities[a.id].participants.Contains(User.users[ID]) == false)
                {
                    Console.WriteLine("The activity is fully booked");
                }
                else if (a.id == which && Activity.activities[a.id].participants.Contains(User.users[ID]) == true)
                {
                    Console.WriteLine("You are already booked to this activity");
                }
                else if (a.id == which && customer.subStart > a.date.timeFrom || a.id == which && customer.subEnd < a.date.timeTo)
                {
                    Console.WriteLine("You do not have an active subscription for the date of this activity");
                }
            }
            Activity.SaveActivities();
        }
        public static void ActivityCancel(int ID)   //TBD: Parameter to Customer.
        {
            int findSpotInList=0;
            int result;
            List<int> whichActivity=new List<int>();
            int counter = 1;
            Console.WriteLine("You are booked for the following activities: ");
            foreach (Activity a in Activity.activities)
            {
                if (a.participants.Contains(User.users[ID]))
                {
                    Console.WriteLine(counter+" "+a.name+" on "+a.date.timeFrom);
                    whichActivity.Add(a.id);
                    counter++;
                }
            }
            Console.WriteLine();
            Console.WriteLine("Which do you want to cancel?(0 to exit otherwise enter the number of the one you want to cancel)");
            string input = Console.ReadLine();
            int.TryParse(input, out result);
            if (result == 0) Console.WriteLine("Goodbye");
            else if (result > 0 && result < whichActivity.Count())
            {
                Console.WriteLine("You have removed yourself from activity: " + activities[whichActivity[result-1]].name);
                
                foreach(Customer customer in activities[whichActivity[result - 1]].participants)
                {
                    if (customer.id == ID) break;
                    findSpotInList++;
                }
                activities[whichActivity[result - 1]].participants.RemoveAt(findSpotInList);
                SaveActivities();
            }
            else Console.WriteLine("Incorrect selection, bye!");
        }
        public static void ActivityView(int ID) //TBD: Parameter to Customer.
        {
            Schedule.ViewScheduleMenu(ID);
        }
    }
}
