using Datalist;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace eShop.Models
{
    public class Period
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Unit Bisnis")]
        [Required(ErrorMessage = "Gudang harus diisi.")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "MasterBusinessUnit")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }

        [Display(Name = "Wilayah")]
        [Required(ErrorMessage = "Wilayah harus diisi.")]
        public int MasterRegionId { get; set; }

        [Display(Name = "Wilayah")]
        public virtual MasterRegion MasterRegion { get; set; }

        [Display(Name = "Tahun")]
        [Required(ErrorMessage = "Tahun harus diisi.")]
        public int Year { get; set; }

        [Display(Name = "Bulan")]
        [Required(ErrorMessage = "Bulan harus diisi.")]
        public int Month { get; set; }

        [Display(Name = "Closing")]
        public bool IsClosed { get; set; }
    }
}