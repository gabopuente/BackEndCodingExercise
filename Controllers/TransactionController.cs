using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEndCodingExercise.Data.Models;
using BackEndCodingExercise.Service.Interfaces;
using BackEndCodingExercise.Service.Services;

namespace BackEndCodingExercise.Controllers
{
    public class TransactionController : Controller
    {
        private readonly ITransactionService _transactionService;
        private readonly IInvoiceService _invoiceService;
        public TransactionController(ITransactionService transactionService
            , IInvoiceService invoiceService)
        {
            _transactionService = transactionService;
            _invoiceService = invoiceService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult<Transaction>> CreateTransaction()
        {
            //Transaction creation
            var tran = new Transaction
            {
                TransactionDate = DateTime.Now,
                TransactionDescription = "Test transaction",
                TransactionAmmount = 50000,
                TransactionPaymentStatus = TransactionStatuses.UnBilled
            };
            return await _transactionService.AddAsync(tran);

        }
        public IActionResult GenerateInvoicesByDateRange()
        {
            List<Transaction> transList= new List<Transaction>();
            transList.Add(new Transaction
            {
                TransactionDate = DateTime.Now.AddDays(-31),
                TransactionDescription = "Test transaction -31 days",
                TransactionAmmount = 50000,
                TransactionPaymentStatus = TransactionStatuses.UnBilled
            });

            transList.Add(new Transaction
            {
                TransactionDate = DateTime.Now.AddDays(-29),
                TransactionDescription = "Test transaction -29 days",
                TransactionAmmount = 50000,
                TransactionPaymentStatus = TransactionStatuses.UnBilled
            });

            transList.Add(new Transaction
            {
                TransactionDate = DateTime.Now.AddDays(-10),
                TransactionDescription = "Test transaction -10 days",
                TransactionAmmount = 50000,
                TransactionPaymentStatus = TransactionStatuses.Billed
            });

            transList.Add(new Transaction
            {
                TransactionDate = DateTime.Now.AddDays(-2),
                TransactionDescription = "Test transaction -2 days",
                TransactionAmmount = 50000,
                TransactionPaymentStatus = TransactionStatuses.UnBilled
            });
            //4 transactions are created 
            _transactionService.AddRange(transList);

            //but only 2 will be eligible for invoicing
            List<Transaction> unbilledTransactions = _transactionService.Find(x => x.TransactionPaymentStatus == TransactionStatuses.UnBilled 
                                                                && (x.TransactionDate >= DateTime.Now.AddDays(-30) 
                                                                && x.TransactionDate <= DateTime.Now)).ToList();
            List<Invoice> invoicesList = new List<Invoice>();
            unbilledTransactions.ForEach(t =>
               {
                   _invoiceService.AddAsync(new Invoice { InvoiceDate = DateTime.Now, TransactionId = t.TransactionId });
               });


            return Ok(invoicesList) ;
            
        }
        public async Task<ActionResult<Transaction>> UpdateTransactionStatus()
        {
            //As transaction created in CreateTransaction is not
            //persistent, I'm creatint a new transaction here and then
            //updating it's status from unBilled to billed
            //for service demo purposes

            var tran = new Transaction
            {
                TransactionDate = DateTime.Now,
                TransactionDescription = "Test transaction",
                TransactionAmmount = 50000,
                TransactionPaymentStatus = TransactionStatuses.UnBilled
            };

            await _transactionService.AddAsync(tran);

            var updateTran = _transactionService.Find(x => x.TransactionId == tran.TransactionId).FirstOrDefault();

            if(updateTran!=null)
            { 
                updateTran.TransactionPaymentStatus = TransactionStatuses.Billed;

                return await _transactionService.UpdateAsync(updateTran);
            }
            else
            {
                return NotFound();
            }

        }

        public async Task<ActionResult<Transaction>> ApplyPayment()
        {
            //Transaction creation as Unbilled
            var tran = new Transaction
            {
                TransactionDate = DateTime.Now,
                TransactionDescription = "Test transaction",
                TransactionAmmount = 50000,
                TransactionPaymentStatus = TransactionStatuses.UnBilled
            };
            await _transactionService.AddAsync(tran);

            //Invoice creation
            Invoice invoice = new Invoice
            {
                InvoiceDate = DateTime.Today,
                TransactionId = tran.TransactionId
            };
            await _invoiceService.AddAsync(invoice);

            return await _invoiceService.ApplyPayment(invoice.InvoiceId, 51000);



        }
    }
}
