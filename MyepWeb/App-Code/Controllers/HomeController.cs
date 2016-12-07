using System;
using System.Web.Mvc;

namespace Site
{
    public class HomeController : Controller
    {
        private readonly SiteDb _db;
        private readonly Interns _interns;
        private readonly Users _users;

        public HomeController(SiteDb db, Interns interns, Users users)
        {
            _db = db;
            _interns = interns;
            _users = users;
        }

        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult App(string id)
        {
            //If they didn't pass us an id, they need to enter one
            if (!id.HasValue())
                return View("NoId");

            //we're allowing them to create a new application with their CmsStudentId
            var model = _interns.LoadByCode(id) ?? _interns.Create(id);

            //they can only submit their application once
            if (model.Submitted != null && !Request.IsLocal)
                return Content("App for code '" + id + "' submitted on '" + model.Submitted + "'", "text/plain");

            return View(model);
        }

        [AllowAnonymous, HttpPost]
        public ActionResult App(FormCollection form)
        {
            var cmsStudentId = form["CmsStudentId"];
            //we're allowing them to create a new application with their CmsStudentId
            var model = _interns.LoadByCode(cmsStudentId) ?? _interns.Create(cmsStudentId);

            try
            {
                if (TryUpdateModel(model, null, null, new[] { "CmsStudentId", "Essay", "Resume", "Recommendation1", "Recommendation2" }))
                {
                    model.Submitted = DateTime.Now;
                    _interns.Save(model); //we need an ID for the files below
                    _interns.UploadEssay(model, Request.Files["Essay"]);
                    _interns.UploadResume(model, Request.Files["Resume"]);
                    _interns.UploadRecommendation1(model, Request.Files["Recommendation1"]);
                    _interns.UploadRecommendation2(model, Request.Files["Recommendation2"]);
                    _interns.Save(model);
                    return Content("Application Submitted", "text/plain");
                }
            }
            catch (Exception ex)
            {
                ViewData.Error(ex);
            }
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            var user = new User();
            return View(user);
        }

        [AllowAnonymous, AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Login(User user)
        {
            try
            {
                if (!_users.Login(user.Email, user.Password, true))
                {
                    throw new Exception("Invalid login");
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }
            return View(user);
        }

        public ActionResult Logout()
        {
            _users.Logout();
            return Redirect(Request.UrlReferrer == null ? "/" : Request.UrlReferrer.ToString());
        }

        //[AllowAnonymous]
        //public ActionResult Reset(string id)
        //{
        //    var model = new User();
        //    return View(model);
        //}

        //[AllowAnonymous, AcceptVerbs(HttpVerbs.Post)]
        //public ActionResult Reset(string id, FormCollection form)
        //{
        //    var model = _users.LoadByCode(id);
        //    if (model == null)
        //        throw new ApplicationException("Reset code not found: " + id);

        //    //if (!TryUpdateModel(model, null, null, new[] {"id"}))
        //    //	throw new ApplicationException("Error updating password: " + ViewData.ModelState.Values.First().Errors.First().ErrorMessage);

        //    _users.ResetPassword(model, form["Password"]);
        //    return Redirect("/");
        //}

        //public ActionResult Migrate(bool? execute)
        //{
        //    var schema = _db.GetSchema();
        //    if (execute == true)
        //    {
        //        schema.Execute();
        //        return Content("Migrated", "text/plain");
        //    }
        //    else
        //    {
        //        return Content(schema.GenerateSql(), "text/plain");
        //    }
        //}
    };
}
