using System.Collections.Generic;
using System.Web.Mvc;

namespace Site
{
    [Authorize]
    public class CodesController : Controller
    {
        private readonly Codes _codes;

        public CodesController(Codes codes)
        {
            _codes = codes;
        }

        public ActionResult Index(string type)
        {
            ViewBag.Types = new SelectList(_codes.GetTypes(), type);
            var model = _codes.Query(type);
            return View("ListCodes", model);
        }

        [HttpPost]
        public ActionResult Index(string type, List<Code> codes)
        {
            foreach (var code in codes)
            {
                if (!string.IsNullOrWhiteSpace(code.Value))
                    _codes.Save(code);
            }
            return Redirect("/codes?Type=" + type);
        }
    };
}
