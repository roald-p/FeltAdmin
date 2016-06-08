using FeltAdmin.Database.API;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FeltAdminCommon.Leon
{
    public class FinishedPerson : IDatabaseObject
    {
        public string Name { get; set; }

        public int ShooterId { get; set; }

        public int Team { get; set; }

        public int Target { get; set; }

        [XmlIgnore]
        public TableName TableName
        {
            get { return TableName.FinishedShooter; }
        }

        [XmlIgnore]
        public Dictionary<int, ColumnInfo> RowInfo
        {
            get
            {
                var row = new Dictionary<int, ColumnInfo>();
                row.Add(0, new ColumnInfo { ColumnName = "Name", ColumnValue = Name });
                row.Add(1, new ColumnInfo { ColumnName = "ShooterId", ColumnValue = ShooterId });
                row.Add(6, new ColumnInfo { ColumnName = "Target", ColumnValue = Target });
                row.Add(7, new ColumnInfo { ColumnName = "Team", ColumnValue = Team });
                return row;
            }
        }

        public static FinishedPerson Parse(string keyvaluePairs)
        {
            var result = new FinishedPerson();
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
