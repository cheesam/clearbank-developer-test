using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Services;
using ClearBank.DeveloperTest.Types;
using Moq;

namespace ClearBank.DeveloperTest.Tests
{
    public abstract class PaymentServiceScenario
    {
        protected readonly Mock<IAccountDataStore> AccountDataStore = new();

        protected readonly Account Account = new()
        {
            Balance = 1000m,
            Status = AccountStatus.Live,
            AllowedPaymentSchemes = AllowedPaymentSchemes.Bacs
                | AllowedPaymentSchemes.FasterPayments
                | AllowedPaymentSchemes.Chaps
        };

        protected MakePaymentRequest Request = new()
        {
            DebtorAccountNumber = "12345678",
            Amount = 100m
        };

        protected PaymentServiceScenario()
        {
            AccountDataStore
                .Setup(x => x.GetAccount(It.IsAny<string>()))
                .Returns(Account);
        }

        protected IPaymentService CreateService() =>
            new PaymentService(AccountDataStore.Object, new PaymentValidatorFactory());

        protected MakePaymentResult MakePayment() =>
            CreateService().MakePayment(Request);
    }
}
