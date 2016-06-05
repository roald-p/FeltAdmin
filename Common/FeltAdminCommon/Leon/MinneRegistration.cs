using FeltAdmin.Database.API;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FeltAdmin.Leon
{
    public class MinneRegistration : IDatabaseObject
    {
        public string Name { get; set; }

        public string ClubName { get; set; }

        public int ShooterId { get; set; }

        public string Class { get; set; }

        public int Range { get; set; }

        public int Team { get; set; }

        public int Target { get; set; }

        public int SumIn { get; set; }

        [XmlIgnore]
        public string Key
        {
            get
            {
                return string.Join("+", Range, Team, Target);
            }
        }

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
                return TableName.MinneRegistration;
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

        public static MinneRegistration Parse(string keyvaluePairs)
        {
            var result = new MinneRegistration();
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
    }
}
