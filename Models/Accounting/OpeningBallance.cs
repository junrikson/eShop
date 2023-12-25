using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eShop.Models
{
    public class OpeningBalance
    {
        [Key, Column(Order = 0)]
        [Display(Name = "Bagan Akun")]
        [Required(ErrorMessage = "Bagan Akun harus diisi.")]
        public int ChartOfAccountId { get; set; }

        [Display(Name = "Bagan Akun")]
        public virtual ChartOfAccount ChartOfAccount { get; set; }

        [Key, Column(Order = 1)]
        [Display(Name = "Unit Bisnis")]
        [Required(ErrorMessage = "Gudang harus diisi.")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }

        [Key, Column(Order = 2)]
        [Display(Name = "Wilayah")]
        [Required(ErrorMessage = "Wilayah harus diisi.")]
        public int MasterRegionId { get; set; }

        [Display(Name = "Wilayah")]
        public virtual MasterRegion MasterRegion { get; set; }

        [Display(Name = "Saldo Awal")]
        public decimal Total { get; set; }
    }

    public class PostingViewModel
    {
        [Display(Name = "Bulan")]
        [Required(ErrorMessage = "Bulan harus diisi.")]
        public int Month { get; set; }

        [Display(Name = "Year")]
        [Required(ErrorMessage = "Year harus diisi.")]
        public int Year { get; set; }

        public int StartYear { get; set; }

        public int EndYear { get; set; }

        [Display(Name = "Unit Bisnis")]
        [Required(ErrorMessage = "Unit Bisnis harus diisi.")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }
    }
}