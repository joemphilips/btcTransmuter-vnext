using System;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.Helpers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.Exchange.ExternalServices.Exchange;
using ExchangeSharp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace BtcTransmuter.Extension.Exchange.Actions.PlaceOrder
{
    public class PlaceOrderDataActionHandler : BaseActionHandler<PlaceOrderData>, IActionDescriptor
    {
        public override string ActionId => "PlaceOrder";
        public string Name => "Place Order";

        public string Description =>
            "Place an order on an exchange";

        public string ViewPartial => "ViewPlaceOrderAction";

        public Task<IActionResult> EditData(RecipeAction data)
        {
            using (var scope = DependencyHelper.ServiceScopeFactory.CreateScope())
            {
                var identifier = $"{Guid.NewGuid()}";
                var memoryCache = scope.ServiceProvider.GetService<IMemoryCache>();
                memoryCache.Set(identifier, data, new MemoryCacheEntryOptions()
                {
                    SlidingExpiration = TimeSpan.FromMinutes(60)
                });

                return Task.FromResult<IActionResult>(new RedirectToActionResult(
                    nameof(PlaceOrderController.EditData),
                    "PlaceOrder", new
                    {
                        identifier
                    }));
            }
        }

        protected override Task<bool> CanExecute(object triggerData, RecipeAction recipeAction)
        {
            return Task.FromResult(recipeAction.ActionId == ActionId);
        }

        protected override async Task<ActionHandlerResult> Execute(object triggerData, RecipeAction recipeAction,
            PlaceOrderData actionData)
        {
            var exchangeService = new ExchangeService(recipeAction.ExternalService);
            var client = exchangeService.ConstructClient();

            var orderRequest = new ExchangeOrderRequest()
            {
                OrderType = actionData.OrderType,
                Price = Convert.ToDecimal(InterpolateString(actionData.Price, triggerData)),
                Amount = Convert.ToDecimal(InterpolateString(actionData.Amount, triggerData)),
                StopPrice = Convert.ToDecimal(InterpolateString(actionData.StopPrice, triggerData)),
                IsBuy = actionData.IsBuy,
                IsMargin = actionData.IsMargin,
            };
            try
            {
                var result = await client.PlaceOrderAsync(orderRequest);
                System.Threading.Thread.Sleep(500);
                result = await client.GetOrderDetailsAsync(result.OrderId);
                return new ActionHandlerResult()
                {
                    Executed = true,
                    Result =
                        $"Place order ({result.OrderId}) Status: {result.Result}"
                };
            }
            catch (Exception e)
            {
                return new ActionHandlerResult()
                {
                    Executed = false,
                    Result =
                        $"Could not place order because {e.Message}"
                };
            }
        }
    }
}