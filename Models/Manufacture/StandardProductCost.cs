using Datalist;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace eShop.Models
{

    public class StandardProductCost
    {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [DatalistColumn]
    [Required(ErrorMessage = "Kode Standart Biaya Produksi harus diisi.")]
    [Index("IX_Code", Order = 1, IsUnique = true)]
    [Display(Name = "Kode Standart Biaya Produksi")]
    [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
    [Remote("IsCodeExists", "StandardProductCosts", AdditionalFields = "Id", ErrorMessage = "Kode ini sudah dipakai.")]
    public string Code { get; set; }

    [DatalistColumn]
    [Required(ErrorMessage = "Nama Standart Biaya Produksi harus diisi.")]
    [Display(Name = "Nama Standart Biaya Produksi")]
    [StringLength(256, ErrorMessage = "Maksimal 256 huruf.")]
    public string Name { get; set; }

        [Display(Name = "Satuan")]
        [Required(ErrorMessage = "Master Item harus diisi.")]
        public int MasterUnitId { get; set; }

        [Display(Name = "Satuan")]
        public virtual MasterUnit MasterUnit { get; set; }

        [Display(Name = "Quantity")]
        [Required(ErrorMessage = "Quantity harus diisi.")]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        public decimal Quantity { get; set; }

        [Display(Name = "Harga")]
        [Required(ErrorMessage = "Harga harus diisi.")]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        public decimal Price { get; set; }

        [Display(Name = "Nilai")]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        public decimal Total { get; set; }

        [DatalistColumn]
    [Display(Name = "Keterangan")]
    [DataType(DataType.MultilineText)]
    public string Notes { get; set; }

    [Display(Name = "Aktif")]
    public bool Active { get; set; }

    [Display(Name = "Dibuat")]
    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss tt}", ApplyFormatInEditMode = true)]
    public DateTime Created { get; set; }

    [Display(Name = "Diubah")]
    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss tt}", ApplyFormatInEditMode = true)]
    public DateTime Updated { get; set; }

    [Display(Name = "User")]
    public int UserId { get; set; }

    [Display(Name = "User")]
    public virtual ApplicationUser User { get; set; }
    }
}

  



