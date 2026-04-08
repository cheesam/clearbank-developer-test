using System;
using ClearBank.DeveloperTest.Types;
using Shouldly;
using Xunit;

namespace ClearBank.DeveloperTest.Tests
{
    public class AccountTests
    {
        [Fact]
        public void Debit_ValidAmount_DecrementsBalance()
        {
            var account = new Account { Balance = 500m };

            account.Debit(100m);

            account.Balance.ShouldBe(400m);
        }

        [Fact]
        public void Debit_AmountExceedsBalance_Throws()
        {
            var account = new Account { Balance = 100m };

            Should.Throw<InvalidOperationException>(() => account.Debit(101m));
        }
    }
}