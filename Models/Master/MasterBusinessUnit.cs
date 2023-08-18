using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

        [Required(ErrorMessage = "Kode unit bisnis harus diisi.")]
        [Index("IX_Code", Order = 1, IsUnique = true)]
        [Display(Name = "Kode Unit Bisnis")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        [Remote("IsCodeExists", "MasterBusinessUnits", AdditionalFields = "Id", ErrorMessage = "Kode ini sudah dipakai.")]
        public string Code { get; set; }

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

        public virtual ICollection<ApplicationUser> ApplicationUsers { get; set; }

        public MasterBusinessUnit()
        {
            this.ApplicationUsers = new HashSet<ApplicationUser>();
        }
    }
}