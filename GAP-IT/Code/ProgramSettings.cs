using System.Security;

namespace GAP_IT.Code
{
    public class ProgramSettings
    {
        public static int Days { get; set; }
        public static bool Resolve { get; set; }
        public static string Protocol { get; set; }
        public static string Address { get; set; }
        public static int Port { get; set; }
        public static bool Remote { get; set; }
        public static bool Remember { get; set; }
        public static string Machine { get; set; }
        public static string Username { get; set; }
        public static SecureString Password { get; set; }
        public static string Domain { get; set; }
    }
}

