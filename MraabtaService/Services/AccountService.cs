using Dapper;
using MraabtaService.Context;
using MraabtaService.Dto_s;
using System.Data.SqlClient;
using System.Linq;
namespace MraabtaService.Services
{
    public class AccountService : IAccountService
    {
        private readonly DapperContext _context;
        public AccountService(DapperContext context)
        {
            _context=context;
        }
        public async Task<CreditClientDto> GetAccount(string UserId, string Password, string AccountId)
        {
            try
            {            
                var query = $@"SELECT
                          z.name ZoneName,                        
                          b.name BranchName,                        
                          cmb.creditClientId,                        
                          cmb.accountNo AccNo,                        
                          cmb.name AccName,                        
                          cmb.BeneficiaryName BenName,                        
                          cmb.BeneficiaryBankAccNo BenAccNo,                        
                          cmb.BenefeciaryBankName BenBank,                        
                          cmb.beneficiaryBankCode BenBankCode,                        
                         -- SUM(cmb.CNCount) CnCount,                        
                         -- SUM(cmb.DeliveredCount) DeliveredCount,                        
                          --SUM(cmb.RRCount) RRCount,                        
                          --SUM(cmb.INVCount) InvCount,                        
                         -- SUM(cmb.RRAmount) RRAmt,                        
                          SUM(cmb.AvailableAmount) AvailableAmt,                        
                         -- SUM(cmb.CODAmount) CODAmt,                        
                          SUM(cmb.INVAmount) InvoiceAmt,                        
                          --SUM(cmb.OutStandingAmount) OutstandingAMT,                        
                          (                        
                            SUM(cmb.AvailableAmount) - SUM(cmb.OutStandingAmount)                        
                          ) NetPayable                        
                        FROM                        
                          (                        
                            SELECT                        
                              cc.zoneCode,                        
                              cc.branchCode,                        
                              c.creditClientId,                        
                              cc.accountNo,                        
                              cc.name,                        
                              cc.BeneficiaryName,                        
                              cc.BeneficiaryBankAccNo,                        
                              b.Name BenefeciaryBankName,                        
                              b.SBPCode beneficiaryBankCode,                        
                              COUNT(c.consignmentNumber) CNCount,                        
                              COUNT(rc3.consignmentNumber) DeliveredCount,                        
                              COUNT(pv.ConsignmentNo) RRCount,                        
                              0 INVCount,                        
                              SUM(pv.Amount) RRAmount,                        
                              SUM(                        
                                ISNULL(pv2.Amount, 0) - ISNULL(pv2.AmountUsed, 0)                        
                              ) AvailableAmount,                        
                              SUM(cdn.codAmount) CODAmount,                        
                              0 INVAmount,                        
                              0 OutStandingAmount                        
                            FROM                        
                              Consignment c                        
                              INNER JOIN CODConsignmentDetail_New cdn ON cdn.consignmentNumber = c.consignmentNumber                        
                              INNER JOIN CreditClients cc ON cc.id = c.creditClientId                        
                              and cc.accountNo not like '%CC%'                        
                              LEFT OUTER JOIN RunsheetConsignment rc3 ON rc3.consignmentNumber = c.consignmentNumber                        
                              AND rc3.Status = '55'                        
                              LEFT OUTER JOIN PaymentVouchers pv ON pv.ConsignmentNo = c.consignmentNumber                        
                              LEFT OUTER JOIN PaymentVouchers pv2 ON pv2.ConsignmentNo = c.consignmentNumber                        
                              AND pv2.ConsignmentNo = rc3.consignmentNumber                        
                              LEFT OUTER JOIN Banks b ON b.Id = cc.BeneficiaryBankCode                        
                              AND b.isMNPBank = '0'                        
                           -- WHERE                        
                             -- C.COD = '1'                        
                             -- AND cc.CODType != '2'                        
                             -- AND cc.IsCOD = '1'                        
                             -- AND c.isapproved = '1'                        
                             -- AND (                        
                             --   C.STATUS <> '9'                        
                             --   OR C.STATUS IS NULL                        
                             -- )                        
                             -- AND c.ispayable = '0'                        
                            GROUP BY                        
                              cc.zoneCode,                        
                              cc.branchCode,                        
                              c.creditClientId,                        
                              cc.name,                        
                              cc.accountNo,                        
                              cc.name,                        
                              cc.BeneficiaryName,                        
                              cc.BeneficiaryBankAccNo,                        
                              b.Name,                        
                              b.SBPCode                        
                            UNION ALL                        
                            SELECT                        
                              INV.zoneCode,                        
                              INV.branchCode,                        
                              INV.creditClientId,                        
                              INV.accountNo,                        
                              INV.name,                        
                              INV.BeneficiaryName,                        
                              INV.BeneficiaryBankAccNo,                        
                              INV.BenefeciaryBankName,                        
                              INV.beneficiaryBankCode,                        
                              0 CNCount,                        
                              0 DeliveredCount,                        
                              0 RRcount,                        
                              COUNT(INV.invoiceNumber) INVCount,                        
                              0 RRAmount,                        
                              0 AvailableAmount,                        
                              0 CODAmount,
                              SUM(
                                ISNULL(INV.Total_Amount, 0)
                              ) INVAmount,
                              SUM(
                                ISNULL(INV.Oustanding, 0)
                              ) OutstandingAmount
                            FROM
                              (
                                SELECT
                                  cc.zoneCode,
                                  cc.branchCode,
                                  cc.id creditClientId,
                                  cc.accountNo,
                                  cc.name,
                                  cc.BeneficiaryName,
                                  cc.BeneficiaryBankAccNo,
                                  b.invoiceNumber,
                                  b3.Name BenefeciaryBankName,
                                  b3.SBPCode beneficiaryBankCode,
                                  SUM(b.Invoice_Amount) Total_Amount,
                                  SUM(b.Invoice_Amount) - (
                                    SUM(b.Recovery) + SUM(b.Adjustment)
                                  ) Oustanding
                                FROM
                                  (
                                    SELECT
                                      i.invoiceNumber,
                                      i.clientId,
                                      SUM(i.totalAmount) Invoice_Amount,
                                      0 RECOVERY,
                                      0 Adjustment
                                    FROM
                                      Invoice AS i
                                    WHERE
                                      i.IsInvoiceCanceled = '0'
                                    GROUP BY
                                      i.invoiceNumber,
                                      i.clientId
                                    UNION
                                    SELECT
                                      ir.InvoiceNo,
                                      i.clientId,
                                      0 Invoice_Amount,
                                      SUM(ir.Amount) RECOVERY,
                                      0 Adjustment
                                    FROM
                                      InvoiceRedeem AS ir
                                      INNER JOIN Invoice AS i ON i.invoiceNumber = ir.InvoiceNo
                                      INNER JOIN PaymentVouchers AS pv ON pv.Id = ir.PaymentVoucherId
                                   --  WHERE
                                   --   ISNULL(pv.PaymentSourceId, 1) IN ('1', '8', '11')
                                   --   AND i.IsInvoiceCanceled = '0'
                                    GROUP BY
                                      ir.InvoiceNo,
                                      i.clientId
                                    UNION
                                    SELECT
                                      ir.InvoiceNo,
                                      i.clientId,
                                      0 Invoice_Amount,
                                      SUM(ir.Amount) RECOVERY,
                                      0 Adjustment
                                    FROM
                                      InvoiceRedeem AS ir
                                      INNER JOIN Invoice AS i ON i.invoiceNumber = ir.InvoiceNo
                                      INNER JOIN PaymentVouchers AS pv ON pv.Id = ir.PaymentVoucherId
                                      INNER JOIN ChequeStatus AS cs ON cs.PaymentVoucherId = pv.Id
                                   --  WHERE
                                   --  pv.PaymentSourceId IN ('2', '3', '4')
                                   --  AND i.IsInvoiceCanceled = '0'
                                   --  AND cs.IsCurrentState = '1'
                                   --  AND cs.ChequeStateId IN ('1', '2')
                                    GROUP BY
                                      ir.InvoiceNo,
                                      i.clientId
                                    UNION
                                    SELECT
                                      gv.InvoiceNo,
                                      gv.CreditClientId,
                                      0 Invoice_Amount,
                                      0 RECOVERY,
                                      SUM(gv.Amount) Adjustment
                                    FROM
                                      GeneralVoucher AS gv
                                    GROUP BY
                                      gv.InvoiceNo,
                                      gv.CreditClientId
                                  ) b
                                  INNER JOIN Invoice AS i ON i.invoiceNumber = b.invoiceNumber
                                  INNER JOIN CreditClients AS cc ON cc.id = b.clientId
                                  and cc.accountNo not like '%CC%'
                                  INNER JOIN Branches AS b2 ON b2.branchCode = cc.branchCode
                                  LEFT OUTER JOIN Banks b3 ON b3.Id = cc.BeneficiaryBankCode
                                  AND b3.isMNPBank = '0'
                               --  WHERE
                               --  cc.IsCOD = '1'
                               --  AND cc.CODType != '2'
                               --  AND cc.sectorid != '0'
                               --  AND i.IsInvoiceCanceled = '0'
                               --  AND i.startDate >= '2017-06-29'
                                GROUP BY
                                  b.invoiceNumber,
                                  cc.zoneCode,
                                  cc.branchCode,
                                  cc.id,
                                  cc.accountNo,
                                  cc.name,
                                  cc.BeneficiaryName,
                                  cc.BeneficiaryBankAccNo,
                                  b.invoiceNumber,
                                  b3.Name,
                                  b3.SBPCode
                                HAVING
                                  SUM(b.Invoice_Amount) - (
                                    SUM(b.Recovery) + SUM(b.Adjustment)
                                  ) > = 1
                              ) INV
                            GROUP BY
                              INV.zoneCode,
                              INV.branchCode,
                              INV.creditClientId,
                              INV.accountNo,
                              INV.name,
                              INV.BeneficiaryName,
                              INV.BeneficiaryBankAccNo,
                              INV.BenefeciaryBankName,
                              INV.beneficiaryBankCode
                          ) CMB
                          INNER JOIN Zones z ON z.zoneCode = cmb.zoneCode
                          INNER JOIN Branches b ON b.branchCode = cmb.branchCode
                           where cmb.accountNo ='{AccountId}'
                        -- and b.branchCode ='4'
                        GROUP BY
                          z.name,
                          b.name,
                          cmb.creditClientId,
                          cmb.accountNo,
                        	  cmb.name,
                          cmb.BeneficiaryName,  cmb.BeneficiaryBankAccNo,  cmb.BenefeciaryBankName,  cmb.beneficiaryBankCode
                        ORDER BY
                          cmb.accountNo";
                using (var connection = _context.CreateConnection())
                {
                    var result= await connection.QuerySingleOrDefaultAsync<CreditClientDto>(query);                 
                     connection.Close();
                    return result;

                }
         
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                throw;
            }
     
        }
    }
}
