using System;
using Shouldly;
using Xunit;

namespace ClearBank.DeveloperTest.Tests
{
    public class PaymentServiceGuardTests : PaymentServiceScenario
    {
        [Fact]
        public void MakePayment_NullRequest_ThrowsArgumentNullException()
        {
            Should.Throw<ArgumentNullException>(() => CreateService().MakePayment(null));
        }

        [Fact]
        public void MakePayment_ZeroAmount_ReturnsFailure()
        {
            Request.Amount = 0;

            MakePayment().Success.ShouldBeFalse();
        }

        [Fact]
        public void MakePayment_NegativeAmount_ReturnsFailure()
        {
            Request.Amount = -1;

            MakePayment().Success.ShouldBeFalse();
        }
    }
}