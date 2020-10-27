using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace SecurePassword
{
    class Program
    {
        // Server=localhost;Database=master;Trusted_Connection=True;

        // Globel Consts
        public const int SALT_SIZE = 24;
        public const int HASH_SIZE = 24;
        public const int ITERATIONS = 100000;


        static void Main(string[] args)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("==== Menu ====");
                Console.WriteLine();
                Console.WriteLine("1: - Create User\n");
                Console.WriteLine("2: - Login");

                string userinput = Console.ReadLine();

                switch (userinput)
                {
                    case "1":           // User Creation
                        Console.Clear();
                        Console.WriteLine("==== Create User ====\n");
                        Console.WriteLine();
                        Console.WriteLine("Chose A Username:");
                        string usernameInput = Console.ReadLine();
                        Console.WriteLine("\n Chose A Password:");
                        string passwordInput = Console.ReadLine();

                        CreateUser(usernameInput, passwordInput);

                        Console.Clear();
                        Console.WriteLine("User Created");
                        Thread.Sleep(2000);

                        break;
                    case "2":           // User Login
                        Console.Clear();
                        Console.WriteLine("==== Login ====\n");
                        Console.WriteLine();
                        Console.WriteLine("Username:");
                        string usernameLogin = Console.ReadLine();
                        Console.WriteLine("Password:");
                        string passwordLogin = Console.ReadLine();

                        UserLogin(usernameLogin, passwordLogin);

                        break;
                }

            }

        }
        private static void CreateUser(string usernameInput, string passwordInput)
        {
            string username = usernameInput;

            Insert(username, passwordInput);
        }

        private static void Insert(string uName, string password)
        {
            var userSalt = Salt();
            var passwordHashed = CreateHash(password, Convert.ToBase64String(userSalt));

            string connectionString = @"Server = ZBC-E-CH-SKP024; Database = SecurePasswordDB; Trusted_Connection = True";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO UserData VALUES ('" + uName + "', '" + Convert.ToBase64String(passwordHashed) + "', '" + Convert.ToBase64String(userSalt) + "')", conn);
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        private static void UserLogin(string usernameLogin, string passwordLogin)
        {
            string username = usernameLogin;

            var passHolder = "";
            var saltHolder = "";

            string connectionString = @"Server = ZBC-E-CH-SKP024; Database = SecurePasswordDB; Trusted_Connection = True";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT HashPass,PassKey FROM UserData WHERE Username='" + username + "'", conn);

                SqlDataReader read = cmd.ExecuteReader();

                while (read.Read())
                {
                    passHolder = (read["HashPass"].ToString());
                    saltHolder = (read["PassKey"].ToString());
                }
                read.Close();
                conn.Close();

            }

            var passwordHashed = CreateHash(passwordLogin, saltHolder);
            if (passHolder == Convert.ToBase64String(passwordHashed))
            {
                Console.WriteLine("KIMOCHIIIIII");
                Console.ReadKey();
            }
            else
            {
                // error code -- make a count for wrong atempts before locking user
                Console.WriteLine("wrong Password");
                Console.ReadKey();
            }


        }

        private static byte[] CreateHash(string input, string salt)
        {
            byte[] byteInput = Encoding.UTF8.GetBytes(input);
            byte[] byteSalt = Encoding.UTF8.GetBytes(salt);
            // Hashing
            Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(byteInput, byteSalt, ITERATIONS);
            return pbkdf2.GetBytes(HASH_SIZE);
        }

        private static byte[] Salt()
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] salt = new byte[SALT_SIZE];
            rng.GetBytes(salt);
            return salt;
        }
    }
}
