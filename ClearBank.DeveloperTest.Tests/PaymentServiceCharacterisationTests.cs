using ClearBank.DeveloperTest.Services;
using ClearBank.DeveloperTest.Types;
using Shouldly;
using Xunit;

namespace ClearBank.DeveloperTest.Tests
{
    public class PaymentServiceCharacterisationTests
    {
        private readonly IPaymentService _paymentService = new PaymentService();

        [Fact]
        public void MakePayment_BacsFlagNotSet_ReturnsFailure()
        {
            var request = new MakePaymentRequest
            {
                DebtorAccountNumber = "12345678",
                PaymentScheme = PaymentScheme.Bacs,
                Amount = 100m
            };

            var result = _paymentService.MakePayment(request);

            result.Success.ShouldBeFalse();
        }

        [Fact]
        public void MakePayment_FasterPaymentsFlagNotSet_ReturnsFailure()
        {
            var request = new MakePaymentRequest
            {
                DebtorAccountNumber = "12345678",
                PaymentScheme = PaymentScheme.FasterPayments,
                Amount = 100m
            };

            var result = _paymentService.MakePayment(request);

            result.Success.ShouldBeFalse();
        }

        [Fact]
        public void MakePayment_ChapsFlagNotSet_ReturnsFailure()
        {
            var request = new MakePaymentRequest
            {
                DebtorAccountNumber = "12345678",
                PaymentScheme = PaymentScheme.Chaps,
                Amount = 100m
            };

            var result = _paymentService.MakePayment(request);

            result.Success.ShouldBeFalse();
        }
    }
}