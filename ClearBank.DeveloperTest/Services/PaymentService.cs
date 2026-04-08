using System;
using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IAccountDataStore _accountDataStore;
        private readonly IPaymentValidatorFactory _validatorFactory;

        public PaymentService(IAccountDataStore accountDataStore, IPaymentValidatorFactory validatorFactory)
        {
            _accountDataStore = accountDataStore;
            _validatorFactory = validatorFactory;
        }

        public MakePaymentResult MakePayment(MakePaymentRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Amount <= 0)
                return new MakePaymentResult();

            var account = _accountDataStore.GetAccount(request.DebtorAccountNumber);

            var result = new MakePaymentResult();

            if (account == null)
                return result;

            var validator = _validatorFactory.GetValidator(request.PaymentScheme);
            result.Success = validator?.Validate(account, request) ?? false;

            if (result.Success)
            {
                account.Balance -= request.Amount;
                _accountDataStore.UpdateAccount(account);
            }

            return result;
        }
    }
}
