using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Site
{
    [Table("Codes")]
    public class Code : IEntity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Index("IX_Code_TypeValue", true)]
        [StringLength(100)]
        public string Type { get; set; }

        [Required]
        [Index("IX_Code_TypeValue", true)]
        [StringLength(100)]
        public string Value { get; set; }

        [StringLength(100)]
        public string Label { get; set; }

        public int Seq { get; set; }

        [StringLength(int.MaxValue)]
        public string Data { get; set; }
    };
}
