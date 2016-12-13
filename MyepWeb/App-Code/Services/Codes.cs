using System.Collections.Generic;
using System.Linq;

namespace Site
{
    public class Codes
    {
        private readonly IDb _db;

        public Codes(IDb db)
        {
            _db = db;
        }

        public List<string> GetTypes()
        {
            return _db
                .Query<Code>()
                .Select(x => x.Type)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }

        public List<Code> Query(string type, bool? active = null)
        {
            var query = _db
                .Query<Code>()
                .Where(x => x.Type == type);

            if (active == true)
            {
                query = query.Where(x => x.Seq >= 0);
            }

            return query
                .OrderBy(x => x.Seq)
                .ThenBy(x => x.Value)
                .ToList();
        }

        public void Save(Code model)
        {
            _db.Save(model, model.Id == 0);
            _db.SaveChanges();
        }
    };
}
