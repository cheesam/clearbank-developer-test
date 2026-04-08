using System;

namespace ClearBank.DeveloperTest.Types
{
    public class Account
    {
        public string AccountNumber { get; set; }
        public decimal Balance { get; set; }
        public AccountStatus Status { get; set; }
        public AllowedPaymentSchemes AllowedPaymentSchemes { get; set; }

        public void Debit(decimal amount)
        {
            if (amount > Balance)
                throw new InvalidOperationException($"Debit amount of {amount} exceeds account balance of {Balance}.");

            Balance -= amount;
        }
    }
}