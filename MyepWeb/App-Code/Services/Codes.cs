using System.Collections.Generic;
using System.Linq;

namespace Site
{
    public class Codes
    {
        private readonly SiteDb _db;

        public Codes(SiteDb db)
        {
            _db = db;
        }

        public List<string> GetTypes()
        {
            return _db.Query<string>("SELECT DISTINCT [Type] FROM [Codes] ORDER BY [Type]").ToList();
        }

        public List<Code> Query(string type, bool? active = null)
        {
            var sql = "SELECT * FROM [Codes] WHERE [Type]=@0";
            if (active == true) sql += " AND Seq>=0 ";
            return _db.Query<Code>(sql + " ORDER BY [Seq],[Value]", type).ToList();
        }

        public void Save(Code model)
        {
            _db.Save("Codes", "Id", model);
        }
    };
}
