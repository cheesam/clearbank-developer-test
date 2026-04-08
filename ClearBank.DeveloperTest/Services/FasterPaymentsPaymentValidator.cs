using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Services
{
    public class FasterPaymentsPaymentValidator : IPaymentValidator
    {
        public bool Validate(Account account, MakePaymentRequest request)
        {
            return account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.FasterPayments)
                && account.Balance >= request.Amount;
        }
    }
}
