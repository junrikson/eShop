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
    public class MasterCurrency
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [DatalistColumn]
        [Required(ErrorMessage = "Kode Currency harus diisi.")]
        [Index("IX_Code", Order = 1, IsUnique = true)]
        [Display(Name = "Kode Currency")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        [Remote("IsCodeExists", "MasterCurrencies", AdditionalFields = "Id", ErrorMessage = "Kode ini sudah dipakai.")]
        public string Code { get; set; }

        [DatalistColumn]
        [Required(ErrorMessage = "Nama currency harus diisi.")]
        [Display(Name = "Nama Currency")]
        [StringLength(256, ErrorMessage = "Maksimal 256 huruf.")]
        public string Name { get; set; }

        [Display(Name = "Keterangan")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        [DatalistColumn]
        [Display(Name = "Rate")]
        [Required(ErrorMessage = "Rate harus diisi.")]
        [DisplayFormat(DataFormatString = "{0:0.##########}", ApplyFormatInEditMode = true)]
        public decimal Rate { get; set; }

        [DatalistColumn]
        [Display(Name = "Default")]
        public bool Default { get; set; }

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

    public class MasterCurrencyDatalist : MvcDatalist<MasterCurrency>
    {
        private DbContext Context { get; }

        public MasterCurrencyDatalist(DbContext context)
        {
            Context = context;

            GetLabel = (model) => model.Code;
        }
        public MasterCurrencyDatalist()
        {
            Url = "/DatalistFilters/AllMasterCurrency";
            Title = "Mata Uang";

            Filter.Sort = "Code";
            Filter.Order = DatalistSortOrder.Asc;
            Filter.Rows = 10;
        }

        public override IQueryable<MasterCurrency> GetModels()
        {
            return Context.Set<MasterCurrency>();
        }
    }

    public class ChangeCurrency
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Mata Uang")]
        [Required(ErrorMessage = "Mata Uang harus diisi.")]
        public int MasterCurrencyId { get; set; }

        [Display(Name = "Mata Uang")]
        public virtual MasterCurrency MasterCurrency { get; set; }

        [Display(Name = "Rate")]
        [Required(ErrorMessage = "Rate harus diisi.")]
        [DisplayFormat(DataFormatString = "{0:0.##########}", ApplyFormatInEditMode = true)]
        public decimal Rate { get; set; }
    }
}