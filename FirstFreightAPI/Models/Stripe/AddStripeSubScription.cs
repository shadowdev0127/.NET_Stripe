using System;
namespace FirstFreightAPI.Models.Stripe
{
    public record AddStripeSubScription(
        string Email,
        string Name,
        AddStripeCard CreditCard);
}