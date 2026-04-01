//Anything that works with Steamworks should only compile if Steamworks is available
#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using System;
using UnityEngine;

//You must "use" the Heathen namespace to access our tools
using Heathen.SteamworksIntegration;
//In our examples we will also work with the API so we add its namespace as well
using Heathen.SteamworksIntegration.API;
//In some cases you also need the Steamworks namespace so you can work with its native enums and similar
using Steamworks;
//This is here so we can work with common collections e.g. List<T>
using System.Collections.Generic;


//You must ALWAYS define your code in a properly formed namespace
//You can obviously name it whatever you want
namespace MyProperlyFormedNamespaceName
{
    [Obsolete("This script is for demonstration purposes only and should NEVER be used as is in any project.")]
    public class Snipit_ItemData : MonoBehaviour
    {
#if UNITY_EDITOR
        private void Start()
        {
            //You can get an ItemData for any of your items from its Item ID
            //In the example below we assume 100 is a valid Item ID that you have
            //Already set up in your Steamworks Developer Portal
            ItemData thisItem = 100;

            //With an ItemData you can now work with this item with simple calls

            //The localised name of the item as seen by this user if any
            var theName = thisItem.Name;

            //The following functions all demonstrate specific parts of Steamworks Inventory check the function listings to find 
            //the conde snipit that suits your needs
        }

        public void CheckOwnership(int ItemID)
        {
            //Lets convert our Item ID into a ItemData for easier work
            ItemData thisItem = ItemID;

            //So you would do this of course only after you where sure you had a good view of the current inventory e.g. after running
            Inventory.Client.GetAllItems(result =>
            {
                //The result tells you about all the items found ... if you care ... or you can just use the code below to check items on demand
                //point is your game's internal view of what the player "owns" is now refreshed for all items
            });

            if (thisItem.GetTotalQuantity() > 0)
                ;// Yep they own at least 1
            else
                ;// Note, they don't own any of these
        }

        public void ItemPricing(int ItemID)
        {
            //Lets convert our Item ID into a ItemData for easier work
            ItemData thisItem = ItemID;

            //Pricing if available
            if (thisItem.HasPrice)
            {
                //Prices are always in base 100 so for USD you would generally want to divide by 100 to convert 199 to 1.99
                //Note that the price is expressed in the users local currency
                var currencySymbol = Inventory.Client.LocalCurrencySymbol;
                var currencyCode = Inventory.Client.LocalCurrencyCode;

                //The base price is not modified by sales or discounts and is the "base" price of the item
                var basePrice = thisItem.BasePrice;

                //The current price in contrast is the current displayed price the user would see in store.
                //it accounts for any sales
                var currentPrice = thisItem.CurrentPrice;

                //You can also use ItemData to get the preformatted current price based on the local currency symbol
                var priceString = thisItem.CurrentPriceString();
            }
        }

        public void StartPurchase(int ItemID)
        {
            //Lets convert our Item ID into a ItemData for easier work
            ItemData thisItem = ItemID;

            //If your doing purchasing in game you probably want to know when a purchase is completed
            //To do that you would "listen" on the transaction complete event
            Inventory.Client.OnSteamMicroTransactionAuthorizationResponse.AddListener(HandleTransactionAuthResponse);

            //You can also "Start Purchase" on an item that has a price
            //Note if you call this on an item that does not have a proper price set up or is hidden it does nothing

            //When you call this all it is doing is adding the item to the user's cart and opening the cart in the Steam Overlay
            thisItem.StartPurchase((result, ioError) =>
            {
                //This is a lambda expression for an anonymous function and is the typical way to handle these context sensitive 
                //callbacks you can use a named function if you like as seen with the AddListener above

                //Its important to get used to ioError most Steam callbacks use it ... it is simply a bool value
                //that if true indicates an IO error has occurred e.g. Steam didn't get or accept the call probably for some
                //hard failure reason, generally if you get an IO error (very rare) you can just try again
                if (!ioError)
                {
                    //Valve's internal order ID for this process 
                    //If you plan on tracking when this order is completed you would want to save this so you can check against it when the transaction complete event comes in
                    var orderId = result.m_ulOrderID;

                    //Valve's internal transaction ID for this process
                    var transactionId = result.m_ulTransID;
                }
            });
        }

        public void HandleTransactionAuthResponse(AppId_t AppId, ulong OrderId, bool WasAuthorized)
        {
            //If this was for this app
            if(AppId == AppData.Me)
            {
                // Check which order this is by matching the input OrderID to the OrderID given when you called StartPurchase
                if(WasAuthorized)
                {
                    //They did authorize the order ... do note 
                    //A user could have modified the order before completing so it may not be the same content it was when you started the purchase
                    //You can either 
                    // A) Depend on inventory update to tell you what changed
                    // Inventory.Client.EventSteamInventoryResultReady.AddListener(HandleNewResultsReady);
                    // This gets called any time inventory results have been reported as updated/ready such as after a purchase
                    // B) You could refresh you games view of the inventory ... this is generally a good idea at key points
                    //    Such as when a purchase has been completed
                    Inventory.Client.GetAllItems(result =>
                    {
                        //You can either read the result to see specifically all the items
                        //or just check the items you care about using ItemData such as thisItem.GetTotalQuantity();
                    });
                }
            }
        }

        public void PromoItems()
        {
            //You can give players items for free such as for ownership of other apps or simply as some starter content.
            //In order to add an item as a promo item you must configure its Item Defintion in Steamworks developer portal accordingly
            //We have extensive guides and articles on the topic as does Valve ... assuming that is set up here is how you work with it
            //in your game code

            //Any items that have automated rules such as ownership of a given app, play time, etc. can be granted in bulk
            //This will only ever drop once so its safe to call this blindly such as on start of a game, etc.
            Inventory.Client.GrantPromoItems(result =>
            {
                //This is a lambda expression for an anonymous function and is the typical way to handle these context sensitive 
                //callbacks you can use a named function if you like

                //the result is an InventoryResult of the items modified by the request if any
            });


            //Manual promo items or items that have a manual rule have to be added specifically ... so lets see how we would do that
            // First we set up an ItemData to work with, you would use the ID of a promo item you wanted to work with
            ItemData thisItem = 100;

            //Next we call Add Promo Item on that item, it will only be added if the user has meet any requirements the defintion
            //has defined so this is safe to call as required.
            thisItem.AddPromoItem(result =>
            {
                //This is a lambda expression for an anonymous function and is the typical way to handle these context sensitive 
                //callbacks you can use a named function if you like

                //the result is an InventoryResult of the items modified by the request if any
            });
        }

        public void PlayTimeItemGenerators()
        {
            //You can define special items called "playtime item generators" that produce items based on playtime rules
            //These items use a different system for "dropping" and are called per item.

            // First we set up an ItemData to work with, you would use the ID of a promo item you wanted to work with
            ItemData thisItem = 100;

            thisItem.TriggerDrop(result =>
            {
                //This is a lambda expression for an anonymous function and is the typical way to handle these context sensitive 
                //callbacks you can use a named function if you like

                //the result is an InventoryResult of the items modified by the request if any
            });
        }

        public void ExchangeItems()
        {
            //You can allow your players to exchange 1 or more items for some other item
            //This can be used for an in-game shop that uses in-game currency
            //It can be used for a crafting system e.g. exchange iron and wood for a sword
            //It can be used for opening loot box style systems with randomly generated content 

            //You are required to define the "recipe" on the item definition of the item you will get 
            //as a result of the exchange. This is again done in Steamworks developer portal
            //We have guides to help you get going the following assumes that is done and is how you would
            //Then work with it in code

            //In all cases you will be working with multiple items, for this snipit we will refer
            //to the items being exchanged as "reagents" and the item we want as "item"
            ItemData reagentA = 100;
            ItemData reagentB = 101;
            ItemData item = 200;

            //1st we need to get a pointer to the required number of each reagent we need
            //lets assume we need 4 of reagentA and 10 of reagentB
            if(reagentA.GetExchangeEntry(4, out var regAEntries))
            {
                //We have enough A and a pointer to them so lets check B now

                if(reagentB.GetExchangeEntry(10, out var regBEntries))
                {
                    //We have enough B and a pointer to them so lets merge the ingredients into a single collection
                    List<ExchangeEntry> ingredients = new();
                    ingredients.AddRange(regAEntries);
                    ingredients.AddRange(regBEntries);

                    //Now we are ready to exchange
                    item.Exchange(ingredients, result =>
                    {
                        //This is a lambda expression for an anonymous function and is the typical way to handle these context sensitive 
                        //callbacks you can use a named function if you like

                        //the result is an InventoryResult of the items modified by the request if any
                    });
                }
            }
        }

        public void ConsumeItem()
        {
            // You can remove an item from the user's inventory in a process called Consume
            // Most games wouldn't do this but if you did have consumable items or
            // In some cases when your syncing Steam inventory with a 3rd party inventory system
            // It can be useful to remove the item from the player's Steam inventory.

            // First we set up an ItemData to work with, you would use the ID of a promo item you wanted to work with
            ItemData thisItem = 100;

            //This will consume just 1 quantity from this item if available
            thisItem.Consume(result =>
            {
                //This is a lambda expression for an anonymous function and is the typical way to handle these context sensitive 
                //callbacks you can use a named function if you like

                //the result is an InventoryResult of the items modified by the request if any
            });

            // Consume a number of this item assuming the player has this many
            thisItem.Consume(42, result =>
            {
                //This is a lambda expression for an anonymous function and is the typical way to handle these context sensitive 
                //callbacks you can use a named function if you like

                //the result is an InventoryResult of the items modified by the request if any
            });
        }
#endif
    }
}
#endif