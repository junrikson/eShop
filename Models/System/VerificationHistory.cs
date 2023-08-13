using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace eShop.Models
{

    public enum EnumVerificationType
    {
        [Display(Name = "Print Transaksi Kas/Bank")]
        PrintBankTransaction = 1,
        [Display(Name = "Edit Transaksi Kas/Bank")]
        EditBankTransaction = 2
    }

    public class VerificationHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Pembelian")]
        public int? PurchaseId { get; set; }

        [Display(Name = "Pembelian")]
        public virtual Purchase Purchase { get; set; }

        [Display(Name = "Penjualan")]
        public int? SaleId { get; set; }

        [Display(Name = "Penjualan")]
        public virtual Sale Sale { get; set; }

        [Display(Name = "Transaksi Kas/Bank")]
        public int? BankTransactionId { get; set; }

        [Display(Name = "Transaksi Kas/Bank")]
        public virtual BankTransaction BankTransaction { get; set; }

        [Display(Name = "Jenis Verifikasi")]
        public EnumVerificationType Type { get; set; }

        [Display(Name = "Kode Verifikasi")]
        [StringLength(128, ErrorMessage = "Maksimal 128 huruf.")]
        [Required(ErrorMessage = "Kode Verifikasi harus diisi.")]
        public string Verification { get; set; }

        [Display(Name = "Alasan Verifikasi")]
        [Required(ErrorMessage = "Alasan Verifikasi harus diisi.")]
        [DataType(DataType.MultilineText)]
        public string Reason { get; set; }

        [Display(Name = "Dibuat")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss tt}", ApplyFormatInEditMode = true)]
        public DateTime Created { get; set; }

        [Display(Name = "User")]
        public int UserId { get; set; }

        [Display(Name = "User")]
        public virtual ApplicationUser User { get; set; }
    }
}
