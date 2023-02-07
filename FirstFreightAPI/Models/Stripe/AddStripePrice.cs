using System;
namespace FirstFreightAPI.Models.Stripe
{
    public record AddStripePrice(
        long UnitAmount,
        string Currency);
}