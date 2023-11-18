using eShop.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Web.Security;

namespace eShop.Extensions
{
    public static class IdentityExtensions
    {
        public static string GetToggleSidebar(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("ToggleSidebar");
            // Test for null to avoid issues during local testing
            return (claim != null) ? claim.Value : string.Empty;
        }

        public static string GetSkin(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("Skin");
            // Test for null to avoid issues during local testing
            return (claim != null) ? claim.Value : string.Empty;
        }
    }

    public static class HtmlExtensions
    {
        public static string GetDisplayName(this Enum enu)
        {
            var attr = GetDisplayAttribute(enu);
            return attr != null ? attr.Name : enu.ToString();
        }

        public static string GetDescription(this Enum enu)
        {
            var attr = GetDisplayAttribute(enu);
            return attr != null ? attr.Description : enu.ToString();
        }

        private static DisplayAttribute GetDisplayAttribute(object value)
        {
            Type type = value.GetType();
            if (!type.IsEnum)
            {
                throw new ArgumentException(string.Format("Type {0} is not an enum", type));
            }

            // Get the enum field.
            var field = type.GetField(value.ToString());
            return field?.GetCustomAttribute<DisplayAttribute>();
        }
    }



    public static class SharedFunctions
    {

        public static decimal GetTotalPurchaseRequest(ApplicationDbContext db, int purchaseRequestId, int? purchaseRequestDetailsId = null)
        {
            decimal total = 0;
            List<PurchaseRequestDetails> purchaseRequestDetails = null;

            if (purchaseRequestDetailsId == null)
            {
                purchaseRequestDetails = db.PurchaseRequestsDetails.Where(x => x.PurchaseRequestId == purchaseRequestId).ToList();
            }
            else
            {
                purchaseRequestDetails = db.PurchaseRequestsDetails.Where(x => x.PurchaseRequestId == purchaseRequestId && x.Id != purchaseRequestDetailsId).ToList();
            }

            if (purchaseRequestDetails != null)
            {
                total = purchaseRequestDetails.Sum(y => y.Total);
            }

            return total;
        }
        public static decimal GetTotalPurchaseOrder(ApplicationDbContext db, int purchaseOrderId, int? purchaseOrderDetailsId = null)
        {
            decimal total = 0;
            List<PurchaseOrderDetails> purchaseOrderDetails = null;

            if (purchaseOrderDetailsId == null)
            {
                purchaseOrderDetails = db.PurchaseOrdersDetails.Where(x => x.PurchaseOrderId == purchaseOrderId).ToList();
            }
            else
            {
                purchaseOrderDetails = db.PurchaseOrdersDetails.Where(x => x.PurchaseOrderId == purchaseOrderId && x.Id != purchaseOrderDetailsId).ToList();
            }

            if (purchaseOrderDetails != null)
            {
                total = purchaseOrderDetails.Sum(y => y.Total);
            }

            return total;
        }

        public static decimal GetTotalStockAdjustment(ApplicationDbContext db, int stockAdjustmentId, int? stockAdjustmentDetailsId = null)
        {
            decimal total = 0;
            List<StockAdjustmentDetails> stockAdjustmentDetails = null;

            if (stockAdjustmentDetailsId == null)
            {
                stockAdjustmentDetails = db.StockAdjustmentsDetails.Where(x => x.StockAdjustmentId == stockAdjustmentId).ToList();
            }
            else
            {
                stockAdjustmentDetails = db.StockAdjustmentsDetails.Where(x => x.StockAdjustmentId == stockAdjustmentId && x.Id != stockAdjustmentDetailsId).ToList();
            }

            if (stockAdjustmentDetails != null)
            {
                total = stockAdjustmentDetails.Sum(y => y.Total);
            }

            return total;
        }

        public static decimal GetTotalPurchaseReturn(ApplicationDbContext db, int purchaseReturnId, int? purchaseReturnDetailsId = null)
        {
            decimal total = 0;
            List<PurchaseReturnDetails> purchaseReturnDetails = null;

            if (purchaseReturnDetailsId == null)
            {
                purchaseReturnDetails = db.PurchaseReturnsDetails.Where(x => x.PurchaseReturnId == purchaseReturnId).ToList();
            }
            else
            {
                purchaseReturnDetails = db.PurchaseReturnsDetails.Where(x => x.PurchaseReturnId == purchaseReturnId && x.Id != purchaseReturnDetailsId).ToList();
            }

            if (purchaseReturnDetails != null)
            {
                total = purchaseReturnDetails.Sum(y => y.Total);
            }

            return total;
        }

        // Begin of Purchase
        public static decimal GetTotalPurchase(ApplicationDbContext db, int purchaseId, int? purchaseDetailsId = null)
        {
            decimal total = 0;
            List<PurchaseDetails> purchaseDetails = null;

            if (purchaseDetailsId == null)
            {
                purchaseDetails = db.PurchasesDetails.Where(x => x.PurchaseId == purchaseId).ToList();
            }
            else
            {
                purchaseDetails = db.PurchasesDetails.Where(x => x.PurchaseId == purchaseId && x.Id != purchaseDetailsId).ToList();
            }

            if (purchaseDetails != null)
            {
                total = purchaseDetails.Sum(y => y.Total);
            }

            return total;
        }

        public static decimal GetTotalProductionBillofMaterial(ApplicationDbContext db, int productionBillofMaterialId, int? productionBillofMaterialDetailsId = null)
        {
            decimal total = 0;
            List<ProductionBillofMaterialDetails> productionBillofMaterialDetails = null;

            if (productionBillofMaterialDetailsId == null)
            {
                productionBillofMaterialDetails = db.ProductionBillofMaterialsDetails.Where(x => x.ProductionBillofMaterialId == productionBillofMaterialId).ToList();
            }
            else
            {
                productionBillofMaterialDetails = db.ProductionBillofMaterialsDetails.Where(x => x.ProductionBillofMaterialId == productionBillofMaterialId && x.Id != productionBillofMaterialDetailsId).ToList();
            }

            if (productionBillofMaterialDetails != null)
            {
                total = productionBillofMaterialDetails.Sum(y => y.Total);
            }

            return total;
        }

        public static decimal GetTotalPackingBillofMaterial(ApplicationDbContext db, int packingBillofMaterialId, int? packingBillofMaterialDetailsId = null)
        {
            decimal total = 0;
            List<PackingBillofMaterialDetails> packingBillofMaterialDetails = null;

            if (packingBillofMaterialDetailsId == null)
            {
                packingBillofMaterialDetails = db.PackingBillofMaterialsDetails.Where(x => x.PackingBillofMaterialId == packingBillofMaterialId).ToList();
            }
            else
            {
                packingBillofMaterialDetails = db.PackingBillofMaterialsDetails.Where(x => x.PackingBillofMaterialId == packingBillofMaterialId && x.Id != packingBillofMaterialDetailsId).ToList();
            }

            if (packingBillofMaterialDetails != null)
            {
                total = packingBillofMaterialDetails.Sum(y => y.Total);
            }

            return total;
        }

        public static decimal GetTotalPackingWorkOrder(ApplicationDbContext db, int packingWorkOrderId, int? packingWorkOrderDetailsId = null)
        {
            decimal total = 0;
            List<PackingWorkOrderDetails> packingWorkOrderDetails = null;

            if (packingWorkOrderDetailsId == null)
            {
                packingWorkOrderDetails = db.PackingWorkOrdersDetails.Where(x => x.PackingWorkOrderId == packingWorkOrderId).ToList();
            }
            else
            {
                packingWorkOrderDetails = db.PackingWorkOrdersDetails.Where(x => x.PackingWorkOrderId == packingWorkOrderId && x.Id != packingWorkOrderDetailsId).ToList();
            }

            if (packingWorkOrderDetails != null)
            {
                total = packingWorkOrderDetails.Sum(y => y.Total);
            }

            return total;
        }

        public static decimal GetTotalProductionWorkOrder(ApplicationDbContext db, int productionWorkOrderId, int? productionWorkOrderDetailsId = null)
        {
            decimal total = 0;
            List<ProductionWorkOrderDetails> productionWorkOrderDetails = null;

            if (productionWorkOrderDetailsId == null)
            {
                productionWorkOrderDetails = db.ProductionWorkOrdersDetails.Where(x => x.ProductionWorkOrderId == productionWorkOrderId).ToList();
            }
            else
            {
                productionWorkOrderDetails = db.ProductionWorkOrdersDetails.Where(x => x.ProductionWorkOrderId == productionWorkOrderId && x.Id != productionWorkOrderDetailsId).ToList();
            }

            if (productionWorkOrderDetails != null)
            {
                total = productionWorkOrderDetails.Sum(y => y.Total);
            }

            return total;
        }

        public static decimal GetTotalMaterialSlip(ApplicationDbContext db, int materialSlipId, int? materialSlipDetailsId = null)
        {
            decimal total = 0;
            List<MaterialSlipDetails> materialSlipDetails = null;

            if (materialSlipDetailsId == null)
            {
                materialSlipDetails = db.MaterialSlipsDetails.Where(x => x.MaterialSlipId == materialSlipId).ToList();
            }
            else
            {
                materialSlipDetails = db.MaterialSlipsDetails.Where(x => x.MaterialSlipId == materialSlipId && x.Id != materialSlipDetailsId).ToList();
            }

            if (materialSlipDetails != null)
            {
                total = materialSlipDetails.Sum(y => y.Total);
            }

            return total;
        }


        public static void CreatePurchaseJournal(ApplicationDbContext db, Purchase purchase)
        {
            Journal journal = new Journal
            {
                Code = purchase.Code,
                Date = purchase.Date,
                MasterBusinessUnitId = purchase.MasterBusinessUnitId,
                Type = EnumJournalType.Purchase,
                Debit = 0,
                Credit = 0,
                PurchaseId = purchase.Id,
                Active = purchase.Active,
                Created = purchase.Created,
                Updated = purchase.Updated,
                UserId = purchase.UserId
            };

            if (string.IsNullOrEmpty(purchase.Notes))
                journal.Notes = "PEMBELIAN NO. " + purchase.Code;
            else
                journal.Notes = purchase.Notes;

            db.Journals.Add(journal);
            db.SaveChanges();
        }

        public static void UpdatePurchaseJournal(ApplicationDbContext db, Purchase purchase)
        {
            Journal journal = db.Journals.Where(x => x.Type == EnumJournalType.Purchase && x.PurchaseId == purchase.Id).FirstOrDefault();

            var purchasesDetails = db.PurchasesDetails.Where(x => x.PurchaseId == purchase.Id).ToList();
            foreach(PurchaseDetails purchaseDetails in purchasesDetails)
            {
                UpdatePurchaseJournalDetails(db, purchaseDetails);
            }

            db.Entry(journal).State = EntityState.Modified;

            journal.Code = purchase.Code;
            journal.Date = purchase.Date;
            journal.MasterBusinessUnitId = purchase.MasterBusinessUnitId;
            journal.Active = purchase.Active;
            journal.Updated = purchase.Updated;
            journal.UserId = purchase.UserId;

            if (string.IsNullOrEmpty(purchase.Notes))
                journal.Notes = "PEMBELIAN NO. " + purchase.Code;
            else
                journal.Notes = purchase.Notes;

            db.SaveChanges();
        }

        public static void DeletePurchaseJournals(ApplicationDbContext db, Purchase purchase)
        {
            Journal journal = db.Journals.Where(x => x.Type == EnumJournalType.Purchase && x.PurchaseId == purchase.Id).FirstOrDefault();

            if (journal != null)
            {
                var journalsDetails = db.JournalsDetails.Where(x => x.JournalId == journal.Id).ToList();

                if (journalsDetails != null && journalsDetails.Count > 0)
                {
                    db.JournalsDetails.RemoveRange(journalsDetails);
                    db.SaveChanges();
                }

                db.Journals.Remove(journal);
                db.SaveChanges();
            }
        }

        public static void CreatePurchaseJournalDetails(ApplicationDbContext db, PurchaseDetails purchaseDetails)
        {
            Purchase purchase = db.Purchases.Find(purchaseDetails.PurchaseId);
            MasterItem masterItem = db.MasterItems.Find(purchaseDetails.MasterItemId);
            Journal journal = db.Journals.Where(x => x.Type == EnumJournalType.Purchase && x.PurchaseId == purchase.Id).FirstOrDefault();
            ChartOfAccount purchaseAccount = db.MasterBusinessUnitsAccounts.Where(x => x.MasterBusinessUnitId == purchase.MasterBusinessUnitId && x.MasterRegionId == purchase.MasterRegionId && x.Type == EnumBusinessUnitAccountType.PurchaseAccount).FirstOrDefault().ChartOfAccount;
            ChartOfAccount APAccount = db.MasterBusinessUnitsAccounts.Where(x => x.MasterBusinessUnitId == purchase.MasterBusinessUnitId && x.MasterRegionId == purchase.MasterRegionId && x.Type == EnumBusinessUnitAccountType.APAccount).FirstOrDefault().ChartOfAccount;

            JournalDetails journalDetails = new JournalDetails
            {
                JournalId = journal.Id,
                MasterRegionId = purchase.MasterRegionId,
                PurchaseDetailsId = purchaseDetails.Id,
                ChartOfAccountId = purchaseAccount.Id,
                Debit = purchaseDetails.Total,
                Credit = 0,
                Created = purchaseDetails.Created,
                Updated = purchaseDetails.Updated,
                UserId = purchaseDetails.UserId,
                Notes = "PEMBELIAN BARANG " + purchaseDetails.Quantity.ToString("N0") + " x " + masterItem.Code
            };

            db.JournalsDetails.Add(journalDetails);
            db.SaveChanges();

            JournalDetails JournalDetailsCredit = db.JournalsDetails.Where(x => x.JournalId == journal.Id && x.isMerged && x.ChartOfAccountId == APAccount.Id).FirstOrDefault();
            if (JournalDetailsCredit != null)
            {
                JournalDetailsCredit.Debit = 0;
                JournalDetailsCredit.Credit = GetTotalPurchase(db, purchase.Id, purchaseDetails.Id) + purchaseDetails.Total;
                JournalDetailsCredit.MasterRegionId = purchase.MasterRegionId;
                JournalDetailsCredit.ChartOfAccountId = APAccount.Id;
                JournalDetailsCredit.Notes = "PEMBELIAN NO. " + purchase.Code;
                JournalDetailsCredit.isMerged = true;
                JournalDetailsCredit.Created = purchaseDetails.Created;
                JournalDetailsCredit.Updated = purchaseDetails.Updated;
                JournalDetailsCredit.UserId = purchaseDetails.UserId;
                db.Entry(JournalDetailsCredit).State = EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                JournalDetailsCredit = new JournalDetails
                {
                    JournalId = journal.Id,
                    Debit = 0,
                    Credit = GetTotalPurchase(db, purchase.Id, purchaseDetails.Id) + purchaseDetails.Total,
                    ChartOfAccountId = APAccount.Id,
                    MasterRegionId = purchase.MasterRegionId,
                    Notes = "PEMBELIAN NO. " + purchase.Code,
                    isMerged = true,
                    Created = purchaseDetails.Created,
                    Updated = purchaseDetails.Updated,
                    UserId = purchaseDetails.UserId
                };

                db.JournalsDetails.Add(JournalDetailsCredit);
                db.SaveChanges();
            }

            journal.Debit = GetTotalJournalDebit(db, journal.Id, journalDetails.Id) + journalDetails.Debit;
            journal.Credit = journal.Debit;
            db.Entry(journal).State = EntityState.Modified;
            db.SaveChanges();
        }

        public static void UpdatePurchaseJournalDetails(ApplicationDbContext db, PurchaseDetails purchaseDetails)
        {
            Purchase purchase = db.Purchases.Find(purchaseDetails.PurchaseId);
            MasterItem masterItem = db.MasterItems.Find(purchaseDetails.MasterItemId);
            ChartOfAccount purchaseAccount = db.MasterBusinessUnitsAccounts.Where(x => x.MasterBusinessUnitId == purchase.MasterBusinessUnitId && x.MasterRegionId == purchase.MasterRegionId && x.Type == EnumBusinessUnitAccountType.PurchaseAccount).FirstOrDefault().ChartOfAccount;
            ChartOfAccount APAccount = db.MasterBusinessUnitsAccounts.Where(x => x.MasterBusinessUnitId == purchase.MasterBusinessUnitId && x.MasterRegionId == purchase.MasterRegionId && x.Type == EnumBusinessUnitAccountType.APAccount).FirstOrDefault().ChartOfAccount;
            JournalDetails journalDetails = db.JournalsDetails.Where(x => x.Journal.Type == EnumJournalType.Purchase && x.PurchaseDetailsId == purchaseDetails.Id).FirstOrDefault();
            Journal journal = db.Journals.Find(journalDetails.JournalId);

            db.Entry(journalDetails).State = EntityState.Modified;
            journalDetails.MasterRegionId = purchase.MasterRegionId;
            journalDetails.ChartOfAccountId = purchaseAccount.Id;
            journalDetails.Debit = purchaseDetails.Total;
            journalDetails.Credit = 0;
            journalDetails.Updated = purchaseDetails.Updated;
            journalDetails.UserId = purchaseDetails.UserId;
            journalDetails.Notes = "PEMBELIAN BARANG " + purchaseDetails.Quantity.ToString("N0") + " x " + masterItem.Code;
            db.SaveChanges();

            JournalDetails JournalDetailsCredit = db.JournalsDetails.Where(x => x.JournalId == journal.Id && x.isMerged && x.ChartOfAccountId == APAccount.Id).FirstOrDefault();

            JournalDetailsCredit.Debit = 0;
            JournalDetailsCredit.Credit = GetTotalPurchase(db, purchase.Id, purchaseDetails.Id) + purchaseDetails.Total;
            JournalDetailsCredit.MasterRegionId = purchase.MasterRegionId;
            JournalDetailsCredit.ChartOfAccountId = APAccount.Id;
            JournalDetailsCredit.Notes = "PEMBELIAN NO. " + purchase.Code;
            JournalDetailsCredit.isMerged = true;
            JournalDetailsCredit.Created = purchaseDetails.Created;
            JournalDetailsCredit.Updated = purchaseDetails.Updated;
            JournalDetailsCredit.UserId = purchaseDetails.UserId;
            db.Entry(JournalDetailsCredit).State = EntityState.Modified;
            db.SaveChanges();

            journal.Debit = GetTotalJournalDebit(db, journal.Id, journalDetails.Id) + journalDetails.Debit;
            journal.Credit = journal.Debit;
            db.Entry(journal).State = EntityState.Modified;
            db.SaveChanges();
        }

        public static void RemovePurchaseJournalDetails(ApplicationDbContext db, PurchaseDetails purchaseDetails)
        {
            JournalDetails journalDetails = db.JournalsDetails.Where(x => x.Journal.Type == EnumJournalType.Purchase && x.PurchaseDetailsId == purchaseDetails.Id).FirstOrDefault();
            Journal journal = db.Journals.Find(journalDetails.JournalId);

            journal.Debit = GetTotalJournalDebit(db, journal.Id, journalDetails.Id);
            journal.Credit = journal.Debit;
            db.Entry(journal).State = EntityState.Modified;
            db.SaveChanges();

            JournalDetails JournalDetailsCredit = db.JournalsDetails.Where(x => x.JournalId == journal.Id && x.isMerged).FirstOrDefault();
            if (JournalDetailsCredit != null)
            {
                JournalDetailsCredit.Credit = GetTotalPurchase(db, purchaseDetails.PurchaseId, purchaseDetails.Id);

                db.Entry(JournalDetailsCredit).State = EntityState.Modified;
                db.SaveChanges();
            }

            db.JournalsDetails.Remove(journalDetails);
            db.SaveChanges();
        }

        // End of Purchase

        public static decimal GetTotalSalesRequest(ApplicationDbContext db, int salesRequestId, int? salesRequestDetailsId = null)
        {
            decimal total = 0;
            List<SalesRequestDetails> salesRequestDetails = null;

            if (salesRequestDetailsId == null)
            {
                salesRequestDetails = db.SalesRequestsDetails.Where(x => x.SalesRequestId == salesRequestId).ToList();
            }
            else
            {
                salesRequestDetails = db.SalesRequestsDetails.Where(x => x.SalesRequestId == salesRequestId && x.Id != salesRequestDetailsId).ToList();
            }

            if (salesRequestDetails != null)
            {
                total = salesRequestDetails.Sum(y => y.Total);
            }

            return total;
        }

        public static decimal GetTotalSalesReturn(ApplicationDbContext db, int salesReturnId, int? salesReturnDetailsId = null)
        {
            decimal total = 0;
            List<SalesReturnDetails> salesReturnDetails = null;

            if (salesReturnDetailsId == null)
            {
                salesReturnDetails = db.SalesReturnsDetails.Where(x => x.SalesReturnId == salesReturnId).ToList();
            }
            else
            {
                salesReturnDetails = db.SalesReturnsDetails.Where(x => x.SalesReturnId == salesReturnId && x.Id != salesReturnDetailsId).ToList();
            }

            if (salesReturnDetails != null)
            {
                total = salesReturnDetails.Sum(y => y.Total);
            }

            return total;
        }

        public static decimal GetTotalSalesOrder(ApplicationDbContext db, int salesOrderId, int? salesOrderDetailsId = null)
        {
            decimal total = 0;
            List<SalesOrderDetails> salesOrderDetails = null;

            if (salesOrderDetailsId == null)
            {
                salesOrderDetails = db.SalesOrdersDetails.Where(x => x.SalesOrderId == salesOrderId).ToList();
            }
            else
            {
                salesOrderDetails = db.SalesOrdersDetails.Where(x => x.SalesOrderId == salesOrderId && x.Id != salesOrderDetailsId).ToList();
            }

            if (salesOrderDetails != null)
            {
                total = salesOrderDetails.Sum(y => y.Total);
            }

            return total;
        }

        public static decimal GetTotalSale(ApplicationDbContext db, int saleId, int? saleDetailsId = null)
        {
            decimal total = 0;
            List<SaleDetails> saleDetails = null;

            if (saleDetailsId == null)
            {
                saleDetails = db.SalesDetails.Where(x => x.SaleId == saleId).ToList();
            }
            else
            {
                saleDetails = db.SalesDetails.Where(x => x.SaleId == saleId && x.Id != saleDetailsId).ToList();
            }

            if (saleDetails != null)
            {
                total = saleDetails.Sum(y => y.Total);
            }

            return total;
        }

        public static decimal GetTotalJournalDebit(ApplicationDbContext db, int journalId, int? journalDetailId = null)
        {
            decimal total = 0;

            List<JournalDetails> journalDetails = null;

            if (journalDetailId == null)
                journalDetails = db.JournalsDetails.Where(x => x.JournalId == journalId).ToList();
            else
                journalDetails = db.JournalsDetails.Where(x => x.JournalId == journalId && x.Id != journalDetailId).ToList();

            if (journalDetails != null)
                total = journalDetails.Sum(y => y.Debit);

            return total;
        }

        public static decimal GetTotalJournalCredit(ApplicationDbContext db, int journalId, int? journalDetailId = null)
        {
            decimal total = 0;

            List<JournalDetails> journalDetails = null;

            if (journalDetailId == null)
                journalDetails = db.JournalsDetails.Where(x => x.JournalId == journalId).ToList();
            else
                journalDetails = db.JournalsDetails.Where(x => x.JournalId == journalId && x.Id != journalDetailId).ToList();

            if (journalDetails != null)
                total = journalDetails.Sum(y => y.Credit);

            return total;
        }

        // Begin of Bank 

        public static decimal GetTotalBankTransactionDetails(ApplicationDbContext db, int bankTransactionId, int? bankTransactionDetailsId = null)
        {
            decimal total = 0;
            List<BankTransactionDetails> bankTransactionsDetails = null;

            if (bankTransactionDetailsId == null)
                bankTransactionsDetails = db.BankTransactionsDetails.Where(x => x.BankTransactionId == bankTransactionId).ToList();
            else
                bankTransactionsDetails = db.BankTransactionsDetails.Where(x => x.BankTransactionId == bankTransactionId && x.Id != bankTransactionDetailsId).ToList();

            if (bankTransactionsDetails != null)
                total = bankTransactionsDetails.Sum(y => y.Total);

            return total;
        }

        public static decimal GetTotalBankTransactionDetailsHeader(ApplicationDbContext db, int bankTransactionId, int? bankTransactionDetailsHeaderId = null)
        {
            decimal total = 0;
            List<BankTransactionDetailsHeader> bankTransactionsDetailsHeader = null;

            if (bankTransactionDetailsHeaderId == null)
                bankTransactionsDetailsHeader = db.BankTransactionsDetailsHeader.Where(x => x.BankTransactionId == bankTransactionId).ToList();
            else
                bankTransactionsDetailsHeader = db.BankTransactionsDetailsHeader.Where(x => x.BankTransactionId == bankTransactionId && x.Id != bankTransactionDetailsHeaderId).ToList();

            if (bankTransactionsDetailsHeader != null)
                total = bankTransactionsDetailsHeader.Sum(y => y.Total);

            return total;
        }
        
        public static void CreateBankJournal(ApplicationDbContext db, BankTransaction bankTransaction)
        {
            Journal journal = new Journal
            {
                Code = bankTransaction.Code,
                Date = bankTransaction.Date,
                MasterBusinessUnitId = bankTransaction.MasterBusinessUnitId,
                Type = EnumJournalType.BankTransaction,
                Debit = 0,
                Credit = 0,
                BankTransactionId = bankTransaction.Id,
                Active = bankTransaction.Active,
                Created = bankTransaction.Created,
                Updated = bankTransaction.Updated,
                UserId = bankTransaction.UserId
            };

            if (string.IsNullOrEmpty(bankTransaction.Notes))
            {
                if (bankTransaction.TransactionType == EnumBankTransactionType.In)
                    journal.Notes = "KAS/BANK MASUK NO. " + bankTransaction.Code;
                else
                    journal.Notes = "KAS/BANK KELUAR NO. " + bankTransaction.Code;
            }
            else
                journal.Notes = bankTransaction.Notes;

            db.Journals.Add(journal);
            db.SaveChanges();
        }

        public static void UpdateBankJournal(ApplicationDbContext db, BankTransaction bankTransaction)
        {
            Journal journal = db.Journals.Where(x => x.Type == EnumJournalType.BankTransaction && x.BankTransactionId == bankTransaction.Id).FirstOrDefault();

            var bankTransactionsDetails = db.BankTransactionsDetails.Where(x => x.BankTransactionId == bankTransaction.Id).ToList();
            foreach (BankTransactionDetails bankTransactionDetails in bankTransactionsDetails)
            {
                UpdateBankJournalDetails(db, bankTransactionDetails);
            }

            var bankTransactionsDetailsHeader = db.BankTransactionsDetailsHeader.Where(x => x.BankTransactionId == bankTransaction.Id).ToList();
            foreach (BankTransactionDetailsHeader bankTransactionDetailsHeader in bankTransactionsDetailsHeader)
            {
                UpdateBankJournalDetailsHeader(db, bankTransactionDetailsHeader);
            }

            db.Entry(journal).State = EntityState.Modified;
            journal.Code = bankTransaction.Code;
            journal.Date = bankTransaction.Date;
            journal.MasterBusinessUnitId = bankTransaction.MasterBusinessUnitId;
            journal.BankTransactionId = bankTransaction.Id;
            journal.Active = bankTransaction.Active;
            journal.Updated = bankTransaction.Updated;
            journal.UserId = bankTransaction.UserId;

            if (string.IsNullOrEmpty(bankTransaction.Notes))
            {
                if (bankTransaction.TransactionType == EnumBankTransactionType.In)
                    journal.Notes = "KAS/BANK MASUK NO. " + bankTransaction.Code;
                else
                    journal.Notes = "KAS/BANK KELUAR NO. " + bankTransaction.Code;
            }
            else
                journal.Notes = bankTransaction.Notes;

            db.SaveChanges();
        }

        public static void DeleteBankJournals(ApplicationDbContext db, BankTransaction bankTransaction)
        {
            Journal journal = db.Journals.Where(x => x.Type == EnumJournalType.BankTransaction && x.BankTransactionId == bankTransaction.Id).FirstOrDefault();

            var journalsDetails = db.JournalsDetails.Where(x => x.JournalId == journal.Id).ToList();

            if (journalsDetails != null)
            {
                db.JournalsDetails.RemoveRange(journalsDetails);
                db.SaveChanges();
            }

            db.Journals.Remove(journal);
            db.SaveChanges();
        }

        public static void CreateBankJournalDetails(ApplicationDbContext db, BankTransactionDetails bankTransactionDetails)
        {
            BankTransaction bankTransaction = db.BankTransactions.Find(bankTransactionDetails.BankTransactionId);
            Journal journal = db.Journals.Where(x => x.Type == EnumJournalType.BankTransaction && x.BankTransactionId == bankTransaction.Id).FirstOrDefault();
            
            JournalDetails journalDetails = new JournalDetails
            {
                JournalId = journal.Id,
                MasterRegionId = bankTransaction.MasterRegionId,
                ChartOfAccountId = bankTransactionDetails.ChartOfAccountId,
                BankTransactionDetailsId = bankTransactionDetails.Id,
                Created = bankTransactionDetails.Created,
                Updated = bankTransactionDetails.Updated,
                UserId = bankTransactionDetails.UserId
            };

            if (string.IsNullOrEmpty(bankTransaction.Notes))
            {
                if (bankTransaction.TransactionType == EnumBankTransactionType.In)
                    journalDetails.Notes = "KAS/BANK MASUK NO. " + bankTransaction.Code;
                else
                    journalDetails.Notes = "KAS/BANK KELUAR NO. " + bankTransaction.Code;
            }
            else
                journalDetails.Notes = bankTransactionDetails.Notes;

            if (bankTransaction.TransactionType == EnumBankTransactionType.In)
            {
                if (bankTransactionDetails.Total < 0)
                {
                    journalDetails.Credit = 0;
                    journalDetails.Debit = bankTransactionDetails.Total * -1;
                }
                else
                {
                    journalDetails.Debit = 0;
                    journalDetails.Credit = bankTransactionDetails.Total;
                }
            }
            else
            {
                if (bankTransactionDetails.Total < 0)
                {
                    journalDetails.Debit = 0;
                    journalDetails.Credit = bankTransactionDetails.Total * -1;
                }
                else
                {
                    journalDetails.Credit = 0;
                    journalDetails.Debit = bankTransactionDetails.Total;
                }
            }

            db.JournalsDetails.Add(journalDetails);
            db.SaveChanges();

            journal.Debit = GetTotalJournalDebit(db, journal.Id, journalDetails.Id) + journalDetails.Debit;
            journal.Credit = GetTotalJournalCredit(db, journal.Id, journalDetails.Id) + journalDetails.Credit;
            db.Entry(journal).State = EntityState.Modified;
            db.SaveChanges();
        }

        public static void UpdateBankJournalDetails(ApplicationDbContext db, BankTransactionDetails bankTransactionDetails)
        {
            BankTransaction bankTransaction = db.BankTransactions.Find(bankTransactionDetails.BankTransactionId);
            JournalDetails journalDetails = db.JournalsDetails.Where(x => x.Journal.Type == EnumJournalType.BankTransaction && x.BankTransactionDetailsId == bankTransactionDetails.Id).FirstOrDefault();

            if (journalDetails != null)
            {
                Journal journal = db.Journals.Find(journalDetails.JournalId);

                db.Entry(journalDetails).State = EntityState.Modified;

                journalDetails.MasterRegionId = bankTransaction.MasterRegionId;
                journalDetails.ChartOfAccountId = bankTransactionDetails.ChartOfAccountId;
                journalDetails.Updated = bankTransactionDetails.Updated;
                journalDetails.UserId = bankTransactionDetails.UserId;

                if (string.IsNullOrEmpty(bankTransaction.Notes))
                {
                    if (bankTransaction.TransactionType == EnumBankTransactionType.In)
                        journalDetails.Notes = "KAS/BANK MASUK NO. " + bankTransaction.Code;
                    else
                        journalDetails.Notes = "KAS/BANK KELUAR NO. " + bankTransaction.Code;
                }
                else
                    journalDetails.Notes = bankTransactionDetails.Notes;

                if (bankTransaction.TransactionType == EnumBankTransactionType.In)
                {
                    if (bankTransactionDetails.Total < 0)
                    {
                        journalDetails.Credit = 0;
                        journalDetails.Debit = bankTransactionDetails.Total * -1;
                    }
                    else
                    {
                        journalDetails.Debit = 0;
                        journalDetails.Credit = bankTransactionDetails.Total;
                    }
                }
                else
                {
                    if (bankTransactionDetails.Total < 0)
                    {
                        journalDetails.Debit = 0;
                        journalDetails.Credit = bankTransactionDetails.Total * -1;
                    }
                    else
                    {
                        journalDetails.Credit = 0;
                        journalDetails.Debit = bankTransactionDetails.Total;
                    }
                }

                db.SaveChanges();

                journal.Debit = GetTotalJournalDebit(db, journal.Id, journalDetails.Id) + journalDetails.Debit;
                journal.Credit = GetTotalJournalCredit(db, journal.Id, journalDetails.Id) + journalDetails.Credit;
                db.Entry(journal).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public static void RemoveBankJournalDetails(ApplicationDbContext db, BankTransactionDetails bankTransactionDetails)
        {
            Journal journal = db.Journals.Where(x => x.Type == EnumJournalType.BankTransaction && x.BankTransactionId == bankTransactionDetails.BankTransactionId).FirstOrDefault();            
            List<JournalDetails> journalsDetails = db.JournalsDetails.Where(x => x.JournalId == journal.Id && x.BankTransactionDetailsId == bankTransactionDetails.Id).ToList();

            foreach (JournalDetails journalDetails in journalsDetails)
            {
                if (journalDetails.Credit == 0)
                    journal.Debit = GetTotalJournalDebit(db, journal.Id, journalDetails.Id);
                else
                    journal.Credit = GetTotalJournalCredit(db, journal.Id, journalDetails.Id);
            }

            db.Entry(journal).State = EntityState.Modified;
            db.SaveChanges();

            if (journalsDetails != null)
            {
                db.JournalsDetails.RemoveRange(journalsDetails);
                db.SaveChanges();
            }
        }

        public static void CreateBankJournalDetailsHeader(ApplicationDbContext db, BankTransactionDetailsHeader bankTransactionDetailsHeader)
        {
            BankTransaction bankTransaction = db.BankTransactions.Find(bankTransactionDetailsHeader.BankTransactionId);
            MasterBank masterBank = db.MasterBanks.Find(bankTransactionDetailsHeader.MasterBankId);
            Journal journal = db.Journals.Where(x => x.Type == EnumJournalType.BankTransaction && x.BankTransactionId == bankTransactionDetailsHeader.BankTransactionId).FirstOrDefault();

            JournalDetails journalDetails = new JournalDetails
            {
                JournalId = journal.Id,
                MasterRegionId = bankTransaction.MasterRegionId,
                ChartOfAccountId = masterBank.ChartOfAccountId,
                BankTransactionDetailsHeaderId = bankTransactionDetailsHeader.Id,
                Created = bankTransactionDetailsHeader.Created,
                Updated = bankTransactionDetailsHeader.Updated,
                UserId = bankTransactionDetailsHeader.UserId,
            };

            if (string.IsNullOrEmpty(bankTransaction.Notes))
            {
                if (bankTransaction.TransactionType == EnumBankTransactionType.In)
                    journalDetails.Notes = "KAS/BANK MASUK NO. " + bankTransaction.Code;
                else
                    journalDetails.Notes = "KAS/BANK KELUAR NO. " + bankTransaction.Code;
            }
            else
                journalDetails.Notes = bankTransactionDetailsHeader.Notes;

            if (bankTransaction.TransactionType == EnumBankTransactionType.In)
            {
                if (bankTransactionDetailsHeader.Total < 0)
                {
                    journalDetails.Debit = 0;
                    journalDetails.Credit = bankTransactionDetailsHeader.Total * -1;
                }
                else
                {
                    journalDetails.Credit = 0;
                    journalDetails.Debit = bankTransactionDetailsHeader.Total;
                }
            }
            else
            {
                if (bankTransactionDetailsHeader.Total < 0)
                {
                    journalDetails.Credit = 0;
                    journalDetails.Debit = bankTransactionDetailsHeader.Total * -1;
                }
                else
                {
                    journalDetails.Debit = 0;
                    journalDetails.Credit = bankTransactionDetailsHeader.Total;
                }
            }

            db.JournalsDetails.Add(journalDetails);
            db.SaveChanges();

            journal.Debit = GetTotalJournalDebit(db, journal.Id, journalDetails.Id) + journalDetails.Debit;
            journal.Credit = GetTotalJournalCredit(db, journal.Id, journalDetails.Id) + journalDetails.Credit;
            db.Entry(journal).State = EntityState.Modified;
            db.SaveChanges();
        }

        public static void UpdateBankJournalDetailsHeader(ApplicationDbContext db, BankTransactionDetailsHeader bankTransactionDetailsHeader)
        {
            BankTransaction bankTransaction = db.BankTransactions.Find(bankTransactionDetailsHeader.BankTransactionId);
            JournalDetails journalDetails = db.JournalsDetails.Where(x => x.Journal.Type == EnumJournalType.BankTransaction && x.BankTransactionDetailsHeaderId == bankTransactionDetailsHeader.Id).FirstOrDefault();
            MasterBank masterBank = db.MasterBanks.Find(bankTransactionDetailsHeader.MasterBankId);


            if (journalDetails != null)
            {
                Journal journal = db.Journals.Find(journalDetails.JournalId);

                db.Entry(journalDetails).State = EntityState.Modified;

                journalDetails.MasterRegionId = bankTransaction.MasterRegionId;
                journalDetails.ChartOfAccountId = masterBank.ChartOfAccountId;
                journalDetails.Updated = bankTransactionDetailsHeader.Updated;
                journalDetails.UserId = bankTransactionDetailsHeader.UserId;

                if (string.IsNullOrEmpty(bankTransaction.Notes))
                {
                    if (bankTransaction.TransactionType == EnumBankTransactionType.In)
                        journalDetails.Notes = "KAS/BANK MASUK NO. " + bankTransaction.Code;
                    else
                        journalDetails.Notes = "KAS/BANK KELUAR NO. " + bankTransaction.Code;
                }
                else
                    journalDetails.Notes = bankTransactionDetailsHeader.Notes;

                if (bankTransaction.TransactionType == EnumBankTransactionType.In)
                {
                    if (bankTransactionDetailsHeader.Total < 0)
                    {
                        journalDetails.Debit = 0;
                        journalDetails.Credit = bankTransactionDetailsHeader.Total * -1;
                    }
                    else
                    {
                        journalDetails.Credit = 0;
                        journalDetails.Debit = bankTransactionDetailsHeader.Total;
                    }
                }
                else
                {
                    if (bankTransactionDetailsHeader.Total < 0)
                    {
                        journalDetails.Credit = 0;
                        journalDetails.Debit = bankTransactionDetailsHeader.Total * -1;
                    }
                    else
                    {
                        journalDetails.Debit = 0;
                        journalDetails.Credit = bankTransactionDetailsHeader.Total;
                    }
                }

                db.SaveChanges();

                journal.Debit = GetTotalJournalDebit(db, journal.Id, journalDetails.Id) + journalDetails.Debit;
                journal.Credit = GetTotalJournalCredit(db, journal.Id, journalDetails.Id) + journalDetails.Credit;
                db.Entry(journal).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public static void RemoveBankJournalDetailsHeader(ApplicationDbContext db, BankTransactionDetailsHeader bankTransactionDetailsHeader)
        {
            Journal journal = db.Journals.Where(x => x.Type == EnumJournalType.BankTransaction && x.BankTransactionId == bankTransactionDetailsHeader.BankTransactionId).FirstOrDefault();
            List<JournalDetails> journalsDetails = db.JournalsDetails.Where(x => x.JournalId == journal.Id && x.BankTransactionDetailsHeaderId == bankTransactionDetailsHeader.Id).ToList();

            foreach (JournalDetails journalDetails in journalsDetails)
            {
                if (journalDetails.Credit == 0)
                    journal.Debit = GetTotalJournalDebit(db, journal.Id, journalDetails.Id);
                else
                    journal.Credit = GetTotalJournalCredit(db, journal.Id, journalDetails.Id);
            }

            db.Entry(journal).State = EntityState.Modified;
            db.SaveChanges();

            if (journalsDetails != null)
            {
                db.JournalsDetails.RemoveRange(journalsDetails);
                db.SaveChanges();
            }
        }

        // End of Bank 

        // Begin of Advance Repayment
        public static decimal GetTotalAdvanceRepayment(ApplicationDbContext db, int advanceRepaymentId, int? advanceRepaymentDetailsId = null)
        {
            decimal total = 0;
            List<AdvanceRepaymentDetails> totalAdvanceRepaymentDetails = null;

            if (advanceRepaymentDetailsId == null)
                totalAdvanceRepaymentDetails = db.AdvanceRepaymentsDetails.Where(x => x.AdvanceRepaymentId == advanceRepaymentId).ToList();
            else
                totalAdvanceRepaymentDetails = db.AdvanceRepaymentsDetails.Where(x => x.AdvanceRepaymentId == advanceRepaymentId && x.Id != advanceRepaymentDetailsId).ToList();

            if (totalAdvanceRepaymentDetails != null)
                total = totalAdvanceRepaymentDetails.Sum(y => y.Total);

            return total;
        }

        public static decimal GetTotalAdvanceRepaymentAllocated(ApplicationDbContext db, AdvanceRepayment advanceRepayment, int? repaymentDetailsHeaderId = null)
        {
            decimal total = 0;

            List<RepaymentDetailsHeader> repaymentsDetailsHeader = null;

            if (repaymentDetailsHeaderId == null)
                repaymentsDetailsHeader = db.RepaymentsDetailsHeader.Where(x => x.Type == EnumRepaymentType.AdvanceRepayment && x.AdvanceRepaymentId == advanceRepayment.Id).ToList();
            else
                repaymentsDetailsHeader = db.RepaymentsDetailsHeader.Where(x => x.Type == EnumRepaymentType.AdvanceRepayment && x.AdvanceRepaymentId == advanceRepayment.Id && x.Id != repaymentDetailsHeaderId).ToList();

            if (repaymentsDetailsHeader != null)
                total = repaymentsDetailsHeader.Sum(y => y.Total);

            return total;
        }

        public static void CreateAdvanceRepaymentJournal(ApplicationDbContext db, AdvanceRepayment advanceRepayment)
        {
            Journal journal = new Journal
            {
                Code = advanceRepayment.Code,
                Date = advanceRepayment.Date,
                MasterBusinessUnitId = advanceRepayment.MasterBusinessUnitId,
                Type = EnumJournalType.AdvanceRepayment,
                Debit = 0,
                Credit = 0,
                AdvanceRepaymentId = advanceRepayment.Id,                
                Active = advanceRepayment.Active,
                Created = advanceRepayment.Created,
                Updated = advanceRepayment.Updated,
                UserId = advanceRepayment.UserId
            };

            if (string.IsNullOrEmpty(advanceRepayment.Notes))
                journal.Notes = "UANG MUKA PENJUALAN NO. " + advanceRepayment.Code;
            else
                journal.Notes = advanceRepayment.Notes;

            db.Journals.Add(journal);
            db.SaveChanges();
        }

        public static void UpdateAdvanceRepaymentJournal(ApplicationDbContext db, AdvanceRepayment advanceRepayment)
        {
            Journal journal = db.Journals.Where(x => x.Type == EnumJournalType.AdvanceRepayment && x.AdvanceRepaymentId == advanceRepayment.Id).FirstOrDefault();

            var advanceRepaymentsDetails = db.AdvanceRepaymentsDetails.Where(x => x.AdvanceRepaymentId == advanceRepayment.Id).ToList();
            foreach (AdvanceRepaymentDetails advanceRepaymentDetails in advanceRepaymentsDetails)
            {
                UpdateAdvanceRepaymentJournalDetails(db, advanceRepaymentDetails);
            }

            db.Entry(journal).State = EntityState.Modified;

            journal.Code = advanceRepayment.Code;
            journal.Date = advanceRepayment.Date;
            journal.MasterBusinessUnitId = advanceRepayment.MasterBusinessUnitId;
            journal.Active = advanceRepayment.Active;
            journal.Updated = advanceRepayment.Updated;
            journal.UserId = advanceRepayment.UserId;

            if (string.IsNullOrEmpty(advanceRepayment.Notes))
                journal.Notes = "UANG MUKA PENJUALAN NO. " + advanceRepayment.Code;
            else
                journal.Notes = advanceRepayment.Notes;

            db.SaveChanges();
        }

        public static void DeleteAdvanceRepaymentJournals(ApplicationDbContext db, AdvanceRepayment advanceRepayment)
        {
            Journal journal = db.Journals.Where(x => x.Type == EnumJournalType.AdvanceRepayment && x.AdvanceRepaymentId == advanceRepayment.Id).FirstOrDefault();

            if (journal != null)
            {
                var journalsDetails = db.JournalsDetails.Where(x => x.JournalId == journal.Id).ToList();

                if (journalsDetails != null && journalsDetails.Count > 0)
                {
                    db.JournalsDetails.RemoveRange(journalsDetails);
                    db.SaveChanges();
                }

                db.Journals.Remove(journal);
                db.SaveChanges();
            }
        }

        public static void CreateAdvanceRepaymentJournalDetails(ApplicationDbContext db, AdvanceRepaymentDetails advanceRepaymentDetails)
        {
            AdvanceRepayment advanceRepayment = db.AdvanceRepayments.Find(advanceRepaymentDetails.AdvanceRepaymentId);
            Journal journal = db.Journals.Where(x => x.Type == EnumJournalType.AdvanceRepayment && x.AdvanceRepaymentId == advanceRepayment.Id).FirstOrDefault();

            JournalDetails journalDetails = new JournalDetails
            {
                JournalId = journal.Id,
                MasterRegionId = advanceRepayment.MasterRegionId,
                AdvanceRepaymentDetailsId = advanceRepaymentDetails.Id,
                Created = advanceRepaymentDetails.Created,
                Updated = advanceRepaymentDetails.Updated,
                UserId = advanceRepaymentDetails.UserId
            };

            if (string.IsNullOrEmpty(advanceRepaymentDetails.Notes))
                journalDetails.Notes = "UANG MUKA PENJUALAN NO. " + advanceRepayment.Code;
            else
                journalDetails.Notes = advanceRepaymentDetails.Notes;

            if (advanceRepaymentDetails.Type == EnumAdvanceRepaymentType.Bank || advanceRepaymentDetails.Type == EnumAdvanceRepaymentType.Cash)
            {
                MasterBank masterBank = db.MasterBanks.Find(advanceRepaymentDetails.MasterBankId);
                journalDetails.ChartOfAccountId = masterBank.ChartOfAccountId;
            }
            else if (advanceRepaymentDetails.Type == EnumAdvanceRepaymentType.Cheque)
            {
                journalDetails.ChartOfAccountId = db.MasterBusinessUnitsAccounts.Where(x => x.MasterBusinessUnitId == advanceRepayment.MasterBusinessUnitId && x.MasterRegionId == advanceRepayment.MasterRegionId && x.Type == EnumBusinessUnitAccountType.ChequeReceivablesAccount).FirstOrDefault().ChartOfAccount.Id;
            }
            else if (advanceRepaymentDetails.Type == EnumAdvanceRepaymentType.MasterCost)
            {
                MasterCost masterCost = db.MasterCosts.Find(advanceRepaymentDetails.MasterCostId);

                journalDetails.ChartOfAccountId = masterCost.ChartOfAccount.Id;
            }

            if (advanceRepaymentDetails.Total >= 0)
            {
                journalDetails.Debit = advanceRepaymentDetails.Total;
                journalDetails.Credit = 0;
            }
            else
            {
                journalDetails.Debit = 0;
                journalDetails.Credit = advanceRepaymentDetails.Total * -1;
            }

            db.JournalsDetails.Add(journalDetails);
            db.SaveChanges();

            JournalDetails JournalDetailsCredit = db.JournalsDetails.Where(x => x.JournalId == journal.Id && x.isMerged).FirstOrDefault();
            if (JournalDetailsCredit != null)
            {
                JournalDetailsCredit.Debit = 0;
                JournalDetailsCredit.Credit = GetTotalAdvanceRepayment(db, advanceRepayment.Id, advanceRepaymentDetails.Id) + advanceRepaymentDetails.Total;
                JournalDetailsCredit.MasterRegionId = advanceRepayment.MasterRegionId;
                JournalDetailsCredit.ChartOfAccountId = db.MasterBusinessUnitsAccounts.Where(x => x.MasterBusinessUnitId == advanceRepayment.MasterBusinessUnitId && x.MasterRegionId == advanceRepayment.MasterRegionId && x.Type == EnumBusinessUnitAccountType.AdvanceRepaymentAccount).FirstOrDefault().ChartOfAccount.Id;
                JournalDetailsCredit.Notes = "UANG MUKA PENJUALAN NO. " + advanceRepayment.Code;
                JournalDetailsCredit.isMerged = true;
                JournalDetailsCredit.Created = advanceRepaymentDetails.Created;
                JournalDetailsCredit.Updated = advanceRepaymentDetails.Updated;
                JournalDetailsCredit.UserId = advanceRepaymentDetails.UserId;
                db.Entry(JournalDetailsCredit).State = EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                JournalDetailsCredit = new JournalDetails
                {
                    JournalId = journal.Id,
                    Debit = 0,
                    Credit = GetTotalAdvanceRepayment(db, advanceRepayment.Id, advanceRepaymentDetails.Id) + advanceRepaymentDetails.Total,
                    ChartOfAccountId = db.MasterBusinessUnitsAccounts.Where(x => x.MasterBusinessUnitId == advanceRepayment.MasterBusinessUnitId && x.MasterRegionId == advanceRepayment.MasterRegionId && x.Type == EnumBusinessUnitAccountType.AdvanceRepaymentAccount).FirstOrDefault().ChartOfAccount.Id,
                    MasterRegionId = advanceRepayment.MasterRegionId,
                    Notes = "UANG MUKA PENJUALAN NO. " + advanceRepayment.Code,
                    isMerged = true,
                    Created = advanceRepaymentDetails.Created,
                    Updated = advanceRepaymentDetails.Updated,
                    UserId = advanceRepaymentDetails.UserId
                };

                db.JournalsDetails.Add(JournalDetailsCredit);
                db.SaveChanges();
            }

            journal.Debit = GetTotalJournalDebit(db, journal.Id, journalDetails.Id) + journalDetails.Debit;
            journal.Credit = journal.Debit;
            db.Entry(journal).State = EntityState.Modified;
            db.SaveChanges();
        }

        public static void UpdateAdvanceRepaymentJournalDetails(ApplicationDbContext db, AdvanceRepaymentDetails advanceRepaymentDetails)
        {
            AdvanceRepayment advanceRepayment = db.AdvanceRepayments.Find(advanceRepaymentDetails.AdvanceRepaymentId);
            JournalDetails journalDetails = db.JournalsDetails.Where(x => x.Journal.Type == EnumJournalType.AdvanceRepayment && x.AdvanceRepaymentDetailsId == advanceRepaymentDetails.Id).FirstOrDefault();

            if (journalDetails != null)
            {
                Journal journal = db.Journals.Find(journalDetails.JournalId);

                db.Entry(journalDetails).State = EntityState.Modified;
                journalDetails.MasterRegionId = advanceRepayment.MasterRegionId;
                journalDetails.Updated = advanceRepaymentDetails.Updated;
                journalDetails.UserId = advanceRepaymentDetails.UserId;

                if (string.IsNullOrEmpty(advanceRepaymentDetails.Notes))
                    journalDetails.Notes = "UANG MUKA PENJUALAN NO. " + advanceRepayment.Code;
                else
                    journalDetails.Notes = advanceRepaymentDetails.Notes;

                if (advanceRepaymentDetails.Type == EnumAdvanceRepaymentType.Bank || advanceRepaymentDetails.Type == EnumAdvanceRepaymentType.Cash)
                {
                    MasterBank masterBank = db.MasterBanks.Find(advanceRepaymentDetails.MasterBankId);
                    journalDetails.ChartOfAccountId = masterBank.ChartOfAccountId;
                }
                else if (advanceRepaymentDetails.Type == EnumAdvanceRepaymentType.Cheque)
                {
                    journalDetails.ChartOfAccountId = db.MasterBusinessUnitsAccounts.Where(x => x.MasterBusinessUnitId == advanceRepayment.MasterBusinessUnitId && x.MasterRegionId == advanceRepayment.MasterRegionId && x.Type == EnumBusinessUnitAccountType.ChequeReceivablesAccount).FirstOrDefault().ChartOfAccount.Id;
                }
                else if (advanceRepaymentDetails.Type == EnumAdvanceRepaymentType.MasterCost)
                {
                    MasterCost masterCost = db.MasterCosts.Find(advanceRepaymentDetails.MasterCostId);

                    journalDetails.ChartOfAccountId = masterCost.ChartOfAccount.Id;
                }

                if (advanceRepaymentDetails.Total >= 0)
                {
                    journalDetails.Debit = advanceRepaymentDetails.Total;
                    journalDetails.Credit = 0;
                }
                else
                {
                    journalDetails.Debit = 0;
                    journalDetails.Credit = advanceRepaymentDetails.Total * -1;
                }
                db.SaveChanges();

                JournalDetails JournalDetailsCredit = db.JournalsDetails.Where(x => x.JournalId == journal.Id && x.isMerged).FirstOrDefault();
                if (JournalDetailsCredit != null)
                {
                    JournalDetailsCredit.Debit = 0;
                    JournalDetailsCredit.Credit = GetTotalAdvanceRepayment(db, advanceRepayment.Id, advanceRepaymentDetails.Id) + advanceRepaymentDetails.Total;
                    JournalDetailsCredit.ChartOfAccountId = db.MasterBusinessUnitsAccounts.Where(x => x.MasterBusinessUnitId == advanceRepayment.MasterBusinessUnitId && x.MasterRegionId == advanceRepayment.MasterRegionId && x.Type == EnumBusinessUnitAccountType.AdvanceRepaymentAccount).FirstOrDefault().ChartOfAccount.Id;
                    JournalDetailsCredit.MasterRegionId = advanceRepayment.MasterRegionId;
                    JournalDetailsCredit.Notes = "UANG MUKA PENJUALAN NO. " + advanceRepayment.Code;
                    JournalDetailsCredit.isMerged = true;
                    JournalDetailsCredit.Created = advanceRepaymentDetails.Created;
                    JournalDetailsCredit.Updated = advanceRepaymentDetails.Updated;
                    JournalDetailsCredit.UserId = advanceRepaymentDetails.UserId;
                    db.Entry(JournalDetailsCredit).State = EntityState.Modified;
                    db.SaveChanges();
                }

                journal.Debit = GetTotalJournalDebit(db, journal.Id, journalDetails.Id) + journalDetails.Debit;
                journal.Credit = journal.Debit;
                db.Entry(journal).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public static void RemoveAdvanceRepaymentJournalDetails(ApplicationDbContext db, AdvanceRepaymentDetails advanceRepaymentDetails)
        {
            JournalDetails journalDetails = db.JournalsDetails.Where(x => x.AdvanceRepaymentDetailsId == advanceRepaymentDetails.Id).FirstOrDefault();
            Journal journal = db.Journals.Find(journalDetails.JournalId);

            journal.Debit = GetTotalJournalDebit(db, journal.Id, journalDetails.Id);
            journal.Credit = journal.Debit;
            db.Entry(journal).State = EntityState.Modified;
            db.SaveChanges();

            JournalDetails JournalDetailsCredit = db.JournalsDetails.Where(x => x.JournalId == journal.Id && x.isMerged).FirstOrDefault();
            if (JournalDetailsCredit != null)
            {
                JournalDetailsCredit.Credit = GetTotalAdvanceRepayment(db, advanceRepaymentDetails.AdvanceRepaymentId, advanceRepaymentDetails.Id);

                db.Entry(JournalDetailsCredit).State = EntityState.Modified;
                db.SaveChanges();
            }

            db.JournalsDetails.Remove(journalDetails);
            db.SaveChanges();
        }
        
        // End of Advance Repayment

        public static string EncodeTo64(string m_enc)
        {
            byte[] toEncodeAsBytes =
            Encoding.UTF8.GetBytes(m_enc);
            string returnValue =
            Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }

        public static string DecodeFrom64(string m_enc)
        {
            byte[] encodedDataAsBytes =
            Convert.FromBase64String(m_enc);
            string returnValue =
            Encoding.UTF8.GetString(encodedDataAsBytes);
            return returnValue;
        }

        public static string RomanNumeralFrom(int number)
        {
            if (number < 1) return string.Empty;
            if (number >= 1000) return "M" + RomanNumeralFrom(number - 1000);
            if (number >= 900) return "CM" + RomanNumeralFrom(number - 900);
            if (number >= 500) return "D" + RomanNumeralFrom(number - 500);
            if (number >= 400) return "CD" + RomanNumeralFrom(number - 400);
            if (number >= 100) return "C" + RomanNumeralFrom(number - 100);
            if (number >= 90) return "XC" + RomanNumeralFrom(number - 90);
            if (number >= 50) return "L" + RomanNumeralFrom(number - 50);
            if (number >= 40) return "XL" + RomanNumeralFrom(number - 40);
            if (number >= 10) return "X" + RomanNumeralFrom(number - 10);
            if (number >= 9) return "IX" + RomanNumeralFrom(number - 9);
            if (number >= 5) return "V" + RomanNumeralFrom(number - 5);
            if (number >= 4) return "IV" + RomanNumeralFrom(number - 4);
            if (number >= 1) return "I" + RomanNumeralFrom(number - 1);
            return string.Empty;
        }

        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        public static string GetUserInfo()
        {
            string ip = System.Web.HttpContext.Current.Request.UserHostAddress;
            var userAgent = System.Web.HttpContext.Current.Request.UserAgent;

            if (ip == "::1")
                ip = "127.0.0.1";

            if (string.IsNullOrEmpty(ip))
                ip = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];

            return "IP Address : " + ip + "; Browser : " + userAgent;
        }
    }

    public static class NumberToEnglish
    {
        public static String changeNumericToWords(double numb)
        {
            return changeToWords(numb, false);
        }
        public static String changeCurrencyToWords(double numb)
        {
            return changeToWords(numb, true);
        }
        private static String changeToWords(double number, bool isCurrency)
        {
            string decimalPart = "";
            if (number - Math.Truncate(number) > 0)
            {
                decimalPart = " And " + NumberToWords((int)((number - Math.Truncate(number)) * 100)) + " Cents";
            }
            if (isCurrency)
            {
                return NumberToWords((int)number) + " Rupiah " + decimalPart;
            }
            else
            {
                return NumberToWords((int)number) + decimalPart;
            }
        }
        private static String NumberToWords(int number)
        {

            if (number == 0)
                return "zero";

            if (number < 0)
                return "minus " + NumberToWords(Math.Abs(number));

            string words = "";

            if ((number / 1000000000) > 0)
            {
                words += NumberToWords(number / 1000000000) + " Billion ";
                number %= 1000000000;
            }

            if ((number / 1000000) > 0)
            {
                words += NumberToWords(number / 1000000) + " Million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " Thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " Hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "And ";

                var unitsMap = new[] { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
                var tensMap = new[] { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }

            return words;
        }
    }

    public static class TerbilangExtension
    {
        public static string Terbilang(this int value)
        {
            return ((decimal)value).Terbilang();
        }

        public static string Terbilang(this long value)
        {
            return ((decimal)value).Terbilang();
        }

        public static string Terbilang(this float value)
        {
            return ((decimal)Math.Truncate(value)).Terbilang();
        }

        public static string Terbilang(this double value)
        {
            return ((decimal)Math.Truncate(value)).Terbilang();
        }

        public static string Terbilang(this decimal value)
        {
            if (value <= 0) return string.Empty;

            StringBuilder sb = new StringBuilder();

            value = Math.Truncate(value);
            string sValue = value.ToString();
            int x = (sValue.Length / 3);
            if (sValue.Length % 3 == 0) x--;

            IEnumerable<string> sValues = sValue.SplitInParts(3);

            foreach (var item in sValues)
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    if (x == 0)
                    {
                        sb.Append(item);
                    }
                    else if ((x == 1))
                    {
                        if (item == TerbilangConstant.SATU)
                            sb.Append(TerbilangConstant.SERIBU);
                        else
                            sb.Bilangan(item, TerbilangConstant.RIBU);
                    }
                    else if (x == 2)
                        sb.Bilangan(item, TerbilangConstant.JUTA);
                    else if (x == 3)
                        sb.Bilangan(item, TerbilangConstant.MILYAR);
                    else if (x == 4)
                        sb.Bilangan(item, TerbilangConstant.TRILIUN);
                    else if (x == 5)
                        sb.Bilangan(item, TerbilangConstant.KUADRILIUN);
                    else if (x == 6)
                        sb.Bilangan(item, TerbilangConstant.KUANTILIUN);
                    else if (x == 7)
                        sb.Bilangan(item, TerbilangConstant.SEKTILIUN);
                    else if (x == 8)
                        sb.Bilangan(item, TerbilangConstant.SEPTILIUN);
                    else if (x == 9)
                        sb.Bilangan(item, TerbilangConstant.OKTILIUN);
                }
                x--;
            }
            return sb.ToString().Trim();
        }

        private static IEnumerable<string> SplitInParts(this string s, int partLength)
        {
            for (int i = 0; i < s.Length; i++)
            {
                int m = i;
                int l = 3;
                if (i == 0)
                {
                    l = s.Length % partLength;
                    if (l == 0) l = 3;
                }
                i += l - 1;
                yield return ParseRatusan(s.Substring(m, l).PadLeft(3, '0'));
            }
        }

        private static void Bilangan(this StringBuilder sb, string value, string bilangan)
        {
            sb.Append(value);
            sb.Append(bilangan);
        }

        private static string ParseSatuan(string s)
        {
            if (s == TerbilangConstant.N_SATU)
                return TerbilangConstant.SATU;
            else if (s == TerbilangConstant.N_DUA)
                return TerbilangConstant.DUA;
            else if (s == TerbilangConstant.N_TIGA)
                return TerbilangConstant.TIGA;
            else if (s == TerbilangConstant.N_EMPAT)
                return TerbilangConstant.EMPAT;
            else if (s == TerbilangConstant.N_LIMA)
                return TerbilangConstant.LIMA;
            else if (s == TerbilangConstant.N_ENAM)
                return TerbilangConstant.ENAM;
            else if (s == TerbilangConstant.N_TUJUH)
                return TerbilangConstant.TUJUH;
            else if (s == TerbilangConstant.N_DELAPAN)
                return TerbilangConstant.DELAPAN;
            else if (s == TerbilangConstant.N_SEMBILAN)
                return TerbilangConstant.SEMBILAN;
            else return string.Empty;
        }

        private static void ParsePuluhan(this StringBuilder sb, string s)
        {
            string s1 = s.Substring(0, 1);
            string s2 = s.Substring(1);
            if (s1 == TerbilangConstant.N_SATU)
            {
                if (s2 == TerbilangConstant.N_NOL) sb.Append(TerbilangConstant.SEPULUH);
                else if (s2 == TerbilangConstant.N_SATU) sb.Append(TerbilangConstant.SEBELAS);
                else sb.Bilangan(ParseSatuan(s2), TerbilangConstant.BELAS);
            }
            else
            {
                if (s1 != TerbilangConstant.N_NOL) sb.Bilangan(ParseSatuan(s1), TerbilangConstant.PULUH);
                sb.Append(ParseSatuan(s2));
            }
        }
        private static string ParseRatusan(string s)
        {
            var sb = new StringBuilder();
            string s1 = s.Substring(0, 1);
            string s2 = s.Substring(1, 2);
            if (s1 == TerbilangConstant.N_SATU) sb.Append(TerbilangConstant.SERATUS);
            else if (s1 != TerbilangConstant.N_NOL) sb.Bilangan(ParseSatuan(s1), TerbilangConstant.RATUS);
            sb.ParsePuluhan(s2);
            return sb.ToString();
        }
    }

    internal static class TerbilangConstant
    {
        internal const string N_NOL = "0";
        internal const string N_SATU = "1";
        internal const string N_DUA = "2";
        internal const string N_TIGA = "3";
        internal const string N_EMPAT = "4";
        internal const string N_LIMA = "5";
        internal const string N_ENAM = "6";
        internal const string N_TUJUH = "7";
        internal const string N_DELAPAN = "8";
        internal const string N_SEMBILAN = "9";

        internal const string SATU = " Satu";
        internal const string DUA = " Dua";
        internal const string TIGA = " Tiga";
        internal const string EMPAT = " Empat";
        internal const string LIMA = " Lima";
        internal const string ENAM = " Enam";
        internal const string TUJUH = " Tujuh";
        internal const string DELAPAN = " Delapan";
        internal const string SEMBILAN = " Sembilan";
        internal const string SEPULUH = " Sepuluh";
        internal const string SEBELAS = " Sebelas";
        internal const string PULUH = " Puluh";
        internal const string BELAS = " Belas";

        internal const string SERIBU = " Seribu";
        internal const string SERATUS = " Seratus";
        internal const string RATUS = " Ratus";
        internal const string RIBU = " Ribu";
        internal const string JUTA = " Juta";
        internal const string MILYAR = " Milyar";
        internal const string TRILIUN = " Triliun";
        internal const string KUADRILIUN = " Kuadriliun";
        internal const string KUANTILIUN = " Kuantiliun";
        internal const string SEKTILIUN = " Sekstiliun";
        internal const string SEPTILIUN = " Septiliun";
        internal const string OKTILIUN = " Oktiliun";
    }
}

