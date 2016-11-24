using System;
using System.IO;
using System.Reflection;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Site
{
    public class ViewInternDoc
    {
        public byte[] Generate(Intern intern)
        {
            using (var stream = new MemoryStream())
            {
                using (var document = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
                {
                    Func<RunProperties> bolded = () => new RunProperties(new Bold());
                    Func<RunProperties> heading1 = () => new RunProperties(new Bold(), new FontSize { Val = "28" });
                    var body = new Body();

                    document.AddMainDocumentPart();
                    document.MainDocumentPart.Document = new Document(body);

                    body.AppendChild(new Paragraph(new Run(heading1(), new Text(intern.FirstName + " " + intern.LastName))));

                    var props = intern.GetType().GetProperties();
                    foreach (var prop in props)
                    {
                        if (!ShouldExclude(prop))
                        {
                            var p = new Paragraph();
                            p.AppendChild(new Run(bolded(), new Text(FormatName(prop))));
                            p.AppendChild(new Run(new Text(": " + FormatValue(prop, intern))));
                            body.AppendChild(p);
                        }
                    }
                }
                return stream.ReadAllBytes();
            }
        }

        private static string FormatName(PropertyInfo prop)
        {
            if (prop.Name.StartsWith("Parent"))
            {
                return prop.Name.DeWiki().Replace("Parent", "Parent/Guardian");
            }
            else
            {
                return prop.Name.DeWiki();
            }
        }

        private static bool ShouldExclude(PropertyInfo prop)
        {
            switch (prop.Name)
            {
                case "Id":
                case "AccessCode":
                case "CmsStudentId":
                case "ConvictedOfCrime":
                case "CrimeDescription":
                case "Race":
                case "RaceOther":
                case "Gender":
                case "ReferredBy":
                case "EssayFileName":
                case "ResumeFileName":
                case "RecommendationFileName1":
                case "RecommendationFileName2":
                case "ShirtSize":
                case "BackgroundCheck":
                case "BackgroundCheckConcerns":
                case "DrugScreen":
                case "CDCLastName":
                case "CDDAttendDate":
                case "TrainingCompleted":
                case "CareerInterest":
                case "Placement":
                case "MonitoringVisitsComments":
                case "HomeDistrict":
                case "HostSiteDistrict":
                case "HomeServiceArea":
                case "HostSiteServiceArea":
                case "Comments":
                case "PhotoConsentAcknowledgement":
                case "StatementOfUnderstanding":
                    return true;

                default:
                    return false;
            }
        }

        private static object FormatValue(PropertyInfo prop, Object obj)
        {
            var type = prop.PropertyType.Name == "Nullable`1"
                ? Nullable.GetUnderlyingType(prop.PropertyType).Name
                : prop.PropertyType.Name;

            switch (type)
            {
                case "DateTime":
                    return ((DateTime?)prop.GetValue(obj, null)).ToDateString();

                case "Boolean":
                    return (bool?)prop.GetValue(obj, null) == true ? "Yes" : "No";

                default:
                    return prop.GetValue(obj, null);
            }
        }
    };
}
