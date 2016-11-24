using System;
using System.Web.Mvc;

namespace Site
{
    [Authorize]
    public class InternsController : Controller
    {
        private readonly Interns _interns;

        public InternsController(Interns interns)
        {
            _interns = interns;
        }

        public ActionResult Index(bool? inactive = false)
        {
            ViewBag.Inactive = inactive == true;
            var model = _interns.Query(inactive);
            return View("ListInterns", model);
        }

        public ActionResult Edit(int? id)
        {
            var model = _interns.Load(id) ?? _interns.Create(null);
            return View("EditIntern", model);
        }

        [HttpPost]
        public ActionResult Edit(int? id, FormCollection form)
        {
            var model = _interns.Load(id) ?? _interns.Create(null);
            try
            {
                if (TryUpdateModel(model))
                {
                    _interns.Save(model);
                    return Redirect("/interns");
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.ToString();
            }
            return View("EditIntern", model);
        }

        public ActionResult Download(int id, string name)
        {
            var model = _interns.Load(id);
            var path = "~/App_Data/Interns/" + model.Id + "/" + name;
            return File(path, "application/octet-stream", name);
        }

        public ActionResult ViewInternDoc(int id)
        {
            var model = _interns.Load(id);
            var filename = model.Id + " " + model.FirstName + " " + model.LastName + ".docx";
            var bytes = new ViewInternDoc().Generate(model);
            return File(bytes, "application/octet-stream", filename);
        }

        [HttpPost]
        public ActionResult Import()
        {
            var ret = _interns.ImportSchoolFile(Request.Files["SchoolFile"]);
            return Content("Imported: " + ret, "text/plain");
            //return Redirect("/interns");
        }

        public ActionResult Export()
        {
            var export = _interns.Export();
            return File(export, "application/octet-stream", "Interns.xls");
        }
    };
}
