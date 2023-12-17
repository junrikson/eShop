using Datalist;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace eShop.Models
{
    public enum EnumBusinessUnitAccountType
    {
        [Display(Name = "AKUN SEMENTARA")]
        TemporaryAccount = 1,
        [Display(Name = "AKUN PIUTANG")]
        ARAccount = 2,
        [Display(Name = "AKUN HUTANG")]
        APAccount = 3,
        [Display(Name = "AKUN PENJUALAN")]
        SaleAccount = 4,
        [Display(Name = "AKUN PEMBELIAN")]
        PurchaseAccount = 5,
        [Display(Name = "AKUN RETUR PENJUALAN")]
        SaleReturnAccount = 6,
        [Display(Name = "AKUN RETUR PEMBELIAN")]
        PurchaseReturGnAccount = 7,
        [Display(Name = "AKUN HPP (HARGA POKOK PENJUALAN)")]
        COGSAccount = 8,
        [Display(Name = "PIUTANG GIRO (MASUK)")]
        ChequeReceivablesAccount = 9,
        [Display(Name = "HUTANG GIRO (KELUAR)")]
        ChequePayableAccount = 10,
        [Display(Name = "UANG MUKA PENJUALAN")]
        AdvanceRepaymentAccount = 11,
        [Display(Name = "UANG MUKA PEMBELIAN")]
        AdvancePaymentAccount = 12
    }

    public class MasterBusinessUnit
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [DatalistColumn]
        [Required(ErrorMessage = "Kode unit bisnis harus diisi.")]
        [Index("IX_Code", Order = 1, IsUnique = true)]
        [Display(Name = "Kode Unit Bisnis")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        [Remote("IsCodeExists", "MasterBusinessUnits", AdditionalFields = "Id", ErrorMessage = "Kode ini sudah dipakai.")]
        public string Code { get; set; }

        [DatalistColumn]
        [Required(ErrorMessage = "Nama unit bisnis harus diisi.")]
        [Display(Name = "Nama Unit Bisnis")]
        [StringLength(256, ErrorMessage = "Maksimal 256 huruf.")]
        public string Name { get; set; }

        [Display(Name = "Pimpinan/Pengurus")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        public string Management { get; set; }

        [Display(Name = "Alamat")]
        [DataType(DataType.MultilineText)]
        public string Address { get; set; }

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

        [Display(Name = "Keterangan")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        [Display(Name = "Nomor Faktur Pajak Terakhir")]
        [StringLength(256, ErrorMessage = "Maksimal 256 huruf.")]
        public string LastTaxNumber { get; set; }

        [Display(Name = "Tanggal Mulai")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [Display(Name = "Aktif")]
        public bool Active { get; set; }

        [Display(Name = "PPN")]
        public bool IsPPN { get; set; }

        [Display(Name = "Nilai PPN (%)")]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        public decimal PPNRate { get; set; }

        [Display(Name = "Tarif FCL (Rp)")]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        public decimal FCLLoadFee { get; set; }

        [Display(Name = "Tarif LCL (Rp)")]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        public decimal LCLLoadFee { get; set; }

        [Display(Name = "Tarif Kontainer Kosong (Rp)")]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        public decimal EmptyLoadFee { get; set; }
    }

    public class MasterBusinessUnitRelation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Unit Bisnis")]
        [Required(ErrorMessage = "Unit Bisnis harus diisi.")]
        public int MasterBusinessUnitId { get; set; }

        [Display(Name = "Unit Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessUnit { get; set; }

        [Display(Name = "Relasi Bisnis")]
        public int MasterBusinessRelationId { get; set; }

        [Display(Name = "Relasi Bisnis")]
        public virtual MasterBusinessUnit MasterBusinessRelation { get; set; }

        [Display(Name = "Wilayah")]
        [Required(ErrorMessage = "Wilayah harus diisi.")]
        public int MasterRegionId { get; set; }

        [Display(Name = "Wilayah")]
        public virtual MasterRegion MasterRegion { get; set; }

        [Display(Name = "Relasi Konsumen")]
        public int? MasterCustomerId { get; set; }

        [Display(Name = "Relasi Konsumen")]
        public virtual MasterCustomer MasterCustomer { get; set; }

        [Display(Name = "Relasi Supplier")]
        public int? MasterSupplierId { get; set; }

        [Display(Name = "Relasi Supplier")]
        public virtual MasterSupplier MasterSupplier { get; set; }

        [Display(Name = "Gudang")]
        public int? MasterWarehouseId { get; set; }

        [Display(Name = "Gudang")]
        public virtual MasterWarehouse MasterWarehouse { get; set; }

        [Display(Name = "Keterangan")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }
    }


    public class MasterBusinessUnitRelationViewModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Unit Bisnis")]
        [Required(ErrorMessage = "Unit Bisnis harus diisi.")]
        public int MasterBusinessUnitId { get; set; }

        [DatalistColumn]
        [Display(Name = "Relasi Bisnis")]
        public string MasterBusinessRelation { get; set; }

        [DatalistColumn]
        [Display(Name = "Wilayah")]
        public string MasterRegion { get; set; }

        [DatalistColumn]
        [Display(Name = "Relasi Konsumen")]
        public string MasterCustomer { get; set; }

        [DatalistColumn]
        [Display(Name = "Relasi Supplier")]
        public string MasterSupplier { get; set; }

        [DatalistColumn]
        [Display(Name = "Keterangan")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }
    }

    public class MasterBusinessUnitDatalist : MvcDatalist<MasterBusinessUnit>
    {
        private DbContext Context { get; }

        public MasterBusinessUnitDatalist(DbContext context)
        {
            Context = context;

            GetLabel = (model) => model.Code + " - " + model.Name;
        }
        public MasterBusinessUnitDatalist()
        {
            Url = "/DatalistFilters/AllMasterBusinessUnit";
            Title = "Master Unit Bisnis";

            Filter.Sort = "Code";
            Filter.Order = DatalistSortOrder.Asc;
            Filter.Rows = 10;
        }

        public override IQueryable<MasterBusinessUnit> GetModels()
        {
            return Context.Set<MasterBusinessUnit>();
        }
    }

    public class MasterBusinessUnitRelationDatalist : MvcDatalist<MasterBusinessUnitRelationViewModel>
    {
        private DbContext Context { get; }

        public MasterBusinessUnitRelationDatalist(DbContext context)
        {
            Context = context;

            GetLabel = (model) => model.MasterBusinessRelation;
        }
        public MasterBusinessUnitRelationDatalist()
        {
            Url = "/DatalistFilters/AllMasterBusinessUnitRelation";
            Title = "Master Unit Bisnis";
            AdditionalFilters.Add("MasterBusinessUnitId");

            Filter.Order = DatalistSortOrder.Asc;
            Filter.Rows = 10;
        }

        public override IQueryable<MasterBusinessUnitRelationViewModel> GetModels()
        {
            return Context.Set<MasterBusinessUnitRelation>()
                .Select(x => new MasterBusinessUnitRelationViewModel
                {
                    Id = x.MasterBusinessRelationId,
                    MasterBusinessUnitId = x.MasterBusinessUnitId,
                    MasterBusinessRelation = x.MasterBusinessRelation.Code,
                    MasterRegion = x.MasterRegion.Code,
                    MasterCustomer = x.MasterCustomer.FullName,
                    MasterSupplier = x.MasterSupplier.FullName
                });
        }
    }
}