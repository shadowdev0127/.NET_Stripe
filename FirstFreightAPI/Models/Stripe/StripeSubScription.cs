using System;
namespace FirstFreightAPI.Models.Stripe
{
    public record StripeSubScription(
        string Name,
        string Email,
        string CustomerId);
}