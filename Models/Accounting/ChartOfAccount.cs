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
    public class ChartOfAccount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [DatalistColumn]
        [Required(ErrorMessage = "Nomor Akun harus diisi.")]
        [Index("IX_Code", Order = 1, IsUnique = true)]
        [Display(Name = "Nomor Akun")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        [Remote("IsCodeExists", "ChartOfAccounts", AdditionalFields = "Id", ErrorMessage = "Nomor Akun sudah ada.")]
        public string Code { get; set; }

        [DatalistColumn]
        [Display(Name = "Nama Akun")]
        [StringLength(256, ErrorMessage = "Maksimal 128 huruf.")]
        [Required(ErrorMessage = "Nama Akun harus diisi.")]
        public string Name { get; set; }

        [Display(Name = "Jenis Akun")]
        [Required(ErrorMessage = "Jenis Akun harus diisi.")]
        public int AccountTypeId { get; set; }

        [Display(Name = "Jenis Akun")]
        public virtual AccountType AccountType { get; set; }

        [Display(Name = "Sub Akun")]
        public int? SubChartOfAccountId { get; set; }

        [Display(Name = "Sub Akun")]
        public virtual ChartOfAccount SubChartOfAccount { get; set; }

        [Display(Name = "Header")]
        public bool IsHeader { get; set; }

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

    public class ChartOfAccountDatalistViewModel
    {
        [Key]
        public int Id { get; set; }

        [DatalistColumn]
        [Required(ErrorMessage = "Nomor Akun harus diisi.")]
        public string Code { get; set; }

        [DatalistColumn]
        [Display(Name = "Nama Akun")]
        public string Name { get; set; }

        [Display(Name = "Header")]
        public bool IsHeader { get; set; }

        [Display(Name = "Level")]
        public int Level { get; set; }

        [Display(Name = "Keterangan")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        [Display(Name = "Aktif")]
        public bool Active { get; set; }
    }

    public class ChartOfAccountDatalist : MvcDatalist<ChartOfAccountDatalistViewModel>
    {
        private DbContext Context { get; }

        public ChartOfAccountDatalist(DbContext context)
        {
            Context = context;

            GetLabel = (model) => model.Code + " - " + model.Name;
        }
        public ChartOfAccountDatalist()
        {
            Url = "/DatalistFilters/AllChartOfAccount";
            Title = "Bagan Akun";

            Filter.Sort = "Code";
            Filter.Order = DatalistSortOrder.Asc;
            Filter.Rows = 10;
        }

        public override IQueryable<ChartOfAccountDatalistViewModel> GetModels()
        {
            return Context.Set<ChartOfAccount>()
                .Select(x => new ChartOfAccountDatalistViewModel
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    IsHeader = x.IsHeader,
                    Notes = x.Notes,
                    Active = x.Active
                });
        }
    }

    public class ChartOfAccountHeaderDatalist : MvcDatalist<ChartOfAccountDatalistViewModel>
    {
        private DbContext Context { get; }

        public ChartOfAccountHeaderDatalist(DbContext context)
        {
            Context = context;

            GetLabel = (model) => model.Code + " - " + model.Name;
        }
        public ChartOfAccountHeaderDatalist()
        {
            Url = "/DatalistFilters/AllChartOfAccountHeader";
            Title = "Bagan Akun";

            Filter.Sort = "Code";
            Filter.Order = DatalistSortOrder.Asc;
            Filter.Rows = 10;
        }

        public override IQueryable<ChartOfAccountDatalistViewModel> GetModels()
        {
            return Context.Set<ChartOfAccount>()
                .Select(x => new ChartOfAccountDatalistViewModel
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    IsHeader = x.IsHeader,
                    Notes = x.Notes,
                    Active = x.Active
                });
        }
    }
}