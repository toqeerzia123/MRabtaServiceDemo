namespace MraabtaService.Dto_s
{
    public class CreditClientDto
    {
        public string ZoneName { get; set; }
        public string BranchName { get; set; }
        public int creditClientId { get; set; }
        public string AccNo { get; set; }
        public string AccName { get; set; }
        public string BenName { get; set; }
        public string BenAccNo { get; set; }
        public string BenBank { get; set; }
        public string BenBankCode { get; set; }
        public int AvailableAmt { get; set; }
        public int InvoiceAmt { get; set; }
        public int NetPayable { get; set; }

    }
}
