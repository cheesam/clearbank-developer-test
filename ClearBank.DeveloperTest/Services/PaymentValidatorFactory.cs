using System.Collections.Generic;
using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Services
{
    public class PaymentValidatorFactory : IPaymentValidatorFactory
    {
        private readonly Dictionary<PaymentScheme, IPaymentValidator> _validators = new()
        {
            { PaymentScheme.Bacs, new BacsPaymentValidator() },
            { PaymentScheme.FasterPayments, new FasterPaymentsPaymentValidator() },
            { PaymentScheme.Chaps, new ChapsPaymentValidator() }
        };

        public IPaymentValidator GetValidator(PaymentScheme paymentScheme)
        {
            _validators.TryGetValue(paymentScheme, out var validator);
            return validator;
        }
    }
}
