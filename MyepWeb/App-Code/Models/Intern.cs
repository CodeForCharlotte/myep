using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Site
{
    [Table("Interns")]
    public class Intern
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [StringLength(10)]
        [Index("IX_Intern_AccessCode", IsUnique = true)]
        public string AccessCode { get; set; }

        [StringLength(10)]
        //[Index("InternCmsStudentId", unique:true)] //allow nulls?
        public string CmsStudentId { get; set; }

        [Required]
        [StringLength(25)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(25)]
        public string LastName { get; set; }

        [StringLength(1)]
        public string MiddleInitial { get; set; }

        [Required]
        [StringLength(50)]
        public string Address1 { get; set; }

        [StringLength(50)]
        public string Address2 { get; set; }

        [Required]
        [StringLength(50)]
        public string City { get; set; }

        [Required]
        [UIHint("_States")]
        [StringLength(2)]
        public string State { get; set; }

        [Required]
        [StringLength(50)]
        public string Zip { get; set; }

        [Required]
        [Phone]
        [StringLength(12)]
        public string Phone { get; set; }

        public bool PhoneIsCell { get; set; }

        [Phone]
        [StringLength(12)]
        public string Phone2 { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(75)]
        public string Email { get; set; }

        [Date]
        public DateTime BirthDate { get; set; }

        [StringLength(100)]
        [MultiSelect]
        public string SummerActivity { get; set; }

        public bool HasSummerTravel { get; set; }

        [StringLength(100)]
        public string SummerTravelDates { get; set; }

        public bool HasSummerSchool { get; set; }

        [StringLength(50)]
        public string InternshipAvailability { get; set; }

        public bool ConvictedOfCrime { get; set; }

        [StringLength(1500)]
        public string CrimeDescription { get; set; }

        public bool HasTransportation { get; set; }

        public bool ResidenceOnBusLine { get; set; }

        [StringLength(100)]
        [MultiSelect]
        public string Race { get; set; }

        [StringLength(100)]
        public string RaceOther { get; set; }

        [StringLength(100)]
        [MultiSelect]
        public string Gender { get; set; }

        [StringLength(100)]
        [MultiSelect]
        public string ReferredBy { get; set; }

        [StringLength(50)]
        [Select]
        public string HighSchool { get; set; }

        [Select]
        public int? Grade { get; set; }

        [Select]
        public int? GraduationMonth { get; set; }

        [Select]
        public int? GraduationYear { get; set; }

        public bool ApplyingToCollege { get; set; }

        [StringLength(1500)]
        public string PlansAfterGraduation { get; set; }

        public bool CurrentlyWorking { get; set; }

        [StringLength(1500)]
        public string ClubsActivities { get; set; }

        [Required]
        [StringLength(100)]
        public string ParentName { get; set; }

        [StringLength(25)]
        public string ParentRelationship { get; set; }

        [Required]
        [StringLength(50)]
        public string ParentAddress1 { get; set; }

        [StringLength(50)]
        public string ParentAddress2 { get; set; }

        [Required]
        [StringLength(50)]
        public string ParentCity { get; set; }

        [Required]
        [StringLength(2)]
        public string ParentState { get; set; }

        [Required]
        [StringLength(50)]
        public string ParentZip { get; set; }

        [Required]
        [Phone]
        [StringLength(12)]
        public string ParentPhone { get; set; }

        [Select]
        [StringLength(25)]
        public string ParentPhoneType { get; set; }

        [Phone]
        [StringLength(12)]
        public string ParentPhone2 { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(75)]
        public string ParentEmail { get; set; }

        [StringLength(1500)]
        public string StrengthsTalents { get; set; }

        [StringLength(1500)]
        public string BriefEssay { get; set; }

        [Select]
        [StringLength(1500)]
        public string CareerInterests { get; set; }

        [StringLength(100)]
        public string EssayFileName { get; set; }

        [StringLength(100)]
        public string ResumeFileName { get; set; }

        [StringLength(100)]
        public string RecommendationFileName1 { get; set; }

        [StringLength(100)]
        public string RecommendationFileName2 { get; set; }

        [Select]
        [StringLength(25)]
        public string ShirtSize { get; set; }

        [Select]
        [StringLength(25)]
        public string BackgroundCheck { get; set; }

        [StringLength(250)]
        public string BackgroundCheckConcerns { get; set; }

        [Select]
        [StringLength(25)]
        public string DrugScreen { get; set; }

        [StringLength(25)]
        public string CDCLastName { get; set; }

        public DateTime? CDDAttendDate { get; set; }

        [StringLength(25)]
        public string TrainingCompleted { get; set; }

        [Select]
        [StringLength(100)]
        public string CareerInterest { get; set; }

        [StringLength(50)]
        public string Placement { get; set; }

        [StringLength(1500)]
        public string MonitoringVisitsComments { get; set; }

        [Select]
        [StringLength(25)]
        public string HomeDistrict { get; set; }

        [Select("Intern.HomeDistrict")]
        [StringLength(25)]
        public string HostSiteDistrict { get; set; }

        [Select]
        [StringLength(25)]
        public string HomeServiceArea { get; set; }

        [Select("Intern.HomeServiceArea")]
        [StringLength(25)]
        public string HostSiteServiceArea { get; set; }

        [StringLength(1500)]
        public string Comments { get; set; }

        public bool? PhotoConsentAcknowledgement { get; set; }

        public bool? StatementOfUnderstanding { get; set; }

        public DateTime? Submitted { get; set; }

        public bool Inactive { get; set; }
    };
}
