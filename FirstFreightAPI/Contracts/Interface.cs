using System;
using FirstFreightAPI.Models.Stripe;

namespace FirstFreightAPI.Contracts
{
    public interface IStripeAppService
    {
        Task<StripeCustomer> AddStripeCustomerAsync(AddStripeCustomer customer, CancellationToken ct);
        Task<StripeCustomer> RemoveStripeCustomerAsync(RemoveStripeCustomer customer, CancellationToken ct);
        Task<StripeSubScription> AddStripeSubScriptionAsync(AddStripeSubScription customer, CancellationToken ct);
        Task<StripeInvoice> AddStripeInvoiceAsync(AddStripeInvoice invoice, CancellationToken ct);
        Task<StripeInvoice> EditStripeInvoiceAsync(EditStripeInvoice invoice, CancellationToken ct);
        Task<StripePayment> AddStripePaymentAsync(AddStripePayment payment, CancellationToken ct);
    }
}