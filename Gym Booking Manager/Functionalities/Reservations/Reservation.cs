using Gym_Booking_Manager.Reservables;
using Gym_Booking_Manager.Schedules;
using Gym_Booking_Manager.Users;

namespace Gym_Booking_Manager.Reservations
{
    public class Reservation
    {
        public static int getReservationID;
        public static List<Reservation> reservations = new List<Reservation>();

        public int id { get; set; }
        public User owner { get; set; }
        public Schedule date { get; set; }
        public List<Reservable>? reservables { get; set; }

        public Reservation(int id, User owner, Schedule date, List<Reservable>? reservables = default)
        {
            this.id = id;
            this.owner = owner;
            this.reservables = new List<Reservable>();
            this.date = date;
            this.reservables = reservables;
        }
        public static void LoadReservations()
        {
            try
            {
                string[] lines = File.ReadAllLines("Functionalities/Reservations/Reservations.txt");
                getReservationID = int.Parse(lines[0]);

                for (int i = 1; i < lines.Length; i++)
                {
                    string[] stringsA = lines[i].Split(";");
                    string[] stringsB = stringsA[4].Split(",");

                    var reservables = new List<Reservable>();

                    // Adding objects(Reservable) from public list(Reservable.reservables) to list(reservables) based on integers(Reservable.id).
                    foreach (string strB in stringsB)
                    {
                        var reservable = Reservable.reservables.Find(r => r.id == int.Parse(strB));
                        reservables.Add(reservable);
                    }

                    var owner = User.users.Find(u => u.id == int.Parse(stringsA[1]));
                    var schedule = new Schedule(DateTime.Parse(stringsA[2]), DateTime.Parse(stringsA[3]));
                    var reservation = new Reservation(int.Parse(stringsA[0]), owner, schedule, reservables);
                    reservations.Add(reservation);
                }
                Program.logger.LogActivity("INFO: LoadReservations() - Read data (\"Reservations/Reservations.txt\") successful.");
            }
            catch { Program.logger.LogActivity("ERROR: LoadReservations() - Read data (\"Reservations/Reservations.txt\") unsuccessful."); }
        }
        public static void SaveReservations()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter("Functionalities/Reservations/Reservations.txt", false))
                {
                    writer.WriteLine(getReservationID);
                    foreach (Reservation rsv in reservations)
                    {
                        string reservables = string.Empty;
                        foreach (Reservable rvb in rsv.reservables)
                        {
                            reservables += $"{rvb.id},";
                        }
                        reservables = reservables[0..^1];
                        writer.WriteLine($"{rsv.id};{rsv.owner.id};{rsv.date.timeFrom};{rsv.date.timeTo};{reservables}");
                    }
                }
                Program.logger.LogActivity("INFO: SaveReservations() - Write data (\"Reservations/Reservations.txt\") successful.");
            }
            catch { Program.logger.LogActivity("ERROR: SaveReservations() - Write data (\"Reservations/Reservations.txt\") unsuccessful."); }
        }
        public static int GetReservationID()
        {
            int id = getReservationID;
            getReservationID++;
            return id;
        }
        public static void NewReservation(User user)
        {
            if (user is Staff) NewReservationStaff(user.id);
            else
            {
                Customer customer = (Customer)user;
                if (customer.isSub) NewReservationUserMember(customer.id);
                else NewReservationUserNonMember(customer.id);
            }
        }
        private static void NewReservationStaff(int userID)
        {
            bool overlap = false;
            List<int> reservableToList = new List<int>();
            DateTime[] date = new DateTime[2];
            Schedule.DateSelecter(date);
            for (int i = 0; i < Reservable.reservables.Count(); i++)
            {
                overlap = false;
                for (int j = 0; j < reservations.Count(); j++)
                {
                    for (int k = 0; k < reservations[j].reservables.Count(); k++)
                    {
                        if (i == reservations[j].reservables[k].id) overlap = date[0] < reservations[j].date.timeTo && reservations[j].date.timeFrom < date[1];
                        if (overlap) break;
                    }
                    if (overlap) break;
                }
                if (!overlap) reservableToList.Add(Reservable.reservables[i].id);
            }
            ChooseReservation(reservableToList, userID, date);
        }
        private static void NewReservationUserMember(int userID)
        {
            bool overlap = false;
            List<int> ReservableToList = new List<int>();
            DateTime[] date = new DateTime[2];
            Schedule.DateSelecter(date);
            for (int i = 0; i < Reservable.reservables.Count(); i++)
            {
                overlap = false;
                for (int j = 0; j < reservations.Count(); j++)
                {
                    for (int k = 0; k < reservations[j].reservables.Count(); k++)
                    {
                        if (i == reservations[j].reservables[k].id) overlap = date[0] < reservations[j].date.timeTo && reservations[j].date.timeFrom < date[1];
                        if (overlap) break;
                    }
                    if (overlap) break;
                }
                if (!overlap && Reservable.reservables[i] is not Space) ReservableToList.Add(Reservable.reservables[i].id);
            }
            ChooseReservation(ReservableToList, userID, date);
        }
        private static void NewReservationUserNonMember(int userID)
        {
            bool overlap = false;
            List<int> ReservableToList = new List<int>();
            DateTime[] date = new DateTime[2];
            Schedule.DateSelecter(date);
            for (int i = 0; i < Reservable.reservables.Count(); i++)
            {
                overlap = false;
                for (int j = 0; j < reservations.Count(); j++)
                {
                    for (int k = 0; k < reservations[j].reservables.Count(); k++)
                    {
                        if (i == reservations[j].reservables[k].id) overlap = date[0] < reservations[j].date.timeTo && reservations[j].date.timeFrom < date[1];
                        if (overlap) break;
                    }
                    if (overlap) break;
                }
                if (!overlap && Reservable.reservables[i] is PTrainer) ReservableToList.Add(Reservable.reservables[i].id);
                if (!overlap && Reservable.reservables[i] is Equipment)
                {
                    Equipment equipment = (Equipment)Reservable.reservables[i];
                    if (equipment.bookable == true) ReservableToList.Add(Reservable.reservables[i].id);
                }
            }
            ChooseReservation(ReservableToList, userID, date);
        }
        private static void ChooseReservation(List<int> ReservableToList, int userID, DateTime[] date)
        {
            List<Reservable> list = new List<Reservable>();
            Console.WriteLine("Available Equipment: ");
            for (int i = 0; i < ReservableToList.Count(); i++)
            {
                Console.WriteLine($"{i + 1} {Reservable.reservables[ReservableToList[i]].name}");
            }

            while (true)
            {
                Console.WriteLine("Type in which equiment/PT you wish to reserve(number): ");
                string input = Console.ReadLine();
                bool isNumber;
                isNumber = int.TryParse(input, out int number);
                if (isNumber && number > 0 && number < ReservableToList.Count() + 1)
                {
                    list.Add(Reservable.reservables[ReservableToList[number - 1]]);
                    Console.WriteLine("You have booked " + Reservable.reservables[ReservableToList[number - 1]].name);
                    reservations.Add(new Reservation(GetReservationID(), User.users[userID], new Schedule(date[0], date[1]), list));
                    SaveReservations();
                    break;
                }
                else
                {
                    Console.WriteLine("Incorrect input!");
                }
            }
        }
        public void UpdateReservation()
        {
            // Update reservations from list.
        }
        public void DeleteReservation()
        {
            // Delete reservations from list.
        }
    }
}
