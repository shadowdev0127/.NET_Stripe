namespace frontend.Models
{
    public record StripePrice(
        long UnitAmount,
        string Currency);
}