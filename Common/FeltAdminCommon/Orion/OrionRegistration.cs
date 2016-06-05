using FeltAdmin.Viewmodels;

namespace FeltAdmin.Orion
{
    public class OrionRegistration
    {
        public int OrionId { get; set; }

        public int Team { get; set; }

        public int Target { get; set; }

        public string Name { get; set; }

        public string ClubName { get; set; }

        public string Class { get; set; }

        public int SumIn { get; set; }

        public int ShooterId { get; set; }

        public int RangeId { get; set; }

        public string RangeName { get; set; }

        public bool Deleted { get; set; }

        public bool RemoveReg { get; set; }

        public string OrionInput
        {
            get
            {
                if (Deleted)
                {
                    return string.Format("{0};{1};{2};;;;;", OrionId, Team, Target, Name, ClubName, Class, SumIn, ShooterId);
                }

                return string.Format("{0};{1};{2};{3} ;{4};{5};{6};{7}", OrionId, Team, Target, Name, ClubName, Class, SumIn, ShooterId);
            }
        }
    }
}
