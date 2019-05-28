using FeltAdmin.Database.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FeltAdminCommon.Orion
{
    public class OrionNewTarget : IDatabaseObject
    {
        [XmlIgnore]
        public TableName TableName
        {
            get { return TableName.OrionNewTarget; }
        }

        [XmlIgnore]
        public Dictionary<int, ColumnInfo> RowInfo
        {
            get
            {
                var row = new Dictionary<int, ColumnInfo>();
                row.Add(0, new ColumnInfo { ColumnName = "OriginalOrionId", ColumnValue = OriginalOrionId });
                row.Add(1, new ColumnInfo { ColumnName = "OriginalTarget", ColumnValue = OriginalTarget });
                row.Add(2, new ColumnInfo { ColumnName = "NewOrionId", ColumnValue = NewOrionId });
                row.Add(3, new ColumnInfo { ColumnName = "NewTarget", ColumnValue = NewTarget });
                return row;
            }
        }

        public static OrionNewTarget Parse(string keyvaluePairs)
        {
            var result = new OrionNewTarget();
            var tuples = keyvaluePairs.Split(';');
            foreach (var tuple in tuples)
            {
                var keyvaluePair = tuple.Split('=');
                switch (keyvaluePair[0].Trim())
                {
                    case "OriginalOrionId":
                        result.OriginalOrionId = int.Parse(keyvaluePair[1].Trim());
                        break;
                    case "OriginalTarget":
                        result.OriginalTarget = int.Parse(keyvaluePair[1].Trim());
                        break;
                    case "NewOrionId":
                        result.NewOrionId = int.Parse(keyvaluePair[1].Trim());
                        break;
                    case "NewTarget":
                        result.NewTarget = int.Parse(keyvaluePair[1].Trim());
                        break;
                }
            }

            return result;
        }

        public int OriginalOrionId { get; set; }
        public int OriginalTarget { get; set; }
        public int NewOrionId { get; set; }
        public int NewTarget { get; set; }
    }
}
