using System.Collections.Generic;
using ClearBank.DeveloperTest.Services;
using ClearBank.DeveloperTest.Types;
using Shouldly;
using Xunit;

namespace ClearBank.DeveloperTest.Tests
{
    public class PaymentValidatorFactoryTests
    {
        private readonly IPaymentValidatorFactory _factory = new PaymentValidatorFactory(
            new Dictionary<PaymentScheme, IPaymentValidator>
            {
                { PaymentScheme.Bacs, new BacsPaymentValidator() },
                { PaymentScheme.FasterPayments, new FasterPaymentsPaymentValidator() },
                { PaymentScheme.Chaps, new ChapsPaymentValidator() }
            });

        [Fact]
        public void GetValidator_BacsScheme_ReturnsBacsPaymentValidator()
        {
            _factory.GetValidator(PaymentScheme.Bacs).ShouldBeOfType<BacsPaymentValidator>();
        }

        [Fact]
        public void GetValidator_FasterPaymentsScheme_ReturnsFasterPaymentsPaymentValidator()
        {
            _factory.GetValidator(PaymentScheme.FasterPayments).ShouldBeOfType<FasterPaymentsPaymentValidator>();
        }

        [Fact]
        public void GetValidator_ChapsScheme_ReturnsChapsPaymentValidator()
        {
            _factory.GetValidator(PaymentScheme.Chaps).ShouldBeOfType<ChapsPaymentValidator>();
        }

        [Fact]
        public void GetValidator_UnregisteredScheme_ReturnsNull()
        {
            var factory = new PaymentValidatorFactory(new Dictionary<PaymentScheme, IPaymentValidator>());

            factory.GetValidator(PaymentScheme.Bacs).ShouldBeNull();
        }
    }
}