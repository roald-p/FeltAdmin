using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace FeltAdminServer.Data
{
    [DataContract]
    public class Result
    {
        [DataMember]
        public int ShooterId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Class { get; set; }

        [DataMember]
        public string ClubName { get; set; }

        [DataMember]
        public string TotalResult { get; set; }

        [DataMember]
        public int TotalSum { get; set; }

        [DataMember]
        public int TotalInnerHits { get; set; }

        [DataMember]
        public List<FeltHold> FeltHolds { get; set; }

        [DataMember]
        public int Minne { get; set; }

        [DataMember]
        public int MinneInner { get; set; }
    }
}