namespace FirstFreightAPI.Models.Stripe
{
    public record AddStripeCustomer(
        string Name,
        string Email,
        string Description
        );
}
