namespace Gym_Booking_Manager.Management.Logger
{
    public class GBMLogger
    {
        //public static GBMLogger dataLogger = new GBMLogger("Logger/DataLogger.txt");
        //public static GBMLogger activityLogger = new GBMLogger("Logger/ActivityLogger.txt");
        //public static GBMLogger reservableLogger = new GBMLogger("Logger/ReservableLogger.txt");
        //public static GBMLogger reservationLogger = new GBMLogger("Logger/ReservationLogger.txt");
        //public static GBMLogger userLogger = new GBMLogger("Logger/UserLogger.txt");

        private readonly string _logFilePath;
        public GBMLogger(string logFilePath)
        {
            _logFilePath = logFilePath;
        }
        public void LogActivity(string message)
        {
            string logDirectory = Path.GetDirectoryName(_logFilePath);
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            using (StreamWriter writer = File.AppendText(_logFilePath))
            {
                writer.WriteLine($"{DateTime.Now} - {message}");
            }
        }
    }
}
