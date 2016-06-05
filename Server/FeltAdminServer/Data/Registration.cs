
using FeltAdmin.Leon;

namespace FeltAdminServer.Data
{
    public class Registration
    {
        public int ShooterId { get; set; }

        public string Name { get; set; }

        public string ClubName { get; set; }

        public string Class { get; set; }

        public int Team { get; set; }

        public int Target { get; set; }

        public Result Result { get; set; }

        public static Registration GetRegistration(LeonPerson leonRegistration)
        {
            return new Registration
            {
                Class = leonRegistration.Class,
                ClubName = leonRegistration.ClubName,
                Name = leonRegistration.Name,
                ShooterId = leonRegistration.ShooterId,
                Team = leonRegistration.Team,
                Target = leonRegistration.Target
            };
        }
    }
}