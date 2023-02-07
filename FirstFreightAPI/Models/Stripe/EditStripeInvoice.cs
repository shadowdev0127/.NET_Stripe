namespace FirstFreightAPI.Models.Stripe
{
    public record EditStripeInvoice(
        string InvoiceId,
        long Quantity,
        AddStripePrice Price);
}
