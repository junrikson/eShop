using eShop.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Security;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using static System.Data.Entity.Infrastructure.Design.Executor;

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

        public static decimal GetTotalCashBankTransactionDetails(ApplicationDbContext db, int cashBankTransactionId, int? cashBankTransactionDetailsId = null)
        {
            decimal total = 0;
            List<CashBankTransactionDetails> bankTransactionsDetails = null;

            if (cashBankTransactionDetailsId == null)
                bankTransactionsDetails = db.CashBankTransactionsDetails.Where(x => x.CashBankTransactionId == cashBankTransactionId).ToList();
            else
                bankTransactionsDetails = db.CashBankTransactionsDetails.Where(x => x.CashBankTransactionId == cashBankTransactionId && x.Id != cashBankTransactionDetailsId).ToList();

            if (bankTransactionsDetails != null)
                total = bankTransactionsDetails.Sum(y => y.Total);

            return total;
        }

        public static decimal GetTotalCashBankTransactionDetailsHeader(ApplicationDbContext db, int cashBankTransactionId, int? cashBankTransactionDetailsHeaderId = null)
        {
            decimal total = 0;
            List<CashBankTransactionDetailsHeader> bankTransactionsDetailsHeader = null;

            if (cashBankTransactionDetailsHeaderId == null)
                bankTransactionsDetailsHeader = db.CashBankTransactionsDetailsHeader.Where(x => x.CashBankTransactionId == cashBankTransactionId).ToList();
            else
                bankTransactionsDetailsHeader = db.CashBankTransactionsDetailsHeader.Where(x => x.CashBankTransactionId == cashBankTransactionId && x.Id != cashBankTransactionDetailsHeaderId).ToList();

            if (bankTransactionsDetailsHeader != null)
                total = bankTransactionsDetailsHeader.Sum(y => y.Total);

            return total;
        }
        public static Journal CreateCashBankJournal(CashBankTransaction cashBankTransaction, ApplicationDbContext db)
        {
            Journal journal = new Journal
            {
                Code = cashBankTransaction.Code,
                Date = cashBankTransaction.Date,
                MasterBusinessUnitId = cashBankTransaction.MasterBusinessUnitId,
                Type = EnumJournalType.CashBankTransaction,
                Debit = 0,
                Credit = 0,
                CashBankTransactionId = cashBankTransaction.Id,
                Active = cashBankTransaction.Active,
                Created = cashBankTransaction.Created,
                Updated = cashBankTransaction.Updated,
                UserId = cashBankTransaction.UserId
            };

            db.Journals.Add(journal);
            db.SaveChanges();

            return journal;
        }

        public static Journal UpdateCashBankJournal(Journal journal, CashBankTransaction cashBankTransaction, EnumCashBankTransactionType enumCashBankTransactionType, ApplicationDbContext db)
        {
            db.Entry(journal).State = EntityState.Modified;

            journal.Code = cashBankTransaction.Code;
            journal.Date = cashBankTransaction.Date;
            journal.MasterBusinessUnitId = cashBankTransaction.MasterBusinessUnitId;
            journal.Type = EnumJournalType.CashBankTransaction;
            journal.Debit = 0;
            journal.Credit = 0;
            journal.CashBankTransactionId = cashBankTransaction.Id;
            journal.Active = cashBankTransaction.Active;
            journal.Updated = cashBankTransaction.Updated;
            journal.UserId = cashBankTransaction.UserId;
            db.SaveChanges();

            List<CashBankTransactionDetails> cashBankTransactionsDetails = db.CashBankTransactionsDetails.Where(x => x.CashBankTransactionId == cashBankTransaction.Id).ToList();
            if (cashBankTransactionsDetails != null && cashBankTransactionsDetails.Count > 0)
            {
                foreach (CashBankTransactionDetails cashBankTransactionDetails in cashBankTransactionsDetails)
                {
                    List<JournalDetails> journalsDetails = db.JournalsDetails.Where(x => x.Journal.Type == EnumJournalType.CashBankTransaction && x.CashBankTransactionDetailsId == cashBankTransactionDetails.Id).ToList();

                    if (journalsDetails == null || journalsDetails.Count <= 0)
                        CreateCashBankJournalDetails(cashBankTransactionDetails, journal, enumCashBankTransactionType, db);
                    else
                        UpdateCashBankJournalDetails(cashBankTransactionDetails, enumCashBankTransactionType, db);
                }
            }

            List<CashBankTransactionDetailsHeader> cashBankTransactionsDetailsHeader = db.CashBankTransactionsDetailsHeader.Where(x => x.CashBankTransactionId == cashBankTransaction.Id).ToList();
            if (cashBankTransactionsDetailsHeader != null && cashBankTransactionsDetailsHeader.Count > 0)
            {
                foreach (CashBankTransactionDetailsHeader cashBankTransactionDetailsHeader in cashBankTransactionsDetailsHeader)
                {
                    List<JournalDetails> journalsDetails = db.JournalsDetails.Where(x => x.Journal.Type == EnumJournalType.CashBankTransaction && x.CashBankTransactionDetailsHeaderId == cashBankTransactionDetailsHeader.Id).ToList();

                    if (journalsDetails == null || journalsDetails.Count <= 0)
                        CreateCashBankJournalDetailsHeader(cashBankTransactionDetailsHeader, journal, enumCashBankTransactionType, db);
                    else
                        UpdateCashBankJournalDetailsHeader(cashBankTransactionDetailsHeader, enumCashBankTransactionType, db);
                }
            }
            return journal;
        }

        public static void DeleteCashBankJournals(int id, Journal journal, CashBankTransaction bankTransaction, ApplicationDbContext db)
        {
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

            db.CashBankTransactionsDetails.RemoveRange(db.CashBankTransactionsDetails.Where(x => x.CashBankTransactionId == id));
            db.SaveChanges();

            db.CashBankTransactionsDetailsHeader.RemoveRange(db.CashBankTransactionsDetailsHeader.Where(x => x.CashBankTransactionId == id));
            db.SaveChanges();

            db.CashBankTransactions.Remove(bankTransaction);
            db.SaveChanges();
        }

        public static void CreateCashBankJournalDetails(CashBankTransactionDetails cashBankTransactionDetails, Journal journal, EnumCashBankTransactionType enumBankTransactionType, ApplicationDbContext db)
        {
            CashBankTransaction cashBankTransaction = db.CashBankTransactions.Find(cashBankTransactionDetails.CashBankTransactionId);
            JournalDetails journalDetails = new JournalDetails
            {
                JournalId = journal.Id,
                MasterRegionId = cashBankTransaction.MasterRegionId,
                ChartOfAccountId = cashBankTransactionDetails.ChartOfAccountId,
                Notes = cashBankTransactionDetails.Notes,
                CashBankTransactionDetailsId = cashBankTransactionDetails.Id,
                Created = cashBankTransactionDetails.Created,
                Updated = cashBankTransactionDetails.Updated,
                UserId = cashBankTransactionDetails.UserId
            };

            if (enumBankTransactionType == EnumCashBankTransactionType.In)
            {
                if (cashBankTransactionDetails.Total < 0)
                {
                    journalDetails.Credit = 0;
                    journalDetails.Debit = cashBankTransactionDetails.Total * -1;
                }
                else
                {
                    journalDetails.Debit = 0;
                    journalDetails.Credit = cashBankTransactionDetails.Total;
                }
            }
            else
            {
                if (cashBankTransactionDetails.Total < 0)
                {
                    journalDetails.Debit = 0;
                    journalDetails.Credit = cashBankTransactionDetails.Total * -1;
                }
                else
                {
                    journalDetails.Credit = 0;
                    journalDetails.Debit = cashBankTransactionDetails.Total;
                }
            }

            db.JournalsDetails.Add(journalDetails);
            db.SaveChanges();

            journal.Debit = GetTotalJournalDebit(db, journal.Id, journalDetails.Id) + journalDetails.Debit;
            journal.Credit = GetTotalJournalCredit(db, journal.Id, journalDetails.Id) + journalDetails.Credit;
            db.Entry(journal).State = EntityState.Modified;
            db.SaveChanges();
        }

        public static void UpdateCashBankJournalDetails(CashBankTransactionDetails cashBankTransactionDetails, EnumCashBankTransactionType enumCashBankTransactionType, ApplicationDbContext db)
        {
            CashBankTransaction cashBankTransaction = db.CashBankTransactions.Find(cashBankTransactionDetails.CashBankTransactionId);
            JournalDetails journalDetails = db.JournalsDetails.Where(x => x.Journal.Type == EnumJournalType.CashBankTransaction && x.CashBankTransactionDetailsId == cashBankTransactionDetails.Id).FirstOrDefault();

            if (journalDetails != null)
            {
                Journal journal = db.Journals.Find(journalDetails.JournalId);

                db.Entry(journalDetails).State = EntityState.Modified;

                journalDetails.MasterRegionId = cashBankTransaction.MasterRegionId;
                journalDetails.ChartOfAccountId = cashBankTransactionDetails.ChartOfAccountId;
                journalDetails.Notes = cashBankTransactionDetails.Notes;
                journalDetails.Updated = cashBankTransactionDetails.Updated;
                journalDetails.UserId = cashBankTransactionDetails.UserId;

                if (enumCashBankTransactionType == EnumCashBankTransactionType.In)
                {
                    if (cashBankTransactionDetails.Total < 0)
                    {
                        journalDetails.Credit = 0;
                        journalDetails.Debit = cashBankTransactionDetails.Total * -1;
                    }
                    else
                    {
                        journalDetails.Debit = 0;
                        journalDetails.Credit = cashBankTransactionDetails.Total;
                    }
                }
                else
                {
                    if (cashBankTransactionDetails.Total < 0)
                    {
                        journalDetails.Debit = 0;
                        journalDetails.Credit = cashBankTransactionDetails.Total * -1;
                    }
                    else
                    {
                        journalDetails.Credit = 0;
                        journalDetails.Debit = cashBankTransactionDetails.Total;
                    }
                }

                db.SaveChanges();

                journal.Debit = GetTotalJournalDebit(db, journal.Id, journalDetails.Id) + journalDetails.Debit;
                journal.Credit = GetTotalJournalCredit(db, journal.Id, journalDetails.Id) + journalDetails.Credit;
                db.Entry(journal).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public static void RemoveCashBankJournalDetails(CashBankTransactionDetails cashBankTransactionDetails, Journal journal, ApplicationDbContext db)
        {
            List<JournalDetails> journalsDetails = db.JournalsDetails.Where(x => x.JournalId == journal.Id && x.CashBankTransactionDetailsId == cashBankTransactionDetails.Id).ToList();

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

        public static void CreateCashBankJournalDetailsHeader(CashBankTransactionDetailsHeader cashBankTransactionDetailsHeader, Journal journal, EnumCashBankTransactionType enumCashBankTransactionType, ApplicationDbContext db)
        {
            CashBankTransaction cashBankTransaction = db.CashBankTransactions.Find(cashBankTransactionDetailsHeader.CashBankTransactionId);
            MasterCashBank masterCashBank = db.MasterCashBanks.Find(cashBankTransactionDetailsHeader.MasterCashBankId);

            JournalDetails journalDetails = new JournalDetails
            {
                JournalId = journal.Id,
                MasterRegionId = cashBankTransaction.MasterRegionId,
                ChartOfAccountId = masterCashBank.ChartOfAccountId,
                CashBankTransactionDetailsHeaderId = cashBankTransactionDetailsHeader.Id,
                Created = cashBankTransactionDetailsHeader.Created,
                Updated = cashBankTransactionDetailsHeader.Updated,
                UserId = cashBankTransactionDetailsHeader.UserId,
                Notes = cashBankTransactionDetailsHeader.Notes
            };

            if (enumCashBankTransactionType == EnumCashBankTransactionType.In)
            {
                if (cashBankTransactionDetailsHeader.Total < 0)
                {
                    journalDetails.Debit = 0;
                    journalDetails.Credit = cashBankTransactionDetailsHeader.Total * -1;
                }
                else
                {
                    journalDetails.Credit = 0;
                    journalDetails.Debit = cashBankTransactionDetailsHeader.Total;
                }
            }
            else
            {
                if (cashBankTransactionDetailsHeader.Total < 0)
                {
                    journalDetails.Credit = 0;
                    journalDetails.Debit = cashBankTransactionDetailsHeader.Total * -1;
                }
                else
                {
                    journalDetails.Debit = 0;
                    journalDetails.Credit = cashBankTransactionDetailsHeader.Total;
                }
            }

            db.JournalsDetails.Add(journalDetails);
            db.SaveChanges();

            journal.Debit = GetTotalJournalDebit(db, journal.Id, journalDetails.Id) + journalDetails.Debit;
            journal.Credit = GetTotalJournalCredit(db, journal.Id, journalDetails.Id) + journalDetails.Credit;
            db.Entry(journal).State = EntityState.Modified;
            db.SaveChanges();
        }

        public static void UpdateCashBankJournalDetailsHeader(CashBankTransactionDetailsHeader cashBankTransactionDetailsHeader, EnumCashBankTransactionType enumBankTransactionType, ApplicationDbContext db)
        {
            CashBankTransaction cashBankTransaction = db.CashBankTransactions.Find(cashBankTransactionDetailsHeader.CashBankTransactionId);
            JournalDetails journalDetails = db.JournalsDetails.Where(x => x.Journal.Type == EnumJournalType.CashBankTransaction && x.CashBankTransactionDetailsHeaderId == cashBankTransactionDetailsHeader.Id).FirstOrDefault();
            MasterCashBank masterBank = db.MasterCashBanks.Find(cashBankTransactionDetailsHeader.MasterCashBankId);


            if (journalDetails != null)
            {
                Journal journal = db.Journals.Find(journalDetails.JournalId);

                db.Entry(journalDetails).State = EntityState.Modified;

                journalDetails.MasterRegionId = cashBankTransaction.MasterRegionId;
                journalDetails.ChartOfAccountId = masterBank.ChartOfAccountId;
                journalDetails.Updated = cashBankTransactionDetailsHeader.Updated;
                journalDetails.UserId = cashBankTransactionDetailsHeader.UserId;
                journalDetails.Notes = cashBankTransactionDetailsHeader.Notes;

                if (enumBankTransactionType == EnumCashBankTransactionType.In)
                {
                    if (cashBankTransactionDetailsHeader.Total < 0)
                    {
                        journalDetails.Debit = 0;
                        journalDetails.Credit = cashBankTransactionDetailsHeader.Total * -1;
                    }
                    else
                    {
                        journalDetails.Credit = 0;
                        journalDetails.Debit = cashBankTransactionDetailsHeader.Total;
                    }
                }
                else
                {
                    if (cashBankTransactionDetailsHeader.Total < 0)
                    {
                        journalDetails.Credit = 0;
                        journalDetails.Debit = cashBankTransactionDetailsHeader.Total * -1;
                    }
                    else
                    {
                        journalDetails.Debit = 0;
                        journalDetails.Credit = cashBankTransactionDetailsHeader.Total;
                    }
                }

                db.SaveChanges();

                journal.Debit = GetTotalJournalDebit(db, journal.Id, journalDetails.Id) + journalDetails.Debit;
                journal.Credit = GetTotalJournalCredit(db, journal.Id, journalDetails.Id) + journalDetails.Credit;
                db.Entry(journal).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public static void RemoveCashBankJournalDetailsHeader(CashBankTransactionDetailsHeader cashBankTransactionDetailsHeader, Journal journal, ApplicationDbContext db)
        {
            List<JournalDetails> journalsDetails = db.JournalsDetails.Where(x => x.JournalId == journal.Id && x.CashBankTransactionDetailsHeaderId == cashBankTransactionDetailsHeader.Id).ToList();

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

// NOTA KREDIT

        public static decimal GetTotalCreditNote(ApplicationDbContext db, int creditNoteId, int? creditNoteDetailsId = null)
        {
            decimal total = 0;
            List<CreditNoteDetails> totalCreditNoteDetails = null;

            if (creditNoteDetailsId == null)
                totalCreditNoteDetails = db.CreditNotesDetails.Where(x => x.CreditNoteId == creditNoteId).ToList();
            else
                totalCreditNoteDetails = db.CreditNotesDetails.Where(x => x.CreditNoteId == creditNoteId && x.Id != creditNoteDetailsId).ToList();

            if (totalCreditNoteDetails != null)
                total = totalCreditNoteDetails.Sum(y => y.Total);

            return total;
        }

        public static decimal GetTotalCreditNoteAllocated(ApplicationDbContext db, CreditNote creditNote, RepaymentDetailsHeader repaymentDetailsHeader = null)
        {
            decimal total = 0;

            var repaymentsDetailsHeader = db.RepaymentsDetailsHeader.Where(x => x.Type == EnumRepaymentType.CreditNote && x.CreditNoteId == creditNote.Id && x.Id != repaymentDetailsHeader.Id).ToList();

            if (repaymentsDetailsHeader != null || repaymentsDetailsHeader.Count > 0)
                total = repaymentsDetailsHeader.Sum(y => y.Total);

            var repaymentDetails = db.RepaymentsDetails.Where(x => x.Type == EnumRepaymentType.CreditNote && x.CreditNoteId == creditNote.Id).ToList();

            if (repaymentDetails != null || repaymentDetails.Count > 0)
                total += repaymentDetails.Sum(y => y.Total);

            return total;
        }

        public static Journal CreateCreditNoteJournal(CreditNote creditNote, ApplicationDbContext db)
        {
            Journal journal = new Journal
            {
                Code = creditNote.Code,
                Date = creditNote.Date,
                MasterBusinessUnitId = creditNote.MasterBusinessUnitId,
                Type = EnumJournalType.CreditNote,
                Debit = 0,
                Credit = 0,
                CreditNoteId = creditNote.Id,
                Active = creditNote.Active,
                Created = creditNote.Created,
                Updated = creditNote.Updated,
                UserId = creditNote.UserId
            };

            db.Journals.Add(journal);
            db.SaveChanges();

            return journal;
        }

        public static Journal UpdateCreditNoteJournal(Journal journal, CreditNote creditNote, ApplicationDbContext db)
        {
            db.Entry(journal).State = EntityState.Modified;

            journal.Code = creditNote.Code;
            journal.Date = creditNote.Date;
            journal.MasterBusinessUnitId = creditNote.MasterBusinessUnitId;
            journal.Type = EnumJournalType.CreditNote;
            journal.Debit = 0;
            journal.Credit = 0;
            journal.CreditNoteId = creditNote.Id;
            journal.Notes = creditNote.Notes;
            journal.Active = creditNote.Active;
            journal.Updated = creditNote.Updated;
            journal.UserId = creditNote.UserId;
            db.SaveChanges();

            var creditNotesDetails = db.CreditNotesDetails.Where(x => x.CreditNoteId == creditNote.Id).ToList();
            if (creditNotesDetails != null && creditNotesDetails.Count > 0)
            {
                foreach (CreditNoteDetails creditNoteDetails in creditNotesDetails)
                {
                    var journalsDetails = db.JournalsDetails.Where(x => x.Journal.Type == EnumJournalType.CreditNote && x.CreditNoteDetailsId == creditNoteDetails.Id).ToList();

                    if (journalsDetails == null || journalsDetails.Count <= 0)
                        CreateCreditNoteJournalDetails(creditNoteDetails, journal, db);
                    else
                        UpdateCreditNoteJournalDetails(creditNoteDetails, db);
                }
            }
            return journal;
        }

        public static void DeleteCreditNoteJournals(int id, Journal journal, ApplicationDbContext db)
        {
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

        public static void CreateCreditNoteJournalDetails(CreditNoteDetails creditNoteDetails, Journal journal, ApplicationDbContext db)
        {
            CreditNote creditNote = db.CreditNotes.Find(creditNoteDetails.CreditNoteId);
            JournalDetails journalDetails = new JournalDetails
            {
                JournalId = journal.Id,
                MasterRegionId = creditNote.MasterRegionId,
                Notes = creditNoteDetails.Notes,
                CreditNoteDetailsId = creditNoteDetails.Id,
                Created = creditNoteDetails.Created,
                Updated = creditNoteDetails.Updated,
                UserId = creditNoteDetails.UserId
            };

            if (creditNoteDetails.Type == EnumCreditNoteType.Bank || creditNoteDetails.Type == EnumCreditNoteType.Cash)
            {
                MasterCashBank masterBank = db.MasterCashBanks.Find(creditNoteDetails.MasterCashBankId);
                journalDetails.ChartOfAccountId = masterBank.ChartOfAccountId;
            }
            else if (creditNoteDetails.Type == EnumCreditNoteType.Cheque)
            {
                journalDetails.ChartOfAccountId = db.MasterBusinessUnitsAccounts.Where(x => x.MasterBusinessUnitId == creditNote.MasterBusinessUnitId && x.MasterRegionId == creditNote.MasterRegionId && x.Type == EnumBusinessUnitAccountType.ChequeReceivablesAccount).FirstOrDefault().ChartOfAccount.Id;
            }
            else if (creditNoteDetails.Type == EnumCreditNoteType.MasterCost)
            {
                MasterCost masterCost = db.MasterCosts.Find(creditNoteDetails.MasterCostId);

                journalDetails.ChartOfAccountId = masterCost.ChartOfAccount.Id;
            }

            if (creditNoteDetails.Total >= 0)
            {
                journalDetails.Debit = creditNoteDetails.Total;
                journalDetails.Credit = 0;
            }
            else
            {
                journalDetails.Debit = 0;
                journalDetails.Credit = creditNoteDetails.Total * -1;
            }

            db.JournalsDetails.Add(journalDetails);
            db.SaveChanges();

            journal.Debit = GetTotalJournalDebit(db, journal.Id, journalDetails.Id) + journalDetails.Debit;
            journal.Credit = GetTotalJournalCredit(db, journal.Id, journalDetails.Id) + journalDetails.Credit;
            db.Entry(journal).State = EntityState.Modified;
            db.SaveChanges();

            JournalDetails JournalDetailsCredit = db.JournalsDetails.Where(x => x.JournalId == journal.Id && x.isMerged).FirstOrDefault();
            if (JournalDetailsCredit != null)
            {
                JournalDetailsCredit.Debit = 0;
                JournalDetailsCredit.Credit = GetTotalCreditNote(db, creditNote.Id, creditNoteDetails.Id) + creditNoteDetails.Total;
                JournalDetailsCredit.MasterRegionId = creditNote.MasterRegionId;
                JournalDetailsCredit.ChartOfAccountId = db.MasterBusinessUnitsAccounts.Where(x => x.MasterBusinessUnitId == creditNote.MasterBusinessUnitId && x.MasterRegionId == creditNote.MasterRegionId && x.Type == EnumBusinessUnitAccountType.CreditNoteAccount).FirstOrDefault().ChartOfAccount.Id;
                JournalDetailsCredit.Notes = "NOTA KREDIT " + creditNote.Code;
                JournalDetailsCredit.isMerged = true;
                JournalDetailsCredit.Created = creditNoteDetails.Created;
                JournalDetailsCredit.Updated = creditNoteDetails.Updated;
                JournalDetailsCredit.UserId = creditNoteDetails.UserId;
                db.Entry(JournalDetailsCredit).State = EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                JournalDetailsCredit = new JournalDetails
                {
                    JournalId = journal.Id,
                    Debit = 0,
                    Credit = GetTotalCreditNote(db, creditNote.Id, creditNoteDetails.Id) + creditNoteDetails.Total,
                    ChartOfAccountId = db.MasterBusinessUnitsAccounts.Where(x => x.MasterBusinessUnitId == creditNote.MasterBusinessUnitId && x.MasterRegionId == creditNote.MasterRegionId && x.Type == EnumBusinessUnitAccountType.CreditNoteAccount).FirstOrDefault().ChartOfAccount.Id,
                    MasterRegionId = creditNote.MasterRegionId,
                    Notes = "NOTA KREDIT " + creditNote.Code,
                    isMerged = true,
                    Created = creditNoteDetails.Created,
                    Updated = creditNoteDetails.Updated,
                    UserId = creditNoteDetails.UserId
                };

                db.JournalsDetails.Add(JournalDetailsCredit);
                db.SaveChanges();
            }

            journal.Credit = GetTotalJournalCredit(db, journal.Id, JournalDetailsCredit.Id) + JournalDetailsCredit.Credit;
            db.Entry(journal).State = EntityState.Modified;
            db.SaveChanges();
        }

        public static void UpdateCreditNoteJournalDetails(CreditNoteDetails creditNoteDetails, ApplicationDbContext db)
        {
            CreditNote creditNote = db.CreditNotes.Find(creditNoteDetails.CreditNoteId);
            var journalDetails = db.JournalsDetails.Where(x => x.Journal.Type == EnumJournalType.CreditNote && x.CreditNoteDetailsId == creditNoteDetails.Id).FirstOrDefault();

            if (journalDetails != null)
            {
                Journal journal = db.Journals.Find(journalDetails.JournalId);

                db.Entry(journalDetails).State = EntityState.Modified;
                journalDetails.MasterRegionId = creditNote.MasterRegionId;
                journalDetails.Notes = creditNoteDetails.Notes;
                journalDetails.Updated = creditNoteDetails.Updated;
                journalDetails.UserId = creditNoteDetails.UserId;

                if (creditNoteDetails.Type == EnumCreditNoteType.Bank || creditNoteDetails.Type == EnumCreditNoteType.Cash)
                {
                    MasterCashBank masterCashBank = db.MasterCashBanks.Find(creditNoteDetails.MasterCashBankId);
                    journalDetails.ChartOfAccountId = masterCashBank.ChartOfAccountId;
                }
                else if (creditNoteDetails.Type == EnumCreditNoteType.Cheque)
                {
                    journalDetails.ChartOfAccountId = db.MasterBusinessUnitsAccounts.Where(x => x.MasterBusinessUnitId == creditNote.MasterBusinessUnitId && x.MasterRegionId == creditNote.MasterRegionId && x.Type == EnumBusinessUnitAccountType.ChequeReceivablesAccount).FirstOrDefault().ChartOfAccount.Id;
                }
                else if (creditNoteDetails.Type == EnumCreditNoteType.MasterCost)
                {
                    MasterCost masterCost = db.MasterCosts.Find(creditNoteDetails.MasterCostId);

                    journalDetails.ChartOfAccountId = masterCost.ChartOfAccount.Id;
                }

                if (creditNoteDetails.Total >= 0)
                {
                    journalDetails.Debit = creditNoteDetails.Total;
                    journalDetails.Credit = 0;
                }
                else
                {
                    journalDetails.Debit = 0;
                    journalDetails.Credit = creditNoteDetails.Total * -1;
                }
                db.SaveChanges();

                journal.Debit = GetTotalJournalDebit(db, journal.Id, journalDetails.Id) + journalDetails.Debit;
                journal.Credit = GetTotalJournalCredit(db, journal.Id, journalDetails.Id) + journalDetails.Credit;
                db.Entry(journal).State = EntityState.Modified;
                db.SaveChanges();

                JournalDetails JournalDetailsCredit = db.JournalsDetails.Where(x => x.JournalId == journal.Id && x.isMerged).FirstOrDefault();
                if (JournalDetailsCredit != null)
                {
                    JournalDetailsCredit.Debit = 0;
                    JournalDetailsCredit.Credit = GetTotalCreditNote(db, creditNote.Id, creditNoteDetails.Id) + creditNoteDetails.Total;
                    JournalDetailsCredit.ChartOfAccountId = db.MasterBusinessUnitsAccounts.Where(x => x.MasterBusinessUnitId == creditNote.MasterBusinessUnitId && x.MasterRegionId == creditNote.MasterRegionId && x.Type == EnumBusinessUnitAccountType.CreditNoteAccount).FirstOrDefault().ChartOfAccount.Id;
                    JournalDetailsCredit.MasterRegionId = creditNote.MasterRegionId;
                    JournalDetailsCredit.Notes = "NOTA KREDIT " + creditNote.Code;
                    JournalDetailsCredit.isMerged = true;
                    JournalDetailsCredit.Created = creditNoteDetails.Created;
                    JournalDetailsCredit.Updated = creditNoteDetails.Updated;
                    JournalDetailsCredit.UserId = creditNoteDetails.UserId;
                    db.Entry(JournalDetailsCredit).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    JournalDetailsCredit = new JournalDetails
                    {
                        JournalId = journal.Id,
                        Debit = 0,
                        Credit = GetTotalCreditNote(db, creditNote.Id, creditNoteDetails.Id) + creditNoteDetails.Total,
                        ChartOfAccountId = db.MasterBusinessUnitsAccounts.Where(x => x.MasterBusinessUnitId == creditNote.MasterBusinessUnitId && x.MasterRegionId == creditNote.MasterRegionId && x.Type == EnumBusinessUnitAccountType.CreditNoteAccount).FirstOrDefault().ChartOfAccount.Id,
                        MasterRegionId = creditNote.MasterRegionId,
                        Notes = "NOTA KREDIT " + creditNote.Code,
                        isMerged = true,
                        Created = creditNoteDetails.Created,
                        Updated = creditNoteDetails.Updated,
                        UserId = creditNoteDetails.UserId
                    };

                    db.JournalsDetails.Add(JournalDetailsCredit);
                    db.SaveChanges();
                }

                journal.Credit = GetTotalJournalCredit(db, journal.Id, JournalDetailsCredit.Id) + JournalDetailsCredit.Credit;
                db.Entry(journal).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public static void RemoveCreditNoteJournalDetails(CreditNoteDetails creditNoteDetails, Journal journal, ApplicationDbContext db)
        {
            var journalDetails = db.JournalsDetails.Where(x => x.JournalId == journal.Id && x.CreditNoteDetailsId == creditNoteDetails.Id).FirstOrDefault();

            if (journalDetails.Credit == 0)
                journal.Debit = GetTotalJournalDebit(db, journal.Id, journalDetails.Id);
            else
                journal.Credit = GetTotalJournalCredit(db, journal.Id, journalDetails.Id);

            db.Entry(journal).State = EntityState.Modified;
            db.SaveChanges();

            db.JournalsDetails.Remove(journalDetails);
            db.SaveChanges();

            JournalDetails JournalDetailsCredit = db.JournalsDetails.Where(x => x.JournalId == journal.Id && x.isMerged).FirstOrDefault();
            if (JournalDetailsCredit != null)
            {
                JournalDetailsCredit.Credit = GetTotalCreditNote(db, creditNoteDetails.CreditNoteId, creditNoteDetails.Id);

                db.Entry(JournalDetailsCredit).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

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

