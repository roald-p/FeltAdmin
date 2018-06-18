using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using FeltAdmin.Database.API;
using FeltAdmin.Leon;
using FeltAdmin.Orion;

namespace FeltAdminCommon.Leon
{
    public class LeonFinalRegistration : IDatabaseObject
    {
        public string Name { get; set; }

        public string ClubName { get; set; }

        public int ShooterId { get; set; }

        public string Class { get; set; }

        public int Range { get; set; }

        public int Team { get; set; }

        public int Target { get; set; }

        public int SumIn { get; set; }

        public LeonFinalRegistration(LeonPerson lp)
        {
            Name = lp.Name;
            ClubName = lp.ClubName;
            ShooterId = lp.ShooterId;
            Class = lp.Class;
            Range = lp.Range;
            Team = lp.Team;
            Target = lp.Target;
            SumIn = lp.SumIn;
        }

        public LeonFinalRegistration()
        {}

        [XmlIgnore]
        public bool IsEmpty
        {
            get
            {
                return string.IsNullOrWhiteSpace(Name) && ShooterId == 0;
            }
        }

        [XmlIgnore]
        public TableName TableName
        {
            get
            {
                return TableName.LeonFinalRegistration;
            }
        }

        [XmlIgnore]
        public Dictionary<int, ColumnInfo> RowInfo
        {
            get
            {
                var row = new Dictionary<int, ColumnInfo>();
                row.Add(0, new ColumnInfo { ColumnName = "Name", ColumnValue = Name });
                row.Add(1, new ColumnInfo { ColumnName = "ShooterId", ColumnValue = ShooterId });
                row.Add(2, new ColumnInfo { ColumnName = "Class", ColumnValue = Class });
                row.Add(3, new ColumnInfo { ColumnName = "ClubName", ColumnValue = ClubName });
                row.Add(4, new ColumnInfo { ColumnName = "Range", ColumnValue = Range });
                row.Add(5, new ColumnInfo { ColumnName = "SumIn", ColumnValue = SumIn });
                row.Add(6, new ColumnInfo { ColumnName = "Target", ColumnValue = Target });
                row.Add(7, new ColumnInfo { ColumnName = "Team", ColumnValue = Team });

                return row;
            }
        }
        public static LeonFinalRegistration Parse(string keyvaluePairs)
        {
            var result = new LeonFinalRegistration();
            var tuples = keyvaluePairs.Split(';');
            foreach (var tuple in tuples)
            {
                var keyvaluePair = tuple.Split('=');
                switch (keyvaluePair[0].Trim())
                {
                    case "Name":
                        result.Name = keyvaluePair[1].Trim();
                        break;
                    case "ShooterId":
                        result.ShooterId = int.Parse(keyvaluePair[1].Trim());
                        break;
                    case "Class":
                        result.Class = keyvaluePair[1].Trim();
                        break;
                    case "ClubName":
                        result.ClubName = keyvaluePair[1].Trim();
                        break;
                    case "Range":
                        result.Range = int.Parse(keyvaluePair[1].Trim());
                        break;
                    case "SumIn":
                        result.SumIn = int.Parse(keyvaluePair[1].Trim());
                        break;
                    case "Target":
                        result.Target = int.Parse(keyvaluePair[1].Trim());
                        break;
                    case "Team":
                        result.Team = int.Parse(keyvaluePair[1].Trim());
                        break;
                }
            }

            return result;
        }

        public OrionRegistration GetOrionRegistration(int orionId, int sumIn)
        {
            var result = new OrionRegistration();
            result.ShooterId = ShooterId;
            result.Class = Class;
            result.ClubName = ClubName;
            result.Name = Name;
            result.OrionId = orionId;
            result.RangeId = Range;
            result.SumIn = sumIn;
            result.Target = Target;
            result.Team = Team;
            return result;
        }
    }
}
