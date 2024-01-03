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

    public enum EnumMethodType
    {
        [Display(Name = "Metode Garis Lurus")]
        StraightLine = 1,
        [Display(Name = "Metode Saldo Menurun Tetap")]
        FixedDecliningBalance = 2,
        [Display(Name = "Tanpa Penyusutan")]
        NoDepreciation = 3
    }
    public class FixedAsset
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Unit Bisnis")]
        [Required(ErrorMessage = "Unit Bisnis harus diisi.")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }

        [Display(Name = "Wilayah")]
        [Required(ErrorMessage = "Wilayah harus diisi.")]
        public int MasterRegionId { get; set; }

        [Display(Name = "Wilayah")]
        public virtual MasterRegion MasterRegion { get; set; }

        [DatalistColumn]
        [Required(ErrorMessage = "Kode FixedAsset must be filled.")]
        [Index("IX_Code", Order = 1, IsUnique = true)]
        [Display(Name = "Kode FixedAsset")]
        [StringLength(128, ErrorMessage = "Maximum 128 Characters.")]
        [Remote("IsCodeExists", "FixedAssets", AdditionalFields = "Id", ErrorMessage = "This code has been used.")]
        public string Code { get; set; }

        [Display(Name = "Nama Aset")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        public string Name { get; set; }

        [Display(Name = "Kategori")]
        [Required(ErrorMessage = "Kategori harus diisi.")]
        public int FixedAssetCategoryId { get; set; }

        [Display(Name = "Kategori")]
        public virtual FixedAssetCategory FixedAssetCategory { get; set; }

        [Required(ErrorMessage = "Metode Penyusutan harus diisi.")]
        [Display(Name = "Metode Penyusutan")]
        public EnumMethodType MethodType { get; set; }

        [Display(Name = "Quantity")]
        [Required(ErrorMessage = "Quantity harus diisi.")]
        public int Quantity { get; set; }

        [Display(Name = "Umur Asset (Bulan)")]
        [Required(ErrorMessage = "Umur Asset harus diisi.")]
        public int EstimatedLife { get; set; }

        [DatalistColumn]
        [Display(Name = "Tgl Pembelian")]
        [Required(ErrorMessage = "Tanggal harus diisi.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Purchasedate { get; set; }

        [Display(Name = "Akun Aset")]
        [Required(ErrorMessage = "Akun Aset harus diisi.")]
        public int AssetAccountId { get; set; }

        [Display(Name = "Akun Aset")]
        public virtual ChartOfAccount AssetAccount { get; set; }

        [Display(Name = "Akun Akumulasi Penyusutan")]
        [Required(ErrorMessage = "Akun Akumulasi Penyusutan.")]
        public int AccumulatedDepreciationAccountId { get; set; }

        [Display(Name = "Akun Akumulasi Penyusutan")]
        public virtual ChartOfAccount AccumulatedDepreciationAccount { get; set; }

        [Display(Name = "Akun Beban Penyusutan")]
        [Required(ErrorMessage = "Akun Beban Penyusutan.")]
        public int DepreciationAccountId { get; set; }

        [Display(Name = "Akun Beban Penyusutan")]
        public virtual ChartOfAccount DepreciationAccount { get; set; }


        [DatalistColumn]
        [Display(Name = "Keterangan")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        [Display(Name = "Total")]
        [Required(ErrorMessage = "Total harus diisi.")]
        public int Total { get; set; }

        [Display(Name = "Aktif")]
        public bool Active { get; set; }

        [Display(Name = "Pajak")]
        public bool Tax { get; set; }

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


    //public class TermSalesPerson
    //{
    //    [Key]
    //    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    //    public int Id { get; set; }

    //    [Display(Name = "Term Sales")]
    //    [Required(ErrorMessage = "Sales Term harus diisi.")]
    //    public int TermId { get; set; }

    //    [Display(Name = "Sales Term")]
    //    public virtual Term Term { get; set; }

    //    [Display(Name = "Kode Sales")]
    //    [Required(ErrorMessage = "Kode Sales harus diisi.")]
    //    public int MasterSalesPersonId { get; set; }

    //    [Display(Name = "Kode Sales")]
    //    public virtual MasterSalesPerson MasterSalesPerson { get; set; }

    //    [Display(Name = "Dibuat")]
    //    [DataType(DataType.DateTime)]
    //    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss tt}", ApplyFormatInEditMode = true)]
    //    public DateTime Created { get; set; }

    //    [Display(Name = "Diubah")]
    //    [DataType(DataType.DateTime)]
    //    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss tt}", ApplyFormatInEditMode = true)]
    //    public DateTime Updated { get; set; }

    //    [Display(Name = "User")]
    //    public int UserId { get; set; }

    //    [Display(Name = "User")]
    //    public virtual ApplicationUser User { get; set; }

    //}

    //public class TermCustomer
    //{
    //    [Key]
    //    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    //    public int Id { get; set; }

    //    [Display(Name = "Term Customer")]
    //    [Required(ErrorMessage = "Customer Term harus diisi.")]
    //    public int TermId { get; set; }

    //    [Display(Name = "Customer Term")]
    //    public virtual Term Term { get; set; }

    //    [DatalistColumn]
    //    [Display(Name = "Kode Customer")]
    //    [Required(ErrorMessage = "Kode Customer harus diisi.")]
    //    public int MasterCustomerId { get; set; }

    //    [Display(Name = "Kode Customer")]
    //    public virtual MasterCustomer MasterCustomer { get; set; }

    //    [Display(Name = "Satuan")]
    //    [Required(ErrorMessage = "Master Item harus diisi.")]
    //    public int MasterItemUnitId { get; set; }

    //    [Display(Name = "Satuan")]
    //    public virtual MasterItemUnit MasterItemUnit { get; set; }

    //    [Display(Name = "Harga")]
    //    [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
    //    public decimal Price { get; set; }

    //    [Display(Name = "Dibuat")]
    //    [DataType(DataType.DateTime)]
    //    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss tt}", ApplyFormatInEditMode = true)]
    //    public DateTime Created { get; set; }

    //    [Display(Name = "Diubah")]
    //    [DataType(DataType.DateTime)]
    //    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss tt}", ApplyFormatInEditMode = true)]
    //    public DateTime Updated { get; set; }

    //    [Display(Name = "User")]
    //    public int UserId { get; set; }

    //    [Display(Name = "User")]
    //    public virtual ApplicationUser User { get; set; }

    //}



}