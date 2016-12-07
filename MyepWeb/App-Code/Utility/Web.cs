using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Site
{
    public static class Web
    {
        public static HttpApplication Application
        {
            get { return HttpContext.Current.ApplicationInstance; }
        }
        public static HttpRequest Request
        {
            get { return Application.Request; }
        }

        public static string ServerName
        {
            get { return Request.ServerVariables["SERVER_NAME"]; }
        }

        public static string ServerPort
        {
            get
            {
                var Port = Request.ServerVariables["SERVER_PORT"];
                if (Port == null || Port == "80" || Port == "443")
                    return "";
                else
                    return ":" + Port;
            }
        }

        public static string ServerProtocol
        {
            get
            {
                var Protocol = Request.ServerVariables["SERVER_PORT_SECURE"];
                if (Protocol == null || Protocol == "0")
                    return "http://";
                else
                    return "https://";
            }
        }

        /// <summary>Does NOT return / at end</summary>
        public static string ServerPath
        {
            get { return ServerProtocol + ServerName + ServerPort; }
        }

        public static bool IsLocal
        {
            get { return Request.IsLocal; }
        }

        public static IEnumerable<SelectListItem> ToSelectList(this IEnumerable<string> items, bool blank = false, params string[] selected)
        {
            var list = new List<SelectListItem>();

            if (items == null)
                return list;

            list.AddRange(items.Where(i => i != null).Select(item => new SelectListItem
            {
                Value = item,
                Text = item,
                Selected = selected.Contains(item)
            }));

            if (blank)
                list.Insert(0, new SelectListItem());

            return list;
        }

        public static IEnumerable<SelectListItem> ToSelectList(this IDictionary<string, string> items, bool blank = false, params string[] selected)
        {
            var list = new List<SelectListItem>();

            if (items == null)
                return list;

            list.AddRange(items.Select(item => new SelectListItem
            {
                Value = item.Key,
                Text = item.Value,
                Selected = selected.Contains(item.Key),
            }));

            if (blank)
                list.Insert(0, new SelectListItem());

            return list;
        }

        public static MvcHtmlString ReadOnly(this HtmlHelper htmlHelper, object value, IDictionary<string, object> htmlAttributes = null)
        {
            var tagBuilder = new TagBuilder("span");
            tagBuilder.MergeAttributes(htmlAttributes);
            tagBuilder.SetInnerText(value.Or());
            return MvcHtmlString.Create(tagBuilder.ToString(TagRenderMode.Normal));
        }

        public static MvcHtmlString MultiSelect(this HtmlHelper htmlHelper, string name, IEnumerable<SelectListItem> items, IDictionary<string, object> htmlAttributes = null)
        {
            var fullName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);

            var sb = new StringBuilder();
            foreach (var item in items)
            {
                var label = new TagBuilder("label");
                //label.MergeAttributes(htmlAttributes);

                var checkbox = new TagBuilder("input");
                checkbox.MergeAttribute("name", fullName, true);
                checkbox.MergeAttribute("type", "checkbox");
                checkbox.MergeAttribute("value", item.Value);
                //checkbox.MergeAttributes(htmlAttributes);

                if (item.Selected)
                    checkbox.MergeAttribute("checked", "checked");

                label.InnerHtml = checkbox.ToString(TagRenderMode.SelfClosing) + " " + item.Text;
                sb.AppendLine("<div class='checkbox'>" + label.ToString(TagRenderMode.Normal) + "</div>");
            }

            return MvcHtmlString.Create(sb.ToString());
        }

        public static ModelMetadata Metadata<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string displayName = null, bool? isReadOnly = null)
        {
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);

            if (metadata.PropertyName == "Id")
                metadata.IsReadOnly = true; //Use Identity attr or private set instead

            if (displayName != null)
                metadata.DisplayName = displayName;

            if (isReadOnly.HasValue)
                metadata.IsReadOnly = isReadOnly.Value;

            return metadata;
        }

        public static MvcHtmlString Editor<TModel>(this HtmlHelper<TModel> htmlHelper, ModelMetadata metadata, IDictionary<string, object> htmlAttributes = null)
        {
            //.Set("placeholder", metadata.DisplayName)
            if (!string.IsNullOrWhiteSpace(metadata.TemplateHint))
            {
                return htmlHelper.Editor(metadata.PropertyName, metadata.TemplateHint, null, new { htmlAttributes });
            }
            else if (metadata.IsReadOnly)
            {
                return htmlHelper.ReadOnly(metadata.Model, htmlAttributes);
            }
            else if (metadata.ModelType == typeof(bool) || metadata.ModelType == typeof(bool?))
            {
                return htmlHelper.CheckBox(metadata.PropertyName, (bool?)metadata.Model == true);
            }
            else if (metadata.PropertyName == "Password")
            {
                return htmlHelper.Password(metadata.PropertyName, metadata.Model, htmlAttributes);
            }
            else
            {
                return htmlHelper.TextBox(metadata.PropertyName, metadata.Model, htmlAttributes);
            }
        }

        public static string MapPath(string path)
        {
            if (path.Contains(@"\"))
                return path;

            var mapped = HostingEnvironment.MapPath(path)
                         ?? path.Replace("/", @"\")
                                .Replace("~", Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));

            CheckFolder(mapped);
            return mapped;
        }

        /// <summary>Verify that a folder exists or creates it.</summary>
        public static bool CheckFolder(string path)
        {
            if (!path.HasValue()) return false;
            if (path.Contains("/")) path = MapPath(path);
            if (Path.GetExtension(path) == "" && !path.EndsWith("\\")) path += @"\";

            var folder = Path.GetDirectoryName(path);
            if (folder == null) return false;
            if (Directory.Exists(folder)) return true;

            try
            {
                Directory.CreateDirectory(folder);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static IHtmlString RadioList(this HtmlHelper htmlHelper, string name, IEnumerable<SelectListItem> list, string separator = null, IDictionary<string, object> htmlAttributes = null)
        {
            var fullname = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);

            if (String.IsNullOrEmpty(fullname))
                throw new ArgumentException("The argument must have a value", "fullname");

            if (list == null)
                throw new ArgumentNullException("list");

            var sb = new StringBuilder();
            var i = 0;

            foreach (var item in list)
            {
                if (!item.Value.HasValue() || !item.Text.HasValue())
                    continue; //blank radio box?

                var radio = new TagBuilder("input");
                radio.MergeAttribute("type", "radio");
                radio.MergeAttribute("id", fullname + "_" + i);
                radio.MergeAttribute("value", item.Value);
                radio.MergeAttribute("name", fullname, true);

                if (item.Selected)
                    radio.MergeAttribute("checked", "checked");

                var label = new TagBuilder("label");
                label.MergeAttributes(htmlAttributes);
                label.InnerHtml = radio.ToString(TagRenderMode.SelfClosing) + " " + item.Text;

                sb.Append(label.ToString(TagRenderMode.Normal) + separator);
                i++;
            }

            return MvcHtmlString.Create(sb.ToString());
        }

        public static IDictionary<string, object> AddClass(this IDictionary<string, object> dict, params string[] classes)
        {
            if (dict == null)
                return null;

            var existing = dict.ContainsKey("class")
                               ? dict["class"].Or().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList()
                               : new List<string>();

            existing.AddRange(classes);

            dict["class"] = String.Join(" ", existing.Distinct());

            return dict;
        }

        public static IDictionary<string, object> RemoveClass(this IDictionary<string, object> dict, params string[] classes)
        {
            if (dict == null)
                return null;

            var existing = dict.ContainsKey("class")
                               ? dict["class"].Or().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList()
                               : new List<string>();

            existing.RemoveAll(x => classes.Contains(x));

            dict["class"] = String.Join(" ", existing.Distinct());

            return dict;
        }
    };
}
