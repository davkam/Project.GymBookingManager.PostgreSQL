﻿using Gym_Booking_Manager.Activities;
using Gym_Booking_Manager.Management.Logger;
using Gym_Booking_Manager.Managements;
using Gym_Booking_Manager.Reservables;
using Gym_Booking_Manager.Reservations;
using Gym_Booking_Manager.Users;

namespace Gym_Booking_Manager
{
    public class Program
    {
        // PUBLIC LOGGER INSTANTIATION:
        public static GBMLogger logger = new GBMLogger("Logger/GBMLogger.txt");
        static void Main(string[] args)
        {
            // LOAD DATA METHODS RUNS BELOW:
            User.LoadUsers();
            Reservable.LoadReservables();
            Reservation.LoadReservations();
            Activity.LoadActivities();

            // MAIN SOFTWARE LOOP RUNS BELOW:
            RunGBM();
        }
        static void RunGBM()
        {
            bool shutdown = false;
            int userID;
            User? currentUser;            
            do
            {
                Console.Clear();                
                Console.WriteLine("<< GYM BOOKING MANAGER >>\n");
                Console.WriteLine(">> Press any key to log in, or [ESC] to quit!");
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                if (keyInfo.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine(">> Thank you for using \"Gym Booking Manager\"!");
                    Task.Delay(1000).Wait();
                    shutdown = true;
                }
                else
                {
                    var user = Login.UserLogin();

                    if (user != null) user.MainMenu();
                    else { logger.LogActivity($"ERROR: RunGMB() - Login attempt unsuccessful."); }
                }
            } while (!shutdown);
        }
    }
}