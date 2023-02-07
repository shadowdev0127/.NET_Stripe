using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FirstFreightAPI.Contracts;
using FirstFreightAPI.Models.Stripe;
using Stripe;

namespace FirstFreightAPI.Controllers
{
    [Route("api/[controller]")]
    public class StripeController : Controller
    {
        private readonly IStripeAppService _stripeService;

        public StripeController(IStripeAppService stripeService)
        {
            _stripeService = stripeService;
        }

        [HttpPost("customer/add")]
        public async Task<ActionResult<StripeCustomer>> AddStripeCustomer(
            [FromBody] AddStripeCustomer customer,
            CancellationToken ct)
        {
            StripeCustomer createCustomer = await _stripeService.AddStripeCustomerAsync(
                customer,
                ct);

            return StatusCode(StatusCodes.Status200OK, createCustomer);
        }

        [HttpPost("customer/remove")]
        public async Task<ActionResult<StripeCustomer>> RemoveStripeCustomer(
            [FromBody] RemoveStripeCustomer customer,
            CancellationToken ct)
        {
            StripeCustomer removeCustomer = await _stripeService.RemoveStripeCustomerAsync(
                customer,
                ct);
            return StatusCode(StatusCodes.Status200OK, customer);
        }

        [HttpPost("subscription/add")]
        public async Task<ActionResult<StripeSubScription>> AddStripeSubScription(
            [FromBody] AddStripeSubScription customer,
            CancellationToken ct)
        {
            StripeSubScription createdCustomer = await _stripeService.AddStripeSubScriptionAsync(
                customer,
                ct);

            return StatusCode(StatusCodes.Status200OK, createdCustomer);
        }

        [HttpPost("invoice/add")]
        public async Task<ActionResult<StripeInvoice>> AddStripeInvoice(
            [FromBody] AddStripeInvoice invoice,
            CancellationToken ct)
        {
            StripeInvoice createdInvoice = await _stripeService.AddStripeInvoiceAsync(
                invoice,
                ct);

            return StatusCode(StatusCodes.Status200OK, createdInvoice);
        }

        [HttpPost("invoice/edit")]
        public async Task<ActionResult<StripeInvoice>> EditStripeInvoice(
            [FromBody] EditStripeInvoice invoice,
            CancellationToken ct)
        {
            StripeInvoice editInvoice = await _stripeService.EditStripeInvoiceAsync(
                invoice,
                ct);

            return StatusCode(StatusCodes.Status200OK, editInvoice);
        }

        [HttpPost("payment/add")]
        public async Task<ActionResult<StripePayment>> AddStripePayment(
            [FromBody] AddStripePayment payment,
            CancellationToken ct)
        {
            StripePayment createdPayment = await _stripeService.AddStripePaymentAsync(
                payment,
                ct);

            return StatusCode(StatusCodes.Status200OK, createdPayment);
        }

        [HttpPost("WebHook")]
        public async Task<IActionResult> WebHook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json,
                    Request.Headers["Stripe-Signature"], "whsec_a0fc2fcd077eedf79e7a8b0fbd7ad84de8070c948fe9731d13e3d858ba89131e");

                // Handle the event
                if (stripeEvent.Type == Events.PaymentIntentSucceeded)
                {
                    Console.WriteLine("Success");
                }
                else if (stripeEvent.Type == Events.CustomerSubscriptionDeleted)
                {
                    Console.WriteLine("Subscription is DELETEDDDDDDDDDDDDDDDDDDD");
                }
                else if(stripeEvent.Type == Events.InvoicePaid)
                {
                    Console.WriteLine(stripeEvent.Data.Object);
                }
                // ... handle other event types
                else
                {
                    Console.WriteLine("Unhandled event type: {0}", stripeEvent.Type);
                }

                return Ok();
            }
            catch (StripeException e)
            {
                return BadRequest();
            }
        }
    }
}