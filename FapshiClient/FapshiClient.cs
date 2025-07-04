using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FapshiClient
{
    public class FapshiClient : IFapshiClient
    {
        private readonly HttpClient _httpClient;
        private readonly FapshiOptions _options;

        public FapshiClient(HttpClient httpClient, IOptions<FapshiOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;

            if (string.IsNullOrWhiteSpace(_options.ApiUser) || string.IsNullOrWhiteSpace(_options.ApiKey))
                throw new ArgumentException("Fapshi API credentials are not configured.");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("apiuser", _options.ApiUser);
            _httpClient.DefaultRequestHeaders.Add("apikey", _options.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
        }

        private static (bool valid, string error) ValidateAmount(int amount) =>
            amount >= 100 ? (true, "") : (false, "amount cannot be less than 100 XAF");

        private static (bool valid, string error) ValidatePhone(string phone) =>
            Regex.IsMatch(phone, @"^6[0-9]{8}$") ? (true, "") : (false, "invalid phone number");

        private async Task<(T? data, string? error, int status)> ExecuteAsync<T>(Func<Task<HttpResponseMessage>> call)
            where T : class, new()
        {
            try
            {
                var res = await call();
                var json = await res.Content.ReadAsStringAsync();

                T data;
                try { data = JsonSerializer.Deserialize<T>(json) ?? new T(); }
                catch { data = new T(); }

                int status = (int)res.StatusCode;
                if (data is FapshiResponse baseRes) baseRes.StatusCode = status;
                return (data, null, status);
            }
            catch (Exception ex)
            {
                return (null, ex.Message, 500);
            }
        }

        private Task<UserTransactionsResponse> ExecuteTransactionsGetAsync(string url)
            => ExecuteAsync<UserTransactionsResponse>(() => _httpClient.GetAsync(url))
               .ContinueWith(t =>
               {
                   if (t.Result.data is UserTransactionsResponse ut)
                       return ut;

                   return new UserTransactionsResponse { StatusCode = t.Result.status, Message = t.Result.error };
               });

        public async Task<InitiatePayResponse> CreatePaymentLinkAsync(int amount, string? email = null, string? redirectUrl = null, string? userId = null, string? externalId = null, bool? cardOnly = null)
        {
            var (ok, err) = ValidateAmount(amount);
            if (!ok) return new InitiatePayResponse { StatusCode = 400, Message = err };

            var payload = new Dictionary<string, object> { ["amount"] = amount };
            if (!string.IsNullOrWhiteSpace(email)) payload["email"] = email;
            if (!string.IsNullOrWhiteSpace(redirectUrl)) payload["redirectUrl"] = redirectUrl;
            if (!string.IsNullOrWhiteSpace(userId)) payload["userId"] = userId;
            if (!string.IsNullOrWhiteSpace(externalId)) payload["externalId"] = externalId;
            if (cardOnly.HasValue) payload["cardOnly"] = cardOnly.Value;

            var (data, error, status) = await ExecuteAsync<InitiatePayResponse>(() =>
                _httpClient.PostAsync($"{_options.BaseUrl}/initiate-pay",
                    new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")));

            return data ?? new InitiatePayResponse { StatusCode = status, Message = error };
        }

        public async Task<DirectPayResponse> ChargePhoneWalletAsync(int amount, string phone)
        {
            var (ok1, e1) = ValidateAmount(amount);
            var (ok2, e2) = ValidatePhone(phone);
            if (!ok1) return new DirectPayResponse { StatusCode = 400, Message = e1 };
            if (!ok2) return new DirectPayResponse { StatusCode = 400, Message = e2 };

            var payload = new { amount, phone };
            var (data, error, status) = await ExecuteAsync<DirectPayResponse>(() =>
                _httpClient.PostAsync($"{_options.BaseUrl}/direct-pay",
                    new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")));

            return data ?? new DirectPayResponse { StatusCode = status, Message = error };
        }

        public async Task<PaymentStatusResponse> GetPaymentStatusAsync(string transId)
        {
            if (string.IsNullOrWhiteSpace(transId) || !Regex.IsMatch(transId, @"^[a-zA-Z0-9]{8,10}$"))
                return new PaymentStatusResponse { StatusCode = 400, Message = "invalid transaction id" };

            var (data, error, status) = await ExecuteAsync<PaymentStatusResponse>(() =>
                _httpClient.GetAsync($"{_options.BaseUrl}/payment-status/{transId}"));

            return data ?? new PaymentStatusResponse { StatusCode = status, Message = error };
        }

        public async Task<ExpirePayResponse> CancelPaymentLinkAsync(string transId)
        {
            if (string.IsNullOrWhiteSpace(transId) || !Regex.IsMatch(transId, @"^[a-zA-Z0-9]{8,10}$"))
                return new ExpirePayResponse { StatusCode = 400, Message = "invalid transaction id" };

            var payload = new { transId };
            var (data, error, status) = await ExecuteAsync<ExpirePayResponse>(() =>
                _httpClient.PostAsync($"{_options.BaseUrl}/expire-pay",
                    new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")));

            return data ?? new ExpirePayResponse { StatusCode = status, Message = error };
        }

        public async Task<BalanceResponse> GetAccountBalanceAsync()
        {
            var (data, error, status) = await ExecuteAsync<BalanceResponse>(() =>
                _httpClient.GetAsync($"{_options.BaseUrl}/balance"));

            return data ?? new BalanceResponse { StatusCode = status, Message = error };
        }

        public async Task<PayoutResponse> DisburseToPhoneAsync(int amount, string phone)
        {
            var (ok1, e1) = ValidateAmount(amount);
            var (ok2, e2) = ValidatePhone(phone);
            if (!ok1) return new PayoutResponse { StatusCode = 400, Message = e1 };
            if (!ok2) return new PayoutResponse { StatusCode = 400, Message = e2 };

            var payload = new { amount, phone };
            var (data, error, status) = await ExecuteAsync<PayoutResponse>(() =>
                _httpClient.PostAsync($"{_options.BaseUrl}/payout",
                    new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")));

            return data ?? new PayoutResponse { StatusCode = status, Message = error };
        }

        public Task<UserTransactionsResponse> GetUserTransactionsAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId) || !Regex.IsMatch(userId, @"^[a-zA-Z0-9-_]{1,100}$"))
                return Task.FromResult(new UserTransactionsResponse { StatusCode = 400, Message = "invalid user id" });

            return ExecuteTransactionsGetAsync($"{_options.BaseUrl}/transaction/{userId}");
        }

        public Task<UserTransactionsResponse> SearchTransactionsAsync(Dictionary<string, string> filters)
        {
            if (filters == null || filters.Count == 0)
                return Task.FromResult(new UserTransactionsResponse { StatusCode = 400, Message = "search filters required" });

            var q = new FormUrlEncodedContent(filters).ReadAsStringAsync().Result;
            return ExecuteTransactionsGetAsync($"{_options.BaseUrl}/search?{q}");
        }
    }
}
