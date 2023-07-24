using Datalist;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace eShop.Models
{
    public enum EnumSupplierType
    {
        [Display(Name = "PERUSAHAAN")]
        Company = 1,
        [Display(Name = "PERORANGAN")]
        Individual = 2
    }

    public class MasterSupplier
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [DatalistColumn]
        [Required(ErrorMessage = "Kode supplier harus diisi.")]
        [Index("IX_Code", Order = 1, IsUnique = true)]
        [Display(Name = "Kode Supplier")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        [Remote("IsCodeExists", "MasterSuppliers", AdditionalFields = "Id", ErrorMessage = "Kode ini sudah dipakai.")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Nama supplier harus diisi.")]
        [Display(Name = "Nama Supplier")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        [Remote("IsNameExists", "MasterSuppliers", AdditionalFields = "Id", ErrorMessage = "Nama supplier sudah ada.")]
        public string Name { get; set; }

        [DatalistColumn]
        [Display(Name = "Nama Supplier")]
        [StringLength(256, ErrorMessage = "Maksimal 256 huruf.")]
        public string FullName { get; set; }

        [Display(Name = "Tanggal Lahir")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? Birthday { get; set; }

        [DatalistColumn]
        [Required(ErrorMessage = "Jenis supplier harus diisi.")]
        [Display(Name = "Jenis Supplier")]
        [JsonConverter(typeof(StringEnumConverter))]
        public EnumSupplierType SupplierType { get; set; }

        [Display(Name = "Contact Person")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        public string ContactPerson { get; set; }

        [Required(ErrorMessage = "Jenis Perusahaan harus diisi.")]
        [Display(Name = "Jenis Perusahaan")]
        public EnumCompanyType CompanyType { get; set; }

        [Required(ErrorMessage = "Jenis Kelamin harus diisi.")]
        [Display(Name = "Jenis Kelamin")]
        public EnumGender Gender { get; set; }

        [DatalistColumn]
        [Display(Name = "Alamat")]
        [DataType(DataType.MultilineText)]
        public string Address { get; set; }

        [Display(Name = "Kota")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        public string City { get; set; }

        [Display(Name = "Kota Asal Barang")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        public string SourceCity { get; set; }

        [Display(Name = "Kode Pos")]
        [StringLength(20, ErrorMessage = "Maksimal 20 huruf.")]
        public string Postal { get; set; }

        [Display(Name = "Telepon 1")]
        [StringLength(20, ErrorMessage = "Maksimal 20 huruf.")]
        public string Phone1 { get; set; }

        [Display(Name = "Telepon 2")]
        [StringLength(20, ErrorMessage = "Maksimal 20 huruf.")]
        public string Phone2 { get; set; }

        [Display(Name = "Handphone")]
        [StringLength(20, ErrorMessage = "Maksimal 20 huruf.")]
        public string Mobile { get; set; }

        [Display(Name = "Fax")]
        [StringLength(20, ErrorMessage = "Maksimal 20 huruf.")]
        public string Fax { get; set; }

        [Display(Name = "Email")]
        [StringLength(256, ErrorMessage = "Maksimal 256 huruf.")]
        public string Email { get; set; }

        [Display(Name = "Nomor KTP")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        public string IDCard { get; set; }

        [Display(Name = "Nomor NPWP")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        public string TaxID { get; set; }

        [Display(Name = "Nama NPWP")]
        [StringLength(256, ErrorMessage = "Maksimal 256 huruf.")]
        public string TaxName { get; set; }

        [Display(Name = "Alamat NPWP")]
        [DataType(DataType.MultilineText)]
        public string TaxAddress { get; set; }

        [Display(Name = "Keterangan")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        [Display(Name = "Email Invoice")]
        public string InvoiceEmail { get; set; }

        [Display(Name = "Email BL")]
        public string BLEmail { get; set; }

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
    }

    public class MasterSupplierDatalist : MvcDatalist<MasterSupplier>
    {
        private DbContext Context { get; }

        public MasterSupplierDatalist(DbContext context)
        {
            Context = context;

            GetLabel = (model) => model.FullName;
        }
        public MasterSupplierDatalist()
        {
            Url = "/DatalistFilters/AllMasterSupplier";
            Title = "Master Supplier";

            Filter.Sort = "FullName";
            Filter.Order = DatalistSortOrder.Asc;
            Filter.Rows = 10;
        }

        public override IQueryable<MasterSupplier> GetModels()
        {
            return Context.Set<MasterSupplier>();
        }
    }
}