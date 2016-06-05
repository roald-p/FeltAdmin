using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

using FeltAdminCommon;

using FeltAdminServer.Data;

namespace FeltAdminServer
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IFestResultService
    {
        [OperationContract]
        List<string> GetCompetitions(RangeClass rangeClass);

        [OperationContract]
        List<Result> GetResults(RangeClass rangeClass, string competition);

        [OperationContract]
        List<Registration> GetRegistrations(RangeClass rangeClass, string competition);
    }
}
