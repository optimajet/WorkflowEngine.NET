using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using System.Text;

namespace Common.WCF
{
    [DataContract]
    public class BaseFault
    {
        [DataMember(IsRequired = true)]
        public int ErrorCode { get; set; }

        [DataMember]
        public string Description { get; set; }

        public BaseFault (int errorCode)
        {
            ErrorCode = errorCode;
        }
    }
}
