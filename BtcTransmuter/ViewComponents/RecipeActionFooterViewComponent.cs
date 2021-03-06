using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Abstractions.Triggers;
using Microsoft.AspNetCore.Mvc;

namespace BtcTransmuter.ViewComponents
{
    public class RecipeActionFooterViewComponent : ViewComponent
    {
        private readonly IEnumerable<ITriggerDescriptor> _triggerDescriptors;
        private readonly IRecipeManager _recipeManager;

        public RecipeActionFooterViewComponent(IEnumerable<ITriggerDescriptor> triggerDescriptors,
            IRecipeManager recipeManager)
        {
            _triggerDescriptors = triggerDescriptors;
            _recipeManager = recipeManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string recipeId)
        {
            var recipe = await _recipeManager.GetRecipe(recipeId);

            var properties = new Dictionary<string, object>();

            if (recipe?.RecipeTrigger != null)
            {
                var descriptor = _triggerDescriptors.First(triggerDescriptor =>
                    triggerDescriptor.TriggerId == recipe?.RecipeTrigger.TriggerId);

                if (descriptor.GetType().IsSubclassOfRawGeneric(typeof(BaseTriggerHandler<,>)))
                {
                    var type = descriptor.GetType().BaseType.GetGenericArguments().First();
                    properties = GetRecursiveAvailableProperties(type);
                }
            }

            return View(new RecipeActionFooterViewModel()
            {
                Properties = properties,
                NoRecipeTrigger = recipe?.RecipeTrigger == null
            });
        }


        private Dictionary<string, object> GetRecursiveAvailableProperties(Type type)
        {
            var properties = new Dictionary<string, dynamic>();
            var tProps = type.GetProperties();
            foreach (var prop in tProps)
            {
                properties.Add(prop.Name,
                    prop.PropertyType.IsPrimitive
                        ? (dynamic) prop.PropertyType.ToString()
                        : GetRecursiveAvailableProperties(prop.PropertyType));
            }

            return properties;
        }
    }
}