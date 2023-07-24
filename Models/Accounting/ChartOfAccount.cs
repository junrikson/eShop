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
    public enum EnumDefaultEntry
    {
        [Display(Name = "Debet")]
        Debit = 1,
        [Display(Name = "Kredit")]
        Credit = 2
    }

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

        [Display(Name = "Unit Bisnis")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }

        [DatalistColumn]
        [Display(Name = "Posisi")]
        [Required(ErrorMessage = "Posisi harus diisi.")]
        public EnumDefaultEntry Position { get; set; }

        [Display(Name = "Header")]
        public bool IsHeader { get; set; }

        [Display(Name = "Level")]
        public int Level { get; set; }

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

        [DatalistColumn]
        [Display(Name = "Unit Bisnis")]
        public string MasterBusinessUnitName { get; set; }

        [Display(Name = "Unit Bisnis")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }

        [DatalistColumn]
        [Display(Name = "Posisi")]
        public EnumDefaultEntry Position { get; set; }

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
            AdditionalFilters.Add("MasterBusinessUnitId");

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
                    MasterBusinessUnitName = x.MasterBusinessUnit.Name,
                    MasterBusinessUnitId = x.MasterBusinessUnitId,
                    MasterBusinessUnit = x.MasterBusinessUnit,
                    Position = x.Position,
                    IsHeader = x.IsHeader,
                    Level = x.Level,
                    Notes = x.Notes,
                    Active = x.Active
                });
        }
    }
}