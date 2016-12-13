using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Site
{
    public class Interns
    {
        private readonly IDb _db;

        public Interns(IDb db)
        {
            _db = db;
        }

        public List<InternInfo> Query(bool? inactive = false)
        {
            var query = _db.Query<Intern>();

            if (inactive != true)
            {
                query = query.Where(x => x.Inactive == false);
            }

            return query
                .Select(x => new InternInfo
                {
                    InternId = x.Id,
                    FullName = x.FirstName + " " + x.LastName,
                })
                .ToList();
        }

        public Intern Load(int? id)
        {
            if (!id.HasValue)
                return null;

            return _db
                .Query<Intern>()
                .SingleOrDefault(x => x.Id == id);
        }

        public Intern LoadByCode(string code)
        {
            if (!code.HasValue())
                return null;

            return _db
                .Query<Intern>()
                .SingleOrDefault(x => x.CmsStudentId == code || x.AccessCode == code);
        }

        public Intern LoadByCmsId(string id)
        {
            if (!id.HasValue())
                return null;

            return _db
                .Query<Intern>()
                .SingleOrDefault(x => x.CmsStudentId == id);
        }

        public Intern Create(string cmsStudentId)
        {
            if (true || !Web.IsLocal)
                return new Intern
                {
                    CmsStudentId = cmsStudentId,
                };

            return new Intern
            {
                CmsStudentId = cmsStudentId,
                FirstName = "Paul",
                LastName = "Wheeler",
                MiddleInitial = "J",
                Address1 = "4067 Sherri Ln",
                City = "Fort Mill",
                State = "SC",
                Zip = "29715",
                Phone = "803-818-2200",
                Email = "paul@tagovi.com",
                BirthDate = new DateTime(1977, 1, 1),
                ParentName = "DC Wheeler",
                ParentAddress1 = "4067 Sherri Ln",
                ParentCity = "Fort Mill",
                ParentState = "SC",
                ParentZip = "29715",
                ParentPhone = "803-818-2200",
                ParentEmail = "paul@tagovi.com",
            };
        }

        public void Save(Intern model)
        {
            if (!model.AccessCode.HasValue())
                model.AccessCode = Guid.NewGuid().ToString("N").Substring(0, 10);

            if (model.RaceOther.HasValue() && !model.Race.Or().Contains("Other"))
                model.Race = string.Join(",", model.Race.Or().Split(',').Union(new[] { "Other" }).Distinct());

            _db.Save(model, model.Id == 0);
            _db.SaveChanges();
        }

        public void UploadEssay(Intern model, HttpPostedFileBase file)
        {
            if (model == null || file == null || file.ContentLength == 0 || file.InputStream == null)
                return;

            var ext = Path.GetExtension(file.FileName);
            model.EssayFileName = "Essay" + ext;
            var path = Web.MapPath("~/App_Data/Interns/" + model.Id + "/" + model.EssayFileName);
            File.WriteAllBytes(path, file.InputStream.ReadAllBytes());
        }

        public void UploadResume(Intern model, HttpPostedFileBase file)
        {
            if (model == null || file == null || file.ContentLength == 0 || file.InputStream == null)
                return;

            var ext = Path.GetExtension(file.FileName);
            model.ResumeFileName = "Resume" + ext;
            var path = Web.MapPath("~/App_Data/Interns/" + model.Id + "/" + model.ResumeFileName);
            File.WriteAllBytes(path, file.InputStream.ReadAllBytes());
        }

        public void UploadRecommendation1(Intern model, HttpPostedFileBase file)
        {
            if (model == null || file == null || file.ContentLength == 0 || file.InputStream == null)
                return;

            var ext = Path.GetExtension(file.FileName);
            model.RecommendationFileName1 = "Recommendation1" + ext;
            var path = Web.MapPath("~/App_Data/Interns/" + model.Id + "/" + model.RecommendationFileName1);
            File.WriteAllBytes(path, file.InputStream.ReadAllBytes());
        }

        public void UploadRecommendation2(Intern model, HttpPostedFileBase file)
        {
            if (model == null || file == null || file.ContentLength == 0 || file.InputStream == null)
                return;

            var ext = Path.GetExtension(file.FileName);
            model.RecommendationFileName2 = "Recommendation2" + ext;
            var path = Web.MapPath("~/App_Data/Interns/" + model.Id + "/" + model.RecommendationFileName2);
            File.WriteAllBytes(path, file.InputStream.ReadAllBytes());
        }

        public int ImportSchoolFile(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0 || file.InputStream == null)
                return 0;

            var added = 0;
            using (var excel = new Excel(file.InputStream))
            {
                //Find columns that map to properties
                var props = new List<PropertyInfo>();
                foreach (var col in 1.UpTo(excel.Cols))
                {
                    var prop = typeof(Intern).GetProperty(excel[1, col].Or());
                    if (prop != null && prop.Name == "Id")
                        props.Add(null); //can't update Ids
                    else
                        props.Add(prop);
                }

                var list = excel.GetList<Intern>();
                foreach (var item in list)
                {
                    if (!item.CmsStudentId.HasValue()) continue; //need id to import

                    var intern = LoadByCmsId(item.CmsStudentId);
                    if (intern != null) continue;
                    intern = Create(null);

                    //only update properties from file
                    foreach (var c in 0.UpTo(props.Count - 1))
                    {
                        if (props[c] != null) intern.Set(props[c], item.Get(props[c]));
                    }

                    Save(intern);
                    added++;
                }
            }

            return added;
        }

        public Stream Export()
        {
            var interns = _db.Query<Intern>().ToList();
            using (var excel = Excel.Create(interns))
            {
                return excel.GetStream();
            }
        }
    };

    public class InternInfo
    {
        public int InternId { get; set; }
        public string FullName { get; set; }
        public int? EmployerId { get; set; }
        public string Organization { get; set; }
    };
}
