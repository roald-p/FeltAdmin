using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeltAdmin.Database.API
{
    public interface IDatabaseObject
    {
        TableName TableName { get; }

        Dictionary<int, ColumnInfo> RowInfo { get; }
    }
}
