using Gym_Booking_Manager.Managements;

namespace Gym_Booking_Manager.Dates
{
    public class Date
    {
        public DateTime timeFrom { get; set; }
        public DateTime timeTo { get; set; }
        public Date(DateTime timeFrom, DateTime timeTo)
        {
            this.timeFrom = timeFrom;
            this.timeTo = timeTo;
        }
        public static DateTime DateTimeSelecter()
        {
            DateTime selectDateTime = DateTime.Now.Date;

            YearSelecter(ref selectDateTime);
            MonthSelecter(ref selectDateTime);
            DaySelecter(ref selectDateTime);
            TimeSelecter(ref selectDateTime);

            return selectDateTime;
        }
        private static void YearSelecter(ref DateTime dateTime)
        {
            ConsoleKeyInfo keyInfo;
            bool runYear = true;
            while (runYear)
            {
                Console.WriteLine($"{$"<< YEAR SELECTER >>",50}");
                Console.WriteLine($"{$">> {dateTime.ToString()[0..^9]} <<",48}");
                Console.WriteLine($"{"<< PREV YEAR[LEFT.ARROW]",-26}{"ACCEPT[ENTER]",-15}{"EXIT[ESC]",-11}{"[RIGHT.ARROW]NEXT YEAR >>",-24}");

                keyInfo = Console.ReadKey(true);
                switch (keyInfo.Key)
                {
                    case ConsoleKey.LeftArrow:
                        if (dateTime > DateTime.Now) dateTime = dateTime.AddYears(-1);
                        break;
                    case ConsoleKey.RightArrow: dateTime = dateTime.AddYears(1); break;
                    case ConsoleKey.Escape: return;
                    case ConsoleKey.Enter: return;
                    default: break;
                }
                Additions.ClearLines(3);
            }
        }
        private static void MonthSelecter(ref DateTime dateTime)
        {
            ConsoleKeyInfo keyInfo;
            bool runMonth = true;
            while (runMonth)
            {
                Console.WriteLine($"{$"<< MONTH SELECTER >>",50}");
                Console.WriteLine($"{$">> {dateTime.ToString()[0..^9]} <<",48}");
                Console.WriteLine($"{"<< PREV MONTH[LEFT.ARROW]",-26}{"ACCEPT[ENTER]",-15}{"EXIT[ESC]",-11}{"[RIGHT.ARROW]NEXT MONTH >>",-24}");

                keyInfo = Console.ReadKey(true);
                switch (keyInfo.Key)
                {
                    case ConsoleKey.LeftArrow:
                        if (dateTime > DateTime.Now) dateTime = dateTime.AddMonths(-1);
                        break;
                    case ConsoleKey.RightArrow: dateTime = dateTime.AddMonths(1); break;
                    case ConsoleKey.Escape: return;
                    case ConsoleKey.Enter: return;
                    default: break;
                }
                Additions.ClearLines(3);
            }
        }
        private static void DaySelecter(ref DateTime dateTime)
        {
            ConsoleKeyInfo keyInfo;
            bool runDay = true;
            while (runDay)
            {
                Console.WriteLine($"{$"<< DAY SELECTER >>",50}");
                Console.WriteLine($"{$">> {dateTime.ToString()[0..^9]} <<",48}");
                Console.WriteLine($"{"<< PREV DAY[LEFT.ARROW]",-26}{"ACCEPT[ENTER]",-15}{"EXIT[ESC]",-11}{"[RIGHT.ARROW]NEXT DAY >>",-24}");

                keyInfo = Console.ReadKey(true);
                switch (keyInfo.Key)
                {
                    case ConsoleKey.LeftArrow:
                        if (dateTime > DateTime.Now) dateTime = dateTime.AddDays(-1);
                        break;
                    case ConsoleKey.RightArrow: dateTime = dateTime.AddDays(1); break;
                    case ConsoleKey.Escape: return;
                    case ConsoleKey.Enter: return;
                    default: break;
                }
                Additions.ClearLines(3);
            }
        }
        private static void TimeSelecter(ref DateTime dateTime, int minHour = 0, int maxHour = 23)
        {
            ConsoleKeyInfo keyInfo;
            bool runTime = true;

            while (runTime)
            {
                if (dateTime.Hour < minHour && minHour != 0) dateTime = dateTime.AddHours(1);
                if (dateTime.Hour > maxHour && maxHour != 23) dateTime = dateTime.AddHours(-1);

                Console.WriteLine($"{$"<< TIME SELECTER >>",50}");
                Console.WriteLine($"{$">> {dateTime.ToString()[^8..]} <<",48}");
                Console.WriteLine($"{"<< PREV HOUR[LEFT.ARROW]",-26}{"ACCEPT[ENTER]",-15}{"EXIT[ESC]",-11}{"[RIGHT.ARROW]NEXT HOUR >>",-24}");

                keyInfo = Console.ReadKey(true);
                switch (keyInfo.Key)
                {
                    case ConsoleKey.LeftArrow:
                        if (dateTime > DateTime.Now) dateTime = dateTime.AddHours(-1);
                        break;
                    case ConsoleKey.RightArrow: dateTime = dateTime.AddHours(1); break;
                    case ConsoleKey.Escape: return;
                    case ConsoleKey.Enter: return;
                    default: break;
                }
                Additions.ClearLines(3);
            }
        }
    }
}
