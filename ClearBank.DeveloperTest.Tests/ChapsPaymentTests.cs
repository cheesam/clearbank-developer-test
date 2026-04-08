using ClearBank.DeveloperTest.Types;
using Moq;
using Shouldly;
using Xunit;

namespace ClearBank.DeveloperTest.Tests
{
    public class ChapsPaymentTests : PaymentServiceScenario
    {
        public ChapsPaymentTests()
        {
            Request.PaymentScheme = PaymentScheme.Chaps;
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
        public void MakePayment_ChapsFlagNotSet_ReturnsFailure()
        {
            Account.AllowedPaymentSchemes = AllowedPaymentSchemes.Bacs | AllowedPaymentSchemes.FasterPayments;

            MakePayment().Success.ShouldBeFalse();

            AccountDataStore.Verify(x => x.UpdateAccount(It.IsAny<Account>()), Times.Never);
        }

        [Fact]
        public void MakePayment_AccountNotLive_ReturnsFailure()
        {
            Account.Status = AccountStatus.Disabled;

            MakePayment().Success.ShouldBeFalse();

            AccountDataStore.Verify(x => x.UpdateAccount(It.IsAny<Account>()), Times.Never);
        }
    }
}
