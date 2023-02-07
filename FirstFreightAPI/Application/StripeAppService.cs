using System;
using Stripe;
using FirstFreightAPI.Contracts;
using FirstFreightAPI.Models.Stripe;
using System.Collections.Immutable;

namespace FirstFreightAPI.Application
{
    public class StripeAppService : IStripeAppService
    {
        private readonly ChargeService _chargeService;
        private readonly CustomerService _customerService;
        private readonly TokenService _tokenService;
        private readonly SubscriptionService _subscriptionService;
        private readonly InvoiceService _invoiceService;
        private readonly InvoiceItemService _invoiceItemService;
        private readonly PriceService _priceService;

        public StripeAppService(
            ChargeService chargeService,
            CustomerService customerService,
            TokenService tokenService,
            SubscriptionService subscriptionService,
            InvoiceService invoiceService,
            InvoiceItemService invoiceItemService,
            PriceService priceService)
        {
            _chargeService = chargeService;
            _customerService = customerService;
            _tokenService = tokenService;
            _subscriptionService = subscriptionService;
            _invoiceService = invoiceService;
            _invoiceItemService = invoiceItemService;
            _priceService = priceService;
        }

        /// <summary>
        /// Create a new customer at Stripe through API using customer and card details from records.
        /// </summary>
        /// <param name="customer">Stripe Customer</param>
        /// <param name="ct">Cancellation Token</param>
        /// <returns>Stripe Customer</returns>
        /// 
        public async Task<StripeCustomer> AddStripeCustomerAsync(AddStripeCustomer customer, CancellationToken ct)
        {
            string query = "name:'" + customer.Name + "'";
            CustomerSearchOptions customerSearchOptions = new CustomerSearchOptions
            {
                Query = query
            };
            var customerSearch = _customerService.Search(customerSearchOptions);

            CustomerCreateOptions customerCreateOptions = new CustomerCreateOptions
            {
                Name = customer.Name,
                Email= customer.Email,
                Description = customer.Description,
            };
            Customer customerCreate = _customerService.Create(customerCreateOptions);
            return new StripeCustomer(customerCreate.Id);
        }

        public async Task<StripeCustomer> RemoveStripeCustomerAsync(RemoveStripeCustomer customer, CancellationToken ct)
        {
            string query = "email:'" + customer.Email + "'";
            CustomerSearchOptions customerSearchOptions = new CustomerSearchOptions 
            {
                Query = query 
            };
            var customerSearch = _customerService.Search(customerSearchOptions);
            var deleteCustomerId = "";
            if(!customerSearch.Data.Any())
            {
                return new StripeCustomer("I can't fount customer");
            }
            else
            {
                deleteCustomerId = customerSearch.Data[0].Id;
            }
            var result = _customerService.Delete(deleteCustomerId);
            return new StripeCustomer(result.Id);
        }
        public async Task<StripeSubScription> AddStripeSubScriptionAsync(AddStripeSubScription customer, CancellationToken ct)
        {
            // Set Stripe Token options based on customer data
            TokenCreateOptions tokenOptions = new TokenCreateOptions
            {
                Card = new TokenCardOptions
                {
                    Name = customer.Name,
                    Number = customer.CreditCard.CardNumber,
                    ExpYear = customer.CreditCard.ExpirationYear,
                    ExpMonth = customer.CreditCard.ExpirationMonth,
                    Cvc = customer.CreditCard.Cvc
                }
            };

            // Create new Stripe Token
            Token stripeToken = await _tokenService.CreateAsync(tokenOptions, null, ct);


            // Set Customer options using
            CustomerCreateOptions customerOptions = new CustomerCreateOptions
            {
                Name = customer.Name,
                Email = customer.Email,
            };


            // Create customer at Stripe
            Customer createdCustomer = await _customerService.CreateAsync(customerOptions, null, ct);

            CustomerUpdateOptions option = new CustomerUpdateOptions
            {
                Source = stripeToken.Id
            };

            Customer updatedCustomer = await _customerService.UpdateAsync(createdCustomer.Id, option, null, ct);

            // Create Subscription
            SubscriptionCreateOptions subscription = new SubscriptionCreateOptions
            {

                Customer = createdCustomer.Id,
                Items = new List<SubscriptionItemOptions>
                  {
                    new SubscriptionItemOptions
                    {
                      Price = "price_1MXovSBxduZxIUO7WvoByglB",
                    },
                  },

            };

            Subscription subscriptionCreated = await _subscriptionService.CreateAsync(subscription, null, ct);


            // Return the created customer at stripe
            return new StripeSubScription(createdCustomer.Name, createdCustomer.Email, createdCustomer.Id);
        }

        public async Task<StripeInvoice> AddStripeInvoiceAsync(AddStripeInvoice invoice, CancellationToken ct)
        {
            // Create Price
            PriceCreateOptions priceCreateOptions = new PriceCreateOptions
            {
                UnitAmount = invoice.Price.UnitAmount,
                Currency = invoice.Price.Currency,
                Product = "prod_NIPoSpR2FmbYbu"
            };
            Price createdPrice = await _priceService.CreateAsync(priceCreateOptions, null, ct);

            // Set the options for the payment we would like to create at Stripe
            CustomerCreateOptions customerOptions = new CustomerCreateOptions
            {
                Name = invoice.Name,
                Email = invoice.Email
            };

            Customer createdCustomer = await _customerService.CreateAsync(customerOptions, null, ct);

            // Create Invoice
            InvoiceCreateOptions invoiceOption = new InvoiceCreateOptions
            {
                Customer = createdCustomer.Id,
                CollectionMethod = "send_invoice",
                DaysUntilDue = invoice.DaysUntilDue,
            };

            Invoice invoiceService = await _invoiceService.CreateAsync(invoiceOption, null, ct);

            InvoiceItemCreateOptions invoiceItemOption = new InvoiceItemCreateOptions
            {
                Customer = createdCustomer.Id,
                Price = createdPrice.Id,
                Invoice = invoiceService.Id,
            };

            InvoiceItem invoiceItemService = await _invoiceItemService.CreateAsync(invoiceItemOption, null, ct);

            var invoiceSendService = new InvoiceService();
            invoiceSendService.FinalizeInvoice(invoiceService.Id);
            var invoiceDetails = invoiceSendService.SendInvoice(invoiceService.Id);

            return new StripeInvoice(
                createdCustomer.Id, invoiceDetails.HostedInvoiceUrl
            );

        }

        public async Task<StripeInvoice> EditStripeInvoiceAsync(EditStripeInvoice invoice, CancellationToken ct)
        {
            InvoiceCreateOptions invoiceCreateOptions = new InvoiceCreateOptions
            {
                FromInvoice = new InvoiceFromInvoiceOptions
                {
                    Invoice = invoice.InvoiceId,
                    Action = "revision",
                },
            };

            Invoice newInvoice = await _invoiceService.CreateAsync(invoiceCreateOptions, null, ct);

            PriceCreateOptions priceCreateOptions = new PriceCreateOptions
            {
                UnitAmount = invoice.Price.UnitAmount,
                Currency = invoice.Price.Currency,
                Product = "prod_NIPoSpR2FmbYbu"
            };

            Price createPrice = await _priceService.CreateAsync(priceCreateOptions, null, ct);

            InvoiceItemUpdateOptions invoiceItemUpdateOptions = new InvoiceItemUpdateOptions
            {
                Price = createPrice.Id,
                Quantity = invoice.Quantity,
                Description = "Change the inovice amount"
            };

            InvoiceItem updateInvoiceItem = await _invoiceItemService.UpdateAsync(newInvoice.Lines.Data[0].InvoiceItem, invoiceItemUpdateOptions, null, ct);

            var finalizeInvoiceService = new InvoiceService();
            var finalizeInvoice = finalizeInvoiceService.FinalizeInvoice(newInvoice.Id);
            var sendInvoice = finalizeInvoiceService.SendInvoice(newInvoice.Id);
            return new StripeInvoice(sendInvoice.CustomerId, sendInvoice.HostedInvoiceUrl);
        }

        /// <summary>
        /// Add a new payment at Stripe using Customer and Payment details.
        /// Customer has to exist at Stripe already.
        /// </summary>
        /// <param name="payment">Stripe Payment</param>
        /// <param name="ct">Cancellation Token</param>
        /// <returns><Stripe Payment/returns>
        public async Task<StripePayment> AddStripePaymentAsync(AddStripePayment payment, CancellationToken ct)
        {
            // Set the options for the payment we would like to create at Stripe
            ChargeCreateOptions paymentOptions = new ChargeCreateOptions
            {
                Customer = payment.CustomerId,
                ReceiptEmail = payment.ReceiptEmail,
                Description = payment.Description,
                Currency = payment.Currency,
                Amount = payment.Amount
            };

            // Create the payment
            var createdPayment = await _chargeService.CreateAsync(paymentOptions, null, ct);

            // Return the payment to requesting method
            return new StripePayment(
              createdPayment.CustomerId,
              createdPayment.ReceiptEmail,
              createdPayment.Description,
              createdPayment.Currency,
              createdPayment.Amount,
              createdPayment.Id);
        }
    }
}