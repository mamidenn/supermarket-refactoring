using System.Collections.Generic;
using System.Globalization;

namespace SupermarketReceipt
{
    public class ShoppingCart
    {
        private readonly List<ProductQuantity> _items = new List<ProductQuantity>();
        private readonly Dictionary<Product, double> _productQuantities = new Dictionary<Product, double>();
        private static readonly CultureInfo Culture = CultureInfo.CreateSpecificCulture("en-GB");


        public List<ProductQuantity> GetItems()
        {
            return new List<ProductQuantity>(_items);
        }

        public void AddItem(Product product)
        {
            AddItemQuantity(product, 1.0);
        }


        public void AddItemQuantity(Product product, double quantity)
        {
            _items.Add(new ProductQuantity(product, quantity));
            if (_productQuantities.ContainsKey(product))
            {
                var newAmount = _productQuantities[product] + quantity;
                _productQuantities[product] = newAmount;
            }
            else
            {
                _productQuantities.Add(product, quantity);
            }
        }

        public void HandleOffers(Receipt receipt, Dictionary<Product, Offer> offers, SupermarketCatalog catalog)
        {
            foreach (var p in _productQuantities.Keys)
            {
                var quantity = _productQuantities[p];
                var quantityAsInt = (int)quantity;
                if (offers.ContainsKey(p))
                {
                    var offer = offers[p];
                    var unitPrice = catalog.GetUnitPrice(p);
                    var divisor = offer.OfferType switch
                    {
                        SpecialOfferType.TwoForAmount => 2,
                        SpecialOfferType.ThreeForTwo => 3,
                        SpecialOfferType.FiveForAmount => 5,
                        _ => 1
                    };

                    if (offer.OfferType == SpecialOfferType.TwoForAmount)
                    {
                        if (quantityAsInt >= 2)
                        {
                            var total = offer.Argument * (quantityAsInt / divisor) + quantityAsInt % 2 * unitPrice;
                            var discountN = unitPrice * quantity - total;
                            receipt.AddDiscount(new Discount(p, "2 for " + PrintPrice(offer.Argument), -discountN));
                            return;
                        }
                    }
                    if (offer.OfferType == SpecialOfferType.ThreeForTwo)
                    {
                        if (quantityAsInt > 2)
                        {
                            var discountAmount = quantity * unitPrice - (quantityAsInt / divisor * 2 * unitPrice + quantityAsInt % 3 * unitPrice);
                            receipt.AddDiscount(new Discount(p, "3 for 2", -discountAmount));
                            return;
                        }
                    }
                    if (offer.OfferType == SpecialOfferType.FiveForAmount)
                    {
                        if (quantityAsInt >= 5)
                        {
                            var discountTotal = unitPrice * quantity - (offer.Argument * (quantityAsInt / divisor) + quantityAsInt % 5 * unitPrice);
                            receipt.AddDiscount(new Discount(p, divisor + " for " + PrintPrice(offer.Argument), -discountTotal));
                            return;
                        }
                    }
                    if (offer.OfferType == SpecialOfferType.TenPercentDiscount)
                    {
                        receipt.AddDiscount(new Discount(p, offer.Argument + "% off", -quantity * unitPrice * offer.Argument / 100.0));
                        return;
                    }
                }
            }
        }

        private string PrintPrice(double price)
        {
            return price.ToString("N2", Culture);
        }
    }
}