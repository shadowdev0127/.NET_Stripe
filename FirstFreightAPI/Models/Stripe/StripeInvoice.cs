using System;
namespace FirstFreightAPI.Models.Stripe
{
    public record StripeInvoice(
        string CustomerId,
        string InvoiceURL);
}