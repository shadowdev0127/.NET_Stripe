using System;
namespace FirstFreightAPI.Models.Stripe
{
    public record AddStripeInvoice(
        string Name, 
        string Email,
        long DaysUntilDue,
        AddStripePrice Price);
}