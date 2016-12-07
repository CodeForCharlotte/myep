using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Site
{
    //REF: http://stackoverflow.com/questions/6834814/modelmetadata-custom-class-attributes-and-an-indescribable-question
    public class SiteModelMetadataProvider : DataAnnotationsModelMetadataProvider
    {
        protected override ModelMetadata CreateMetadata(
            IEnumerable<Attribute> attributes,
            Type containerType,
            Func<object> modelAccessor,
            Type modelType,
            string propertyName
        )
        {
            var attrs = new List<Attribute>(attributes);
            var modelMetadata = base.CreateMetadata(attrs, containerType, modelAccessor, modelType, propertyName);

            //Date
            var dateAttr = attrs.OfType<DateAttribute>().SingleOrDefault();
            if (dateAttr != null)
            {
                modelMetadata.TemplateHint = "_Date";
            }

            //DateTime
            var dateTimeAttr = attrs.OfType<DateTimeAttribute>().SingleOrDefault();
            if (dateTimeAttr != null)
            {
                modelMetadata.TemplateHint = "_DateTime";
            }

            //Select
            var selectAttr = attrs.OfType<SelectAttribute>().SingleOrDefault();
            if (selectAttr != null)
            {
                modelMetadata.AdditionalValues["CodeType"] = selectAttr.CodeType.Or(containerType.Name + "." + propertyName);
                modelMetadata.TemplateHint = "_Select";
            }

            //MultiSelect
            var multiAttr = attrs.OfType<MultiSelectAttribute>().SingleOrDefault();
            if (multiAttr != null)
            {
                modelMetadata.AdditionalValues["CodeType"] = multiAttr.CodeType.Or(containerType.Name + "." + propertyName);
                modelMetadata.TemplateHint = "_MultiSelect";
            }

            //Length
            var stringLength = attrs.OfType<StringLengthAttribute>().SingleOrDefault();
            if (stringLength != null && stringLength.MaximumLength >= 250)
            {
                modelMetadata.TemplateHint = "_Multiline";
            }

            //Bool
            if (modelMetadata.ModelType == typeof(bool))
            {
                modelMetadata.TemplateHint = "_YesNo";
            }

            return modelMetadata;
        }
    };

    //REF: http://www.prideparrot.com/blog/archive/2012/6/customizing_property_binding_through_attributes
    public class SiteModelBinder : DefaultModelBinder
    {
        protected override void BindProperty(ControllerContext controllerContext,
            ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor)
        {
            if (propertyDescriptor.Attributes.OfType<PropertyBindAttribute>().Any())
            {
                var modelBindAttr = propertyDescriptor.Attributes.OfType<PropertyBindAttribute>().FirstOrDefault();

                if (modelBindAttr.BindProperty(controllerContext, bindingContext, propertyDescriptor))
                    return;
            }

            base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public abstract class PropertyBindAttribute : Attribute
    {
        public abstract bool BindProperty(ControllerContext controllerContext,
        ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor);
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DateAttribute : Attribute { }; //Use UI for dates

    [AttributeUsage(AttributeTargets.Property)]
    public class DateTimeAttribute : Attribute { }; //Use UI for date times

    [AttributeUsage(AttributeTargets.Property)]
    public class SelectAttribute : Attribute
    {
        public SelectAttribute() { }
        public SelectAttribute(string codeType) { CodeType = codeType; }
        public string CodeType { get; set; }
    };

    [AttributeUsage(AttributeTargets.Property)]
    public class MultiSelectAttribute : PropertyBindAttribute ////Checkbox list
    {
        public MultiSelectAttribute() { }
        public MultiSelectAttribute(string codeType) { CodeType = codeType; }
        public string CodeType { get; set; }

        public override bool BindProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor)
        {
            if (propertyDescriptor.PropertyType == typeof(string))
            {
                var value = bindingContext.ValueProvider.GetValue(propertyDescriptor.Name);
                if (value != null)
                {
                    propertyDescriptor.SetValue(bindingContext.Model, value.AttemptedValue);
                    return true;
                }
            }

            return false;
        }
    };
}
