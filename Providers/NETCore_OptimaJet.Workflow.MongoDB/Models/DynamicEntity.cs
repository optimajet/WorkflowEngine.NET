using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimaJet.Workflow.MongoDB
{
    public abstract class DynamicEntity
    {
        [BsonExtraElements]
        public BsonDocument ExtraElements { get; set; }
    }
}
