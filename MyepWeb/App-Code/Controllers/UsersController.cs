using System;
using System.Web.Mvc;

namespace Site
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly Users _users;

        public UsersController(Users users)
        {
            _users = users;
        }

        public ActionResult Index()
        {
            var model = _users.Query();
            return View("ListUsers", model);
        }

        public ActionResult Edit(int? id)
        {
            var model = _users.Load(id) ?? _users.Create();
            return View("EditUser", model);
        }

        [HttpPost]
        public ActionResult Edit(int? id, FormCollection form)
        {
            var model = _users.Load(id) ?? _users.Create();
            try
            {
                if (TryUpdateModel(model))
                {
                    _users.Save(model);
                    if (form["_submit"] == "EmailLogin")
                    {
                        _users.EmailLoginInfo(model);
                    }
                    return Redirect("/users");
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.ToString();

            }
            return View("EditUser", model);
        }
    };
}
