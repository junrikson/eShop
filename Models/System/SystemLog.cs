using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace eShop.Models
{
    public enum EnumMenuType
    {
        [Display(Name = "Master Konsumen")]
        MasterCustomer = 1,
        [Display(Name = "Master Bank")]
        MasterCashBank = 2,
        [Display(Name = "Master Kapal")]
        MasterShip = 3,
        [Display(Name = "Master Satuan")]
        MasterUnit = 4,
        [Display(Name = "Master User")]
        MasterUser = 5,
        [Display(Name = "Master Gudang")]
        MasterWarehouse = 6,
        [Display(Name = "Master Pelabuhan")]
        MasterPort = 7,
        [Display(Name = "Master Unit Bisnis")]
        MasterBusinessUnit = 8,
        [Display(Name = "Master Relasi")]
        MasterRelation = 9,
        [Display(Name = "Master Kontainer")]
        MasterContainer = 10,
        [Display(Name = "Quotation")]
        Quotation = 11,
        [Display(Name = "Detail Quotation")]
        QuotationDetails = 12,
        [Display(Name = "Master Satuan Harga")]
        MasterPriceUnit = 13,
        [Display(Name = "Input Kontainer")]
        ContainerInput = 14,
        [Display(Name = "Detail Input Kontainer")]
        ContainerInputDetails = 15,
        [Display(Name = "Input Gudang")]
        WarehouseInput = 16,
        [Display(Name = "Detail Input Gudang")]
        WarehouseInputDetails = 17,
        [Display(Name = "Roles")]
        Role = 18,
        [Display(Name = "Otorisasi")]
        Authorization = 19,
        [Display(Name = "Master MKL")]
        MasterMKL = 20,
        [Display(Name = "U.P. Quotation")]
        QuotationRecipient = 21,
        [Display(Name = "Loading Kapal")]
        Loading = 22,
        [Display(Name = "Detail Loading Kapal")]
        LoadingDetails = 23,
        [Display(Name = "Surat Muat Sementara")]
        Shipment = 24,
        [Display(Name = "Detail Surat Muat Sementara")]
        ShipmentDetails = 25,
        [Display(Name = "Manifest")]
        Manifest = 26,
        [Display(Name = "BL")]
        BL = 27,
        [Display(Name = "Nomor BL")]
        BLCreate = 28,
        [Display(Name = "BAST")]
        Handover = 29,
        [Display(Name = "Nomor BAST")]
        HandoverCreate = 30,
        [Display(Name = "Invoice")]
        Invoice = 31,
        [Display(Name = "Detail Invoice")]
        InvoiceDetails = 32,
        [Display(Name = "Invoice Lainnya")]
        InvoiceOthers = 33,
        [Display(Name = "Detail Invoice Lainnya")]
        InvoiceOthersDetails = 34,
        [Display(Name = "Nota Kredit")]
        CreditNote = 35,
        [Display(Name = "Detail Nota Kredit")]
        CreditNoteDetails = 36,
        [Display(Name = "Bagan Akun")]
        ChartOfAccount = 37,
        [Display(Name = "Master Biaya")]
        MasterCost = 38,
        [Display(Name = "Booking Order")]
        BookingOrder = 39,
        [Display(Name = "Detail Booking Order")]
        BookingOrderDetails = 40,
        [Display(Name = "Master Wilayah")]
        MasterRegion = 41,
        [Display(Name = "Detail Master Wilayah")]
        MasterRegionDetails = 42,
        [Display(Name = "Kas Masuk")]
        CashIns = 43,
        [Display(Name = "Kas Keluar")]
        CashOuts = 44,
        [Display(Name = "Bank Masuk")]
        BankIns = 45,
        [Display(Name = "Bank Keluar")]
        BankOuts = 46,
        [Display(Name = "Detail Kas Masuk")]
        CashInsDetails = 47,
        [Display(Name = "Detail Kas Keluar")]
        CashOutsDetails = 48,
        [Display(Name = "Detail Bank Masuk")]
        BankInsDetails = 49,
        [Display(Name = "Detail Bank Keluar")]
        BankOutsDetails = 50,
        [Display(Name = "Jurnal Umum")]
        Journals = 51,
        [Display(Name = "Detail Jurnal Umum")]
        JournalsDetails = 52,
        [Display(Name = "Memo")]
        Memo = 53,
        [Display(Name = "Master Biaya Forwarding")]
        ForwardingMasterCost = 54,
        [Display(Name = "Master Vendor Forwarding")]
        ForwardingMasterVendor = 55,
        [Display(Name = "Quotation Forwarding")]
        ForwardingQuotation = 56,
        [Display(Name = "Job Order")]
        ForwardingJobOrder = 57,
        [Display(Name = "Selling")]
        ForwardingSelling = 58,
        [Display(Name = "Buying")]
        ForwardingBuying = 59,
        [Display(Name = "Master Currency")]
        MasterCurrency = 60,
        [Display(Name = "Quotation Details Forwarding")]
        ForwardingQuotationDetails = 61,
        [Display(Name = "Job Order Details")]
        ForwardingJobOrderDetails = 62,
        [Display(Name = "Selling Details")]
        ForwardingSellingDetails = 63,
        [Display(Name = "Buying Details")]
        ForwardingBuyingDetails = 64,
        [Display(Name = "Email")]
        Email = 65,
        [Display(Name = "Giro & Cek Masuk")]
        ChequeIn = 66,
        [Display(Name = "Giro & Cek Keluar")]
        ChequeOut = 67,
        [Display(Name = "Pencairan Giro & Cek")]
        GiroChequeCashOut = 68,
        [Display(Name = "Detail Pencairan Giro & Cek")]
        GiroChequeCashOutDetails = 69,
        [Display(Name = "Pelunasan")]
        Repayment = 70,
        [Display(Name = "Detail Pelunasan")]
        RepaymentDetails = 71,
        [Display(Name = "Login")]
        Login = 72,
        [Display(Name = "Akun Master Wilayah")]
        MasterRegionAccounts = 73,
        [Display(Name = "Surat Jalan")]
        DeliveryOrder = 74,
        [Display(Name = "Satuan Barang")]
        MasterItemUnits = 75,
        [Display(Name = "Detail Tally Sheet")]
        TallySheetsDetails = 76,
        [Display(Name = "Jenis Kontainer")]
        MasterContainerType = 77,
        [Display(Name = "Jenis Supplier")]
        MasterSupplier = 78,
        [Display(Name = "Job Order Kontainer")]
        ForwardingJobOrderContainers = 79,
        [Display(Name = "Jenis Stowage")]
        StowageType = 80,
        [Display(Name = "Stowage Plan")]
        StowagePlan = 81,
        [Display(Name = "Jenis Stowage Detail")]
        StowageTypeDetails = 82,
        [Display(Name = "Permohonan Pengeluaran")]
        BankOutsPending = 83,
        [Display(Name = "Detail Permohonan Pengeluaran")]
        BankOutsDetailsPending = 84,
        [Display(Name = "Unloading Stowage")]
        StowagePlanUnloading = 85,
        [Display(Name = "Master Kolektor")]
        MasterCollector = 86,
        [Display(Name = "Setoran Sales")]
        SalesDeposit = 87,
        [Display(Name = "Detail Setoran Sales")]
        SalesDepositDetails = 88,
        [Display(Name = "On/Off Hire")]
        ContainerHire = 89,
        [Display(Name = "Master Item")]
        MasterItem = 90,
        [Display(Name = "Master Category")]
        MasterCategory = 91,
        [Display(Name = "Master Brand")]
        MasterBrand = 92,
        [Display(Name = "Purchase Order")]
        PurchaseOrder = 93,
        [Display(Name = "Purchase Order Details")]
        PurchaseOrderDetails = 94,
        [Display(Name = "Purchase Request")]
        PurchaseRequest = 95,
        [Display(Name = "Purchase Request Details")]
        PurchaseRequestDetails = 96,
        [Display(Name = "Purchase")]
        Purchase = 97,
        [Display(Name = "Purchase Details")]
        PurchaseDetails = 98,
        [Display(Name = "SalesRequest")]
        SalesRequest = 99,
        [Display(Name = "SalesRequestDetails")]
        SalesRequestDetails = 100,
        [Display(Name = "Sales Order")]
        SalesOrder = 101,
        [Display(Name = "Sales Order Details")]
        SalesOrderDetails = 102,
        [Display(Name = "Sale")]
        Sale = 103,
        [Display(Name = "Sale Details")]
        SaleDetails = 104,       
        [Display(Name = "Purchase Return")]
        PurchaseReturn = 105,
        [Display(Name = "Purchase Return Details")]
        PurchaseReturnDetails = 106,
        [Display(Name = "Sales Return")]
        SalesReturn = 107,
        [Display(Name = "Sales Return Details")]
        SalesReturnDetails = 108,
        [Display(Name = "Adjustment")]
        StockAdjustment = 109,
        [Display(Name = "Adjustment Details")]
        StockAdjustmentDetails = 110,
    }

    public enum EnumActions
    {
        [Display(Name = "membuat")]
        CREATE = 1,
        [Display(Name = "mengubah")]
        EDIT = 2,
        [Display(Name = "menghapus")]
        DELETE = 3,
        [Display(Name = "mencetak")]
        PRINT = 4
    }

    public class SystemLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Tanggal")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss tt}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        [Display(Name = "Jenis Menu")]
        public EnumMenuType? MenuType { get; set; }

        [Display(Name = "ID Menu")]
        public int MenuId { get; set; }

        [Display(Name = "Kode Menu")]
        public string MenuCode { get; set; }

        [Display(Name = "Tindakan")]
        public EnumActions? Actions { get; set; }

        [Display(Name = "User")]
        public int UserId { get; set; }

        [Display(Name = "User")]
        public virtual ApplicationUser User { get; set; }
    }
}