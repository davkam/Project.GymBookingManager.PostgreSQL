using System;
using System.Collections.Generic;
using Gym_Booking_Manager.Reservations;
using Gym_Booking_Manager.Users;
using Gym_Booking_Manager.Calendars;

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
        public Calendar date { get; set; }
        public Reservation reservation { get; set; }
        public List<Customer>? participants { get; set; }

        public Activity(int id, string name, string description, bool open, int limit,
            Staff instructor, Calendar date, Reservation reservation, List<Customer>? participants = default(List<Customer>))
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
                string[] lines = File.ReadAllLines("Activities/Activities.txt");
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
                    var calendar = new Calendar(DateTime.Parse(stringsA[6]), DateTime.Parse(stringsA[7]));
                    var reservation = Reservation.reservations.Find(r => r.id == int.Parse(stringsA[8]));
                    var activity = new Activity(int.Parse(stringsA[0]), stringsA[1], stringsA[2], bool.Parse(stringsA[3]), int.Parse(stringsA[4]), staff, calendar, reservation, participants);
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
                using (StreamWriter writer = new StreamWriter("Activities/Activities.txt", false))
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
        private static int GetID()
        {
            int id = getActivityID;
            getActivityID++;
            return id;
        }
        public void NewActivity()
        {
            // Staff registers new activites.
        }
        public void EditActivity()
        {
            // Staff edits activites.
        }
        public void DeleteActivity()
        {
            // Staff deletes activites.
        }
        public void ActivityRegister()
        {
            // Customers register for activities.
        }
        public void ActivityCancel()
        {
            // Customers cancels registered activities.
        }
        public void ActivityView()
        {
            // Customers views registered activities.
        }
    }
}
