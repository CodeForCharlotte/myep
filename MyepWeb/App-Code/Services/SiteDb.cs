using PetaPoco;
using PetaSchema;

namespace Site
{
    public class SiteDb : Database
    {
        public SiteDb() : base("SiteDb") { }

        public DbSchema GetSchema()
        {
            var schema = new DbSchema(this);
            Ext.ImplementorsOf<IEntity>().ForEach(schema.AddTable);
            return schema;
        }

    };

    public interface IEntity
    {
        //placeholder interface for database entities
    }
}
