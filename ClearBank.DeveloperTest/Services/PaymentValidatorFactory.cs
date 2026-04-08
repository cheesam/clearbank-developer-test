using System.Collections.Generic;
using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Services
{
    public class PaymentValidatorFactory : IPaymentValidatorFactory
    {
        private readonly IDictionary<PaymentScheme, IPaymentValidator> _validators;

        public PaymentValidatorFactory(IDictionary<PaymentScheme, IPaymentValidator> validators)
        {
            _validators = validators;
        }

        public IPaymentValidator GetValidator(PaymentScheme paymentScheme)
        {
            _validators.TryGetValue(paymentScheme, out var validator);
            return validator;
        }
    }
}