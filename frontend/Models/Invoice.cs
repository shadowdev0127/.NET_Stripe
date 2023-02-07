namespace frontend.Models
{
    public record Invoice(
        string Name,
        string Email,
        long DaysUntilDue,
        StripePrice Price);
}