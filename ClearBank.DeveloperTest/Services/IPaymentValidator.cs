using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Services
{
    public interface IPaymentValidator
    {
        bool Validate(Account account, MakePaymentRequest request);
    }
}
