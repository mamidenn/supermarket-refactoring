using System.Collections.Generic;
using NUnit;
using NUnit.Framework;

namespace SupermarketReceipt.Test
{
    public class SupermarketNUnitTest
    {
        SupermarketCatalog catalog;
        Product toothbrush = new Product("toothbrush", ProductUnit.Each);
        Product apples = new Product("apples", ProductUnit.Kilo);

        [SetUp]
        public void Setup()
        {
            catalog = new FakeCatalog();
            catalog.AddProduct(toothbrush, 5.0);
            catalog.AddProduct(apples, 1.99);
        }

        [TestCase]
        public void TenPercentDiscount()
        {
            // ARRANGE
            var cart = new ShoppingCart();
            cart.AddItemQuantity(apples, 2.5);

            var teller = new Teller(catalog);
            teller.AddSpecialOffer(SpecialOfferType.TenPercentDiscount, toothbrush, 10.0);

            // ACT
            var receipt = teller.ChecksOutArticlesFrom(cart);

            // ASSERT
            CheckReceipt(receipt, 1.99, 2.5, 2.5 * 1.99, 2.5 * 1.99, 0);
        }

        private void CheckReceipt(Receipt receipt,
                                  double itemPrice,
                                  double quantity,
                                  double totalPriceWithoutDiscount,
                                  double totalPriceWithDiscount,
                                  int discountAmount = 1)
        {
            Assert.That(receipt.GetTotalPrice(), Is.EqualTo(totalPriceWithDiscount));
            Assert.That(receipt.GetDiscounts().Count, Is.EqualTo(discountAmount));
            Assert.That(receipt.GetItems().Count, Is.EqualTo(1));
            var receiptItem = receipt.GetItems()[0];
            Assert.That(receiptItem.Price, Is.EqualTo(itemPrice));
            Assert.That(receiptItem.TotalPrice, Is.EqualTo(totalPriceWithoutDiscount));
            Assert.That(receiptItem.Quantity, Is.EqualTo(quantity));
        }

        [TestCase]
        public void TenPercentDiscountMultipleItems()
        {
            // ARRANGE
            var cart = new ShoppingCart();
            cart.AddItemQuantity(apples, 1);
            cart.AddItemQuantity(toothbrush, 1.0);

            var teller = new Teller(catalog);
            teller.AddSpecialOffer(SpecialOfferType.TenPercentDiscount, toothbrush, 10.0);

            // ACT
            var receipt = teller.ChecksOutArticlesFrom(cart);

            // ASSERT
            Assert.That(receipt.GetTotalPrice(), Is.EqualTo(1.99 + 4.5));
            Assert.That(receipt.GetDiscounts().Count, Is.EqualTo(1));
            Assert.That(receipt.GetItems().Count, Is.EqualTo(2));
            var receiptItem = receipt.GetItems()[0];
            Assert.That(receiptItem.Product, Is.EqualTo(apples));
            Assert.That(receiptItem.Price, Is.EqualTo(1.99));
            Assert.That(receiptItem.TotalPrice, Is.EqualTo(1.99));
            Assert.That(receiptItem.Quantity, Is.EqualTo(1));
            receiptItem = receipt.GetItems()[1];
            Assert.That(receiptItem.Product, Is.EqualTo(toothbrush));
            Assert.That(receiptItem.Price, Is.EqualTo(5));
            Assert.That(receiptItem.TotalPrice, Is.EqualTo(5));
            Assert.That(receiptItem.Quantity, Is.EqualTo(1));
        }

        [TestCase]
        public void TenPercentDiscoutIsApplied()
        {
            // ARRANGE
            var cart = new ShoppingCart();
            cart.AddItemQuantity(toothbrush, 1.0);

            var teller = new Teller(catalog);
            teller.AddSpecialOffer(SpecialOfferType.TenPercentDiscount, toothbrush, 10.0);

            // ACT
            var receipt = teller.ChecksOutArticlesFrom(cart);

            // ASSERT
            CheckReceipt(receipt, 5.0, 1, 5, 4.5);
        }

        [TestCase(5, 25, 20)]
        [TestCase(7, 35, 30)]
        [TestCase(10, 50, 40)]
        public void FiveForAmountDiscoutIsApplied(double quantity, double totalPriceWithoutDiscount, double totalPriceWithDiscount)
        {
            // ARRANGE
            var cart = new ShoppingCart();
            cart.AddItemQuantity(toothbrush, quantity);

            var teller = new Teller(catalog);
            teller.AddSpecialOffer(SpecialOfferType.FiveForAmount, toothbrush, 20.0);

            // ACT
            var receipt = teller.ChecksOutArticlesFrom(cart);

            // ASSERT
            CheckReceipt(receipt, 5.0, quantity, totalPriceWithoutDiscount, totalPriceWithDiscount);
        }
        [TestCase(2, 10, 8)]
        [TestCase(3, 15, 13)]
        [TestCase(4, 20, 16)]
        public void TwoForAmountDiscoutIsApplied(double quantity, double totalPriceWithoutDiscount, double totalPriceWithDiscount)
        {
            // ARRANGE
            var cart = new ShoppingCart();
            cart.AddItemQuantity(toothbrush, quantity);

            var teller = new Teller(catalog);
            teller.AddSpecialOffer(SpecialOfferType.TwoForAmount, toothbrush, 8.0);

            // ACT
            var receipt = teller.ChecksOutArticlesFrom(cart);

            // ASSERT
            CheckReceipt(receipt, 5.0, quantity, totalPriceWithoutDiscount, totalPriceWithDiscount);
        }
        [TestCase(3.0, 15.0, 10.0)]
        [TestCase(4.0, 20.0, 15.0)]
        [TestCase(8.0, 40.0, 30.0)]
        [TestCase(3.5, 17.5, 10)]
        public void ThreeForTwoDiscoutIsApplied(double quantity, double totalPriceWithoutDiscount, double totalPriceWithDiscount)
        {
            // ARRANGE
            var cart = new ShoppingCart();
            cart.AddItemQuantity(toothbrush, quantity);

            var teller = new Teller(catalog);
            teller.AddSpecialOffer(SpecialOfferType.ThreeForTwo, toothbrush, 10);

            // ACT
            var receipt = teller.ChecksOutArticlesFrom(cart);

            // ASSERT
            CheckReceipt(receipt, 5.0, quantity, totalPriceWithoutDiscount, totalPriceWithDiscount);

        }

        [TestCase]
        public void ThreeForTwoAndTenPercentDiscountForOneItemOnlyAppliesTenPercentDiscount()
        {
            // ARRANGE
            var cart = new ShoppingCart();
            cart.AddItemQuantity(toothbrush, 3.0);

            var teller = new Teller(catalog);
            teller.AddSpecialOffer(SpecialOfferType.ThreeForTwo, toothbrush, 10);
            teller.AddSpecialOffer(SpecialOfferType.TenPercentDiscount, toothbrush, 10);

            // ACT
            var receipt = teller.ChecksOutArticlesFrom(cart);

            // ASSERT
            CheckReceipt(receipt, 5.0, 3, 15, 13.5);
        }

        [TestCase]
        public void TwoDiscountsForOneItemOnlyAppliesLastDiscount()
        {
            // ARRANGE
            var cart = new ShoppingCart();
            cart.AddItemQuantity(toothbrush, 3.0);

            var teller = new Teller(catalog);
            teller.AddSpecialOffer(SpecialOfferType.TenPercentDiscount, toothbrush, 10);
            teller.AddSpecialOffer(SpecialOfferType.ThreeForTwo, toothbrush, 10);

            // ACT
            var receipt = teller.ChecksOutArticlesFrom(cart);

            // ASSERT
            CheckReceipt(receipt, 5.0, 3, 15, 10);
        }
    }
}