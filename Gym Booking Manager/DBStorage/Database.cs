using System;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using Gym_Booking_Manager.Activities;
using Gym_Booking_Manager.Dates;
using Gym_Booking_Manager.Reservables;
using Gym_Booking_Manager.Reservations;
using Gym_Booking_Manager.Users;
using Npgsql;
using static System.Net.Mime.MediaTypeNames;

namespace Gym_Booking_Manager.DBStorage
{
    public class Database
    {
        public static Database Instance => _instance ??= new Database();
        private static Database? _instance;

        private const string _configFilePath = "db_config.json";

        private Dictionary<string, string> _connectionInfo;
        private string _connectionString;

        private Database()
        {
            _connectionInfo = new Dictionary<string, string>()
            {
                { "Host", "" },
                { "Port", "" },
                { "User Id", "" },
                { "Password", "" },
                { "Database", "" }
            };

            SetConnectionString();
            CreateTables();
        }

        #region Public Methods
        public List<User> GetAllUsers()
        {
            var users = new List<User>();
            string query = "SELECT id, first_name, last_name, ssn, phone, email, login_name, login_password, user_type, sub_start, sub_end, is_sub, is_guest FROM users;";

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                connection.Open();

                using var command = new NpgsqlCommand(query, connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    User user;
                    if (reader.GetString(8) == "admin") user = new Admin();
                    else if (reader.GetString(8) == "staff") user = new Staff();
                    else
                    {
                        user = new Customer()
                        {
                            subStart = reader.IsDBNull(9) ? default(DateTime) : reader.GetDateTime(9),
                            subEnd = reader.IsDBNull(10) ? default(DateTime) : reader.GetDateTime(10),
                            isSub = reader.IsDBNull(11) ? false : reader.GetBoolean(11),
                            isGuest = reader.IsDBNull(12) ? false : reader.GetBoolean(12)
                        };
                    }

                    user.id = reader.GetInt32(0);
                    user.firstName = reader.GetString(1);
                    user.lastName = reader.GetString(2);
                    user.ssn = reader.GetString(3);
                    user.phone = reader.IsDBNull(4) ? null : reader.GetString(4);
                    user.email = reader.IsDBNull(5) ? null : reader.GetString(5);
                    user.loginName = reader.GetString(6);
                    user.loginPass = reader.GetString(7);

                    users.Add(user);

                    // TBD: Temporary solution to handling user id, change to GUID/UUID
                    if (user.id >= User.getUserID) User.getUserID = user.id + 1;
                }

                Program.logger.LogActivity("INFO: GetAllUsers() - Loaded user data successfully.");
            }
            catch (Exception ex)
            {
                // TBD: Improve log handling
                Console.WriteLine($"Database.LoadAllUsers Error: {ex.Message}");
                Program.logger.LogActivity($"ERROR: GetAllUsers() - Loaded user data unsuccessfully. ERROR: {ex.Message}");
            }

            return users;
        }
        public bool AddUser(User user)
        {
            string query = "";

            if (user is Customer)
            {
                query = @"
                INSERT INTO users (id, first_name, last_name, ssn, phone, email, login_name, login_password, user_type, sub_start, sub_end, is_sub, is_guest)
                VALUES (@userId, @firstName, @lastName, @ssn, @phone, @email, @loginName, @loginPassword, @userType, @subStart, @subEnd, @isSub, @isGuest);
                ";
            }
            else
            {
                query = @"
                INSERT INTO users (id, first_name, last_name, ssn, phone, email, login_name, login_password, user_type)
                VALUES (@userId, @firstName, @lastName, @ssn, @phone, @email, @loginName, @loginPassword, @userType);
                ";
            }

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                connection.Open();

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("userId", user.id);
                command.Parameters.AddWithValue("firstName", user.firstName);
                command.Parameters.AddWithValue("lastName", user.lastName);
                command.Parameters.AddWithValue("ssn", user.ssn);
                command.Parameters.AddWithValue("phone", user.phone);
                command.Parameters.AddWithValue("email", user.email);
                command.Parameters.AddWithValue("loginName", user.loginName);
                command.Parameters.AddWithValue("loginPassword", user.loginPass);   // TBD: Use password hashing!
                command.Parameters.AddWithValue("userType", user.GetUserType());

                if (user is Customer)
                {
                    Customer customer = (Customer)user;
                    command.Parameters.AddWithValue("subStart", customer.subStart);
                    command.Parameters.AddWithValue("subEnd", customer.subEnd);
                    command.Parameters.AddWithValue("isSub", customer.isSub);
                    command.Parameters.AddWithValue("isGuest", customer.isGuest);
                }

                command.ExecuteNonQuery();

                Program.logger.LogActivity($"INFO: AddUser() - Added new user successfully. USER: {user.loginName}");

                return true;
            }
            catch (Exception ex)
            {
                // TBD: Improve log handling
                Console.WriteLine($"Database.SaveUser Error: {ex.Message}");
                Program.logger.LogActivity($"ERROR: AddUser() - Added new user unsuccessfully. USER: {user.loginName} ERROR: {ex.Message}");
                return false;
            }
        }
        public bool UpdateUser(User user)
        {
            string query = "";
            if (user is Customer)
            {
                query = @"
                UPDATE users
                SET first_name = @firstName,
                    last_name = @lastName,
                    ssn = @ssn,
                    phone = @phone,
                    email = @email,
                    login_name = @loginName,
                    login_password = @loginPassword,
                    sub_start = @subStart,
                    sub_end = @subEnd,
                    is_sub = @isSub,
                    is_guest = @isGuest
                WHERE id = @userId;
                ";
            }
            else
            {
                query = @"
                UPDATE users
                SET first_name = @firstName,
                    last_name = @lastName,
                    ssn = @ssn,
                    phone = @phone,
                    email = @email,
                    login_name = @loginName,
                    login_password = @loginPassword
                WHERE id = @userId;
                ";
            }

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                connection.Open();

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("userId", user.id);
                command.Parameters.AddWithValue("firstName", user.firstName);
                command.Parameters.AddWithValue("lastName", user.lastName);
                command.Parameters.AddWithValue("ssn", user.ssn);
                command.Parameters.AddWithValue("phone", user.phone);
                command.Parameters.AddWithValue("email", user.email);
                command.Parameters.AddWithValue("loginName", user.loginName);
                command.Parameters.AddWithValue("loginPassword", user.loginPass);

                if (user is Customer)
                {
                    Customer customer = (Customer)user;
                    command.Parameters.AddWithValue("subStart", customer.subStart);
                    command.Parameters.AddWithValue("subEnd", customer.subEnd);
                    command.Parameters.AddWithValue("isSub", customer.isSub);
                    command.Parameters.AddWithValue("isGuest", customer.isGuest);
                }

                int rowsAffected = command.ExecuteNonQuery();

                Program.logger.LogActivity($"INFO: UpdateUser() - Updated user successfully. USER: {user.loginName}");

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                // TBD: Improve log handling
                Console.WriteLine($"Database.UpdateUser Error: {ex.Message}");
                Program.logger.LogActivity($"ERROR: UpdateUser() - Updated user unsuccessfully. USER: {user.loginName} ERROR: {ex.Message}");
                return false;
            }
        }
        public bool RemoveUser(User user)
        {
            string query = @"
                DELETE FROM users
                WHERE id = @userId;
            ";

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                connection.Open();

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("userId", user.id);

                int rowsAffected = command.ExecuteNonQuery();

                Program.logger.LogActivity($"INFO: RemoveUser() - Removed user successfully. USER: {user.loginName}");

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                // TBD: Improve log handling
                Console.WriteLine($"Database.RemoveUser Error: {ex.Message}");
                Program.logger.LogActivity($"ERROR: RemoveUser() - Removed user unsuccessfully. USER: {user.loginName} ERROR: {ex.Message}");
                return false;
            }
        }
        public List<Reservable> GetAllReservables()
        {
            var reservables = new List<Reservable>();
            string query = "SELECT id, name, description, is_available, reservable_type, members_only, capacity, instructor_id FROM reservables;";

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                connection.Open();

                using var command = new NpgsqlCommand(query, connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Reservable reservable;
                    if (reader.GetString(4) == "equipment")
                    {
                        reservable = new Equipment()
                        {
                            membersOnly = reader.GetBoolean(5)
                        };
                    }
                    else if (reader.GetString(4) == "space") 
                    {
                        reservable = new Space()
                        {
                            capacity = reader.GetInt32(6)
                        };
                    }
                    else
                    {
                        reservable = new PTrainer()
                        {
                            instructor = (Staff)User.users.FirstOrDefault(u => u.id == reader.GetInt32(7))
                        };
                    }

                    reservable.id = reader.GetInt32(0);
                    reservable.name = reader.GetString(1);
                    reservable.description = reader.GetString(2);
                    reservable.isAvailable = reader.GetBoolean(3);

                    reservables.Add(reservable);

                    // TBD: Temporary solution to handling id, change to GUID/UUID
                    if (reservable.id >= Reservable.getReservableID) Reservable.getReservableID = reservable.id + 1;
                }

                Program.logger.LogActivity("INFO: GetAllReservables() - Loaded reservables data successfully.");
            }
            catch (Exception ex)
            {
                // TBD: Improve log handling
                Console.WriteLine($"Database.GetAllReservables Error: {ex.Message}");
                Program.logger.LogActivity($"ERROR: GetAllReservables() - Loaded reservables data unsuccessfully. ERROR: {ex.Message}");
            }

            return reservables;
        }
        public bool AddReservable(Reservable reservable)
        {
            string query = "";
            if (reservable is Equipment)
            {
                query = @"
                    INSERT INTO reservables (id, name, description, is_available, reservable_type, members_only)
                    VALUES (@reservableId, @name, @description, @isAvailable, @reservableType, @membersOnly);
                ";
            }
            else if (reservable is Space) 
            {
                query = @"
                    INSERT INTO reservables (id, name, description, is_available, reservable_type, capacity)
                    VALUES (@reservableId, @name, @description, @isAvailable, @reservableType, @capacity);
                ";
            }
            else if (reservable is PTrainer)
            {
                query = @"
                    INSERT INTO reservables (id, name, description, is_available, reservable_type, instructor_id)
                    VALUES (@reservableId, @name, @description, @isAvailable, @reservableType, @instructorId);
                ";
            }

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                connection.Open();

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("reservableId", reservable.id);
                command.Parameters.AddWithValue("name", reservable.name);
                command.Parameters.AddWithValue("description", reservable.description);
                command.Parameters.AddWithValue("isAvailable", reservable.isAvailable);
                command.Parameters.AddWithValue("reservableType", reservable.GetReservableType());

                if (reservable is Equipment)
                {
                    Equipment equipment = reservable as Equipment;
                    command.Parameters.AddWithValue("membersOnly", equipment.membersOnly);
                }
                else if (reservable is Space)
                {
                    Space space = reservable as Space;
                    command.Parameters.AddWithValue("capacity", space.capacity);
                }
                else if (reservable is PTrainer)
                {
                    PTrainer pTrainer = reservable as PTrainer;
                    command.Parameters.AddWithValue("instructorId", pTrainer.instructor.id);
                }

                command.ExecuteNonQuery();

                Program.logger.LogActivity($"INFO: AddReservable() - Added reservable successfully. RESERVABLE ID: {reservable.id}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database.AddReservable Error: {ex.Message}");
                Program.logger.LogActivity($"ERROR: AddReservable() - Failed to add reservable. ERROR: {ex.Message}");
                return false;
            }
        }
        public bool UpdateReservable(Reservable reservable)
        {
            string query = "";
            if (reservable is Equipment)
            {
                query = @"
                    UPDATE reservables
                    SET name = @name,
                        description = @description,
                        is_available = @isAvailable,
                        reservable_type = @reservableType,
                        members_only = @membersOnly
                    WHERE id = @reservableId;
                ";
            }
            else if (reservable is Space)
            {
                query = @"
                    UPDATE reservables
                    SET name = @name,
                        description = @description,
                        is_available = @isAvailable,
                        reservable_type = @reservableType,
                        capacity = @capacity
                    WHERE id = @reservableId;
                ";
            }
            else if (reservable is PTrainer)
            {
                query = @"
                    UPDATE reservables
                    SET name = @name,
                        description = @description,
                        is_available = @isAvailable,
                        reservable_type = @reservableType,
                        instructor_id = @instructorId
                    WHERE id = @reservableId;
                ";
            }

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                connection.Open();

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("reservableId", reservable.id);
                command.Parameters.AddWithValue("name", reservable.name);
                command.Parameters.AddWithValue("description", reservable.description);
                command.Parameters.AddWithValue("isAvailable", reservable.isAvailable);
                command.Parameters.AddWithValue("reservableType", reservable.GetReservableType());

                if (reservable is Equipment)
                {
                    Equipment equipment = reservable as Equipment;
                    command.Parameters.AddWithValue("membersOnly", equipment.membersOnly);
                }
                else if (reservable is Space)
                {
                    Space space = reservable as Space;
                    command.Parameters.AddWithValue("capacity", space.capacity);
                }
                else if (reservable is PTrainer)
                {
                    PTrainer pTrainer = reservable as PTrainer;
                    command.Parameters.AddWithValue("instructorId", pTrainer.instructor.id);
                }

                int rowsAffected = command.ExecuteNonQuery();

                Program.logger.LogActivity($"INFO: UpdateReservable() - Updated reservable successfully. RESERVABLE ID: {reservable.id}");
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database.UpdateReservable Error: {ex.Message}");
                Program.logger.LogActivity($"ERROR: UpdateReservable() - Failed to update reservable. ERROR: {ex.Message}");
                return false;
            }
        }
        public bool RemoveReservable(Reservable reservable)
        {
            string query = @"
                DELETE FROM reservables
                WHERE id = @reservableId;
            ";

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                connection.Open();

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("reservableId", reservable.id);

                int rowsAffected = command.ExecuteNonQuery();

                Program.logger.LogActivity($"INFO: RemoveReservable() - Removed reservable successfully. RESERVABLE ID: {reservable.id}");
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database.RemoveReservable Error: {ex.Message}");
                Program.logger.LogActivity($"ERROR: RemoveReservable() - Failed to remove reservable. ERROR: {ex.Message}");
                return false;
            }
        }
        public List<Reservation> GetAllReservations()
        {
            var reservations = new List<Reservation>();
            string query = @"
                SELECT r.id, r.owner_id, r.reservation_date_from, r.reservation_date_to, rr.reservable_id FROM reservations r
                LEFT JOIN reservation_reservables rr ON r.id = rr.reservation_id;";

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                connection.Open();

                using var command = new NpgsqlCommand(query, connection);
                using var reader = command.ExecuteReader();

                Dictionary<int, Reservation> reservationMap = new();

                while (reader.Read())
                {
                    int reservationId = reader.GetInt32(0);

                    if (!reservationMap.ContainsKey(reservationId))
                    {
                        var reservation = new Reservation
                        {
                            id = reservationId,
                            owner = User.users.FirstOrDefault(u => u.id == reader.GetInt32(1)),
                            date = new Date(
                                reader.GetDateTime(2),
                                reader.GetDateTime(3)
                                ),
                            reservables = new List<Reservable>()
                        };

                        reservationMap[reservationId] = reservation;
                        reservations.Add(reservation);

                        // TBD: Temporary solution to handling id, change to GUID/UUID
                        if (reservation.id >= Reservation.getReservationID) Reservation.getReservationID = reservation.id + 1;
                    }

                    if (!reader.IsDBNull(4))
                    {
                        var reservable = Reservable.reservables.FirstOrDefault(r => r.id == reader.GetInt32(4));
                        reservationMap[reservationId].reservables.Add(reservable);
                    }
                }

                Program.logger.LogActivity("INFO: GetAllReservations() - Loaded reservations data successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database.GetAllReservations Error: {ex.Message}");
                Program.logger.LogActivity($"ERROR: GetAllReservations() - Loaded reservations data unsuccessfully. ERROR: {ex.Message}");
            }

            return reservations;
        }
        public bool AddReservation(Reservation reservation)
        {
            string query = @"
                INSERT INTO reservations (id, owner_id, reservation_date_from, reservation_date_to)
                VALUES (@reservationId, @ownerId, @dateFrom, @dateTo);
            ";

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                connection.Open();

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("reservationId", reservation.id);
                command.Parameters.AddWithValue("ownerId", reservation.owner.id);
                command.Parameters.AddWithValue("dateFrom", reservation.date.timeFrom);
                command.Parameters.AddWithValue("dateTo", reservation.date.timeTo);

                command.ExecuteNonQuery();

                foreach (var reservable in reservation.reservables)
                {
                    string reservableQuery = @"
                        INSERT INTO reservation_reservables (reservation_id, reservable_id)
                        VALUES (@reservationId, @reservableId);
                    ";

                    using var reservableCommand = new NpgsqlCommand(reservableQuery, connection);
                    reservableCommand.Parameters.AddWithValue("reservationId", reservation.id);
                    reservableCommand.Parameters.AddWithValue("reservableId", reservable.id);
                    reservableCommand.ExecuteNonQuery();
                }

                Program.logger.LogActivity($"INFO: AddReservation() - Added new reservation successfully. RESERVATION ID: {reservation.id}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database.AddReservation Error: {ex.Message}");
                Program.logger.LogActivity($"ERROR: AddReservation() - Failed to add reservation. ERROR: {ex.Message}");
                return false;
            }
        }
        public bool UpdateReservation(Reservation reservation)
        {
            string query = @"
                UPDATE reservations
                SET owner_id = @ownerId,
                    reservation_date_from = @dateFrom,
                    reservation_date_to = @dateTo
                WHERE id = @reservationId;
            ";

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                connection.Open();

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("reservationId", reservation.id);
                command.Parameters.AddWithValue("ownerId", reservation.owner.id);
                command.Parameters.AddWithValue("dateFrom", reservation.date.timeFrom);
                command.Parameters.AddWithValue("dateTo", reservation.date.timeTo);

                int rowsAffected = command.ExecuteNonQuery();

                string deleteReservablesQuery = @"
                    DELETE FROM reservation_reservables WHERE reservation_id = @reservationId;
                ";

                using var deleteCommand = new NpgsqlCommand(deleteReservablesQuery, connection);
                deleteCommand.Parameters.AddWithValue("reservationId", reservation.id);
                deleteCommand.ExecuteNonQuery();

                foreach (var reservable in reservation.reservables)
                {
                    string reservableQuery = @"
                        INSERT INTO reservation_reservables (reservation_id, reservable_id)
                        VALUES (@reservationId, @reservableId);
                    ";

                    using var reservableCommand = new NpgsqlCommand(reservableQuery, connection);
                    reservableCommand.Parameters.AddWithValue("reservationId", reservation.id);
                    reservableCommand.Parameters.AddWithValue("reservableId", reservable.id);
                    reservableCommand.ExecuteNonQuery();
                }

                Program.logger.LogActivity($"INFO: UpdateReservation() - Updated reservation successfully. RESERVATION ID: {reservation.id}");
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database.UpdateReservation Error: {ex.Message}");
                Program.logger.LogActivity($"ERROR: UpdateReservation() - Failed to update reservation. ERROR: {ex.Message}");
                return false;
            }
        }
        public bool RemoveReservation(Reservation reservation)
        {
            string query = @"
                DELETE FROM reservations
                WHERE id = @reservationId;
            ";

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                connection.Open();

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("reservationId", reservation.id);

                int rowsAffected = command.ExecuteNonQuery();

                Program.logger.LogActivity($"INFO: RemoveReservation() - Removed reservation successfully. RESERVATION ID: {reservation.id}");
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database.RemoveReservation Error: {ex.Message}");
                Program.logger.LogActivity($"ERROR: RemoveReservation() - Failed to remove reservation. ERROR: {ex.Message}");
                return false;
            }
        }
        public List<Activity> GetAllActivities()
        {
            var activities = new List<Activity>();
            string query = @"
                SELECT a.id, a.name, a.description, a.is_open, a.participant_limit, a.instructor_id, a.activity_date_from, a.activity_date_to, a.reservation_id, ap.customer_id FROM activities a
                LEFT JOIN activity_participants ap ON a.id = ap.activity_id;";

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                connection.Open();

                using var command = new NpgsqlCommand(query, connection);
                using var reader = command.ExecuteReader();

                Dictionary<int, Activity> activityMap = new();

                while (reader.Read())
                {
                    int activityId = reader.GetInt32(0);

                    if (!activityMap.ContainsKey(activityId))
                    {
                        var activity = new Activity()
                        {
                            id = activityId,
                            name = reader.GetString(1),
                            description = reader.GetString(2),
                            open = reader.GetBoolean(3),
                            limit = reader.GetInt32(4),
                            instructor = (Staff)User.users.FirstOrDefault(u => u.id == reader.GetInt32(5)),
                            date = new Date(
                                reader.GetDateTime(6),
                                reader.GetDateTime(7)
                                ),
                            reservation = Reservation.reservations.FirstOrDefault(r => r.id == reader.GetInt32(8)),
                            participants = new List<Customer>()
                        };

                        activityMap[activityId] = activity;
                        activities.Add(activity);

                        // TBD: Temporary solution to handling id, change to GUID/UUID
                        if (activity.id >= Activity.getActivityID) Activity.getActivityID = activity.id + 1;
                    }

                    if (!reader.IsDBNull(9))
                    {
                        var customer = (Customer)User.users.FirstOrDefault(c => c.id == reader.GetInt32(9));
                        activityMap[activityId].participants.Add(customer);
                    }
                }

                Program.logger.LogActivity("INFO: GetAllActivities() - Loaded activities data successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database.GetAllActivities Error: {ex.Message}");
                Program.logger.LogActivity($"ERROR: GetAllActivities() - Loaded activities data unsuccessfully. ERROR: {ex.Message}");
            }

            return activities;
        }
        public bool AddActivity(Activity activity)
        {
            string query = @"
                INSERT INTO activities (id, name, description, is_open, participant_limit, instructor_id, activity_date_from, activity_date_to, reservation_id)
                VALUES (@activityId, @name, @description, @isOpen, @limit, @instructorId, @dateFrom, @dateTo, @reservationId);
            ";

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                connection.Open();

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("activityId", activity.id);
                command.Parameters.AddWithValue("name", activity.name);
                command.Parameters.AddWithValue("description", activity.description);
                command.Parameters.AddWithValue("isOpen", activity.open);
                command.Parameters.AddWithValue("limit", activity.limit);
                command.Parameters.AddWithValue("instructorId", activity.instructor.id);
                command.Parameters.AddWithValue("dateFrom", activity.date.timeFrom);
                command.Parameters.AddWithValue("dateTo", activity.date.timeTo);
                command.Parameters.AddWithValue("reservationId", activity.reservation?.id);

                foreach (var participant in activity.participants)
                {
                    string participantQuery = @"
                        INSERT INTO activity_participants (activity_id, customer_id)
                        VALUES (@activityId, @customerId);
                    ";

                    using var participantCommand = new NpgsqlCommand(participantQuery, connection);
                    participantCommand.Parameters.AddWithValue("activityId", activity.id);
                    participantCommand.Parameters.AddWithValue("customerId", participant.id);
                    participantCommand.ExecuteNonQuery();
                }

                command.ExecuteNonQuery();

                Program.logger.LogActivity($"INFO: AddActivity() - Added activity successfully. ACTIVITY ID: {activity.id}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database.AddActivity Error: {ex.Message}");
                Program.logger.LogActivity($"ERROR: AddActivity() - Failed to add activity. ERROR: {ex.Message}");
                return false;
            }
        }
        public bool UpdateActivity(Activity activity)
        {
            string query = @"
                UPDATE activities
                SET name = @name,
                    description = @description,
                    is_open = @isOpen,
                    participant_limit = @limit,
                    instructor_id = @instructorId,
                    activity_date_from = @dateFrom,
                    activity_date_to = @dateTo,
                    reservation_id = @reservationId
                WHERE id = @activityId;
            ";

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                connection.Open();

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("activityId", activity.id);
                command.Parameters.AddWithValue("name", activity.name);
                command.Parameters.AddWithValue("description", activity.description);
                command.Parameters.AddWithValue("isOpen", activity.open);
                command.Parameters.AddWithValue("limit", activity.limit);
                command.Parameters.AddWithValue("instructorId", activity.instructor.id);
                command.Parameters.AddWithValue("dateFrom", activity.date.timeFrom);
                command.Parameters.AddWithValue("dateTo", activity.date.timeTo);
                command.Parameters.AddWithValue("reservationId", activity.reservation?.id);

                int rowsAffected = command.ExecuteNonQuery();

                string deleteParticipantsCommand = @"
                    DELETE FROM activity_participants WHERE activity_id = @activityId;
                ";

                using var deleteCommand = new NpgsqlCommand(deleteParticipantsCommand, connection);
                deleteCommand.Parameters.AddWithValue("activityId", activity.id);
                deleteCommand.ExecuteNonQuery();

                foreach (var participant in activity.participants)
                {
                    string participantQuery = @"
                        INSERT INTO activity_participants (activity_id, customer_id)
                        VALUES (@activityId, @customerId);
                    ";

                    using var participantCommand = new NpgsqlCommand(participantQuery, connection);
                    participantCommand.Parameters.AddWithValue("activityId", activity.id);
                    participantCommand.Parameters.AddWithValue("customerId", participant.id);
                    participantCommand.ExecuteNonQuery();
                }

                Program.logger.LogActivity($"INFO: UpdateActivity() - Updated activity successfully. ACTIVITY ID: {activity.id}");
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database.UpdateActivity Error: {ex.Message}");
                Program.logger.LogActivity($"ERROR: UpdateActivity() - Failed to update activity. ERROR: {ex.Message}");
                return false;
            }
        }
        public bool RemoveActivity(Activity activity)
        {
            string query = @"
                DELETE FROM activities
                WHERE id = @activityId;
            ";

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                connection.Open();

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("activityId", activity.id);

                int rowsAffected = command.ExecuteNonQuery();

                Program.logger.LogActivity($"INFO: RemoveActivity() - Removed activity successfully. ACTIVITY ID: {activity.id}");
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database.RemoveActivity Error: {ex.Message}");
                Program.logger.LogActivity($"ERROR: RemoveActivity() - Failed to remove activity. ERROR: {ex.Message}");
                return false;
            }
        }
        #endregion

        #region Private Methods
        private void CreateTables()
        {
            CreateUserTable();
            CreateReservablesTable();
            CreateReservationsTable();
            CreateActivitiesTables();
        }
        private void CreateUserTable()
        {
            // TBD: Separate user tables according to user type
            try
            {
                // TBD: Use UUID as PK and assign ID automatically in database
                string query = @"
                CREATE TABLE IF NOT EXISTS users (
                    id INTEGER PRIMARY KEY,
                    first_name VARCHAR(100) NOT NULL,
                    last_name VARCHAR(100) NOT NULL,
                    ssn CHAR(15) UNIQUE NOT NULL,
                    phone VARCHAR(20) UNIQUE,
                    email VARCHAR(255) UNIQUE,
                    login_name VARCHAR(100) UNIQUE NOT NULL,
                    login_password TEXT NOT NULL,
                    user_type VARCHAR(10) CHECK (user_type IN ('admin', 'staff', 'customer')) NOT NULL,
                    sub_start TIMESTAMP DEFAULT NULL,
                    sub_end TIMESTAMP DEFAULT NULL,
                    is_sub BOOLEAN DEFAULT FALSE,
                    is_guest BOOLEAN DEFAULT FALSE
                );";

                ExecuteNonQuery(query);
                Program.logger.LogActivity("INFO: CreateUserTable() - Table created successfully.");
            }
            catch (Exception ex)
            {
                Program.logger.LogActivity($"Database.CreateUserTables Error: {ex.Message}");
                Console.WriteLine($"Database.CreateUserTables Error: {ex.Message}");
            }
        }
        private void CreateReservablesTable()
        {
            // TBD: Separate reservable tables according to reservable type
            try
            {
                string query = @"
                CREATE TABLE IF NOT EXISTS reservables (
                    id INTEGER PRIMARY KEY,
                    name VARCHAR(100) NOT NULL,
                    description TEXT,
                    is_available BOOLEAN DEFAULT TRUE,
                    reservable_type VARCHAR(10) CHECK (reservable_type IN ('equipment', 'space', 'ptrainer')) NOT NULL,
                    members_only BOOLEAN DEFAULT TRUE,
                    capacity INTEGER, 
                    instructor_id INTEGER REFERENCES users(id) ON DELETE CASCADE
                );";

                ExecuteNonQuery(query);
                Program.logger.LogActivity("INFO: CreateReservablesTable() - Table created successfully.");
            }
            catch (Exception ex)
            {
                Program.logger.LogActivity($"Database.CreateReservablesTable Error: {ex.Message}");
                Console.WriteLine($"Database.CreateReservablesTable Error: {ex.Message}");
            }
        }
        private void CreateReservationsTable()
        {
            try
            {
                string query = @"
                CREATE TABLE IF NOT EXISTS reservations (
                    id INTEGER PRIMARY KEY,
                    owner_id INTEGER REFERENCES users(id) ON DELETE CASCADE,
                    reservation_date_from TIMESTAMP NOT NULL,
                    reservation_date_to TIMESTAMP NOT NULL
                );

                CREATE TABLE IF NOT EXISTS reservation_reservables (
                    reservation_id INTEGER NOT NULL REFERENCES reservations(id) ON DELETE CASCADE,
                    reservable_id INTEGER NOT NULL REFERENCES reservables(id) ON DELETE CASCADE,
                    PRIMARY KEY (reservation_id, reservable_id)
                );";

                ExecuteNonQuery(query);
                Program.logger.LogActivity("INFO: CreateReservationsTable() - Tables created successfully.");
            }
            catch (Exception ex)
            {
                Program.logger.LogActivity($"Database.CreateReservationsTable Error: {ex.Message}");
                Console.WriteLine($"Database.CreateReservationsTable Error: {ex.Message}");
            }
        }
        private void CreateActivitiesTables()
        {
            try
            {
                string query = @"
                CREATE TABLE IF NOT EXISTS activities (
                    id INTEGER PRIMARY KEY,
                    name VARCHAR(100) NOT NULL,
                    description TEXT,
                    is_open BOOLEAN DEFAULT TRUE,
                    participant_limit INTEGER,
                    instructor_id INTEGER REFERENCES users(id) ON DELETE CASCADE,
                    activity_date_from TIMESTAMP NOT NULL,
                    activity_date_to TIMESTAMP NOT NULL,
                    reservation_id INTEGER REFERENCES reservations(id) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS activity_participants (
                    activity_id INTEGER REFERENCES activities(id) ON DELETE CASCADE,
                    customer_id INTEGER REFERENCES users(id) ON DELETE CASCADE,
                    PRIMARY KEY (activity_id, customer_id)
                );";

                ExecuteNonQuery(query);
                Program.logger.LogActivity("INFO: CreateActivitiesTables() - Tables created successfully.");
            }
            catch (Exception ex)
            {
                Program.logger.LogActivity($"Database.CreateActivitiesTables Error: {ex.Message}");
                Console.WriteLine($"Database.CreateActivitiesTables Error: {ex.Message}");
            }
        }
        private void SetConnectionString()
        {
            if (_connectionString != null && _connectionInfo.Count > 0) return;

            if (File.Exists(_configFilePath))
            {
                string jsonContent = File.ReadAllText(_configFilePath);

                if (!string.IsNullOrEmpty(jsonContent))
                {
                    _connectionInfo = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);
                }
            }
            else
            {
                Console.WriteLine("<< GYM BOOKING MANAGER >>");
                Console.WriteLine("<< DATABASE CONFIGURATION >>\n");
                Console.WriteLine($">> No config file found '{_configFilePath}'");
                Console.WriteLine($">> Set database attributes for PostgreSQL:");

                foreach (var kvp in _connectionInfo)
                {
                    Console.Write($">> {kvp.Key}: ");
                    _connectionInfo[kvp.Key] = Console.ReadLine();
                }

                string jsonContent = JsonSerializer.Serialize(_connectionInfo, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(_configFilePath, jsonContent);

                Console.WriteLine("\n>> New database config file created!");
                Console.WriteLine(">> If attributes are set incorrectly, delete config file and restart application.");
                Console.Write(">> Press any key to continue...");
                Console.ReadKey(true);
                Console.Clear();
            }

            _connectionString = $"Host={_connectionInfo["Host"]};" +
                                $"Port={_connectionInfo["Port"]};" +
                                $"User Id={_connectionInfo["User Id"]};" +
                                $"Password={_connectionInfo["Password"]};" +
                                $"Database={_connectionInfo["Database"]};";
        }
        private void ExecuteNonQuery(string query)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                connection.Open();

                using var command = new NpgsqlCommand(query, connection);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"QUERY ERROR: {ex.Message}");
            }
        }
        #endregion
    }
}