using System;
using Stripe;
using FirstFreightAPI.Application;
using FirstFreightAPI.Contracts;

namespace FirstFreightAPI
{
    public static class StripeInfrastructure
    {
        public static IServiceCollection AddStripeInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            StripeConfiguration.ApiKey = configuration.GetValue<string>("StripeSettings:SecretKey");

            return services
                .AddScoped<CustomerService>()
                .AddScoped<ChargeService>()
                .AddScoped<TokenService>()
                .AddScoped<SubscriptionService>()
                .AddScoped<InvoiceService>()
                .AddScoped<InvoiceItemService>()
                .AddScoped<PriceService>()
                .AddScoped<IStripeAppService, StripeAppService>();
        }
    }
}