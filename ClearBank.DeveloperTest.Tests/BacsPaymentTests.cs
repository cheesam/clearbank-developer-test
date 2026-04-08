using ClearBank.DeveloperTest.Types;
using Moq;
using Shouldly;
using Xunit;

namespace ClearBank.DeveloperTest.Tests
{
    public class BacsPaymentTests : PaymentServiceScenario
    {
        public BacsPaymentTests()
        {
            Request.PaymentScheme = PaymentScheme.Bacs;
        }

        [Fact]
        public void MakePayment_ValidAccount_ReturnsSuccess()
        {
            MakePayment().Success.ShouldBeTrue();
        }

        [Fact]
        public void MakePayment_ValidAccount_DecrementsBalance()
        {
            var expectedBalance = Account.Balance - Request.Amount;

            MakePayment();

            Account.Balance.ShouldBe(expectedBalance);
        }

        [Fact]
        public void MakePayment_NullAccount_ReturnsFailure()
        {
            AccountDataStore.Setup(x => x.GetAccount(It.IsAny<string>())).Returns((Account)null);

            MakePayment().Success.ShouldBeFalse();

            AccountDataStore.Verify(x => x.UpdateAccount(It.IsAny<Account>()), Times.Never);
        }

        [Fact]
        public void MakePayment_BacsFlagNotSet_ReturnsFailure()
        {
            Account.AllowedPaymentSchemes = AllowedPaymentSchemes.FasterPayments | AllowedPaymentSchemes.Chaps;

            MakePayment().Success.ShouldBeFalse();

            AccountDataStore.Verify(x => x.UpdateAccount(It.IsAny<Account>()), Times.Never);
        }
    }
}
