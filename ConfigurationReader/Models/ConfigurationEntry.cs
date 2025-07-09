using System.ComponentModel.DataAnnotations;

namespace ConfigurationReader.Models
{
    public class ConfigurationEntry
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "İsim alanı zorunludur.")]
        [StringLength(100)]
        [Display(Name = "Anahtar Adı")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Tip alanı zorunludur.")]
        [StringLength(50)]
        [Display(Name = "Veri Tipi")]
        public string Type { get; set; }

        [Required(ErrorMessage = "Değer alanı zorunludur.")]
        [StringLength(500)]
        [Display(Name = "Değer")]
        public string Value { get; set; }

        [Display(Name = "Aktif mi?")]
        public bool IsActive { get; set; }

        [Required(ErrorMessage = "Uygulama Adı zorunludur.")]
        [StringLength(100)]
        [Display(Name = "Uygulama Adı")]
        public string ApplicationName { get; set; }
    }
}
