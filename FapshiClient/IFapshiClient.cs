using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FapshiClient
{
    public interface IFapshiClient
    {
        Task<InitiatePayResponse> CreatePaymentLinkAsync(int amount, string? email = null, string? redirectUrl = null, string? userId = null, string? externalId = null, bool? cardOnly = null);
        Task<DirectPayResponse> ChargePhoneWalletAsync(int amount, string phone);
        Task<PaymentStatusResponse> GetPaymentStatusAsync(string transId);
        Task<ExpirePayResponse> CancelPaymentLinkAsync(string transId);
        Task<BalanceResponse> GetAccountBalanceAsync();
        Task<PayoutResponse> DisburseToPhoneAsync(int amount, string phone);
        Task<UserTransactionsResponse> GetUserTransactionsAsync(string userId);
        Task<UserTransactionsResponse> SearchTransactionsAsync(Dictionary<string, string> filters);
    }
}
