namespace Gym_Booking_Manager.Schedules
{
    public class Schedule
    {
        public static void DateSelecter(DateTime[] date)
        {
            bool run = true;
            string input;
        rerun:
            while (run == true)
            {
                Console.WriteLine("Enter a date and time of start of lending (in the format yyyy-MM-dd HH):");
                input = Console.ReadLine() + ":00:00";
                if (DateTime.TryParse(input, out date[0]))
                {
                    run = false;
                }
                else
                {
                    Console.WriteLine("Invalid date and time format.");
                }
            }
            run = true;
            while (run == true)
            {
                Console.WriteLine("Enter a date and time of end of lending (in the format yyyy-MM-dd HH):");
                input = Console.ReadLine() + ":00:00";
                if (DateTime.TryParse(input, out date[1]))
                {
                    run = false;
                }
                else
                {
                    Console.WriteLine("Invalid date and time format.");
                }
            }
            if (date[0] >= date[1])
            {
                Console.WriteLine("End date/time is the same or earlier then start date/time, try again.");
                run = true;
                goto rerun;
            }
        }
    }
}

