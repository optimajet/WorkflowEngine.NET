using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OptimaJet.Workflow.MongoDB
{
    public abstract class DynamicEntity
    {
        [BsonExtraElements]
        public BsonDocument ExtraElements { get; set; }
    }
}
