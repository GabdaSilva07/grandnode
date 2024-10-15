using FluentAssertions;
using Grand.Core;
using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Orders;
using Grand.Domain.Shipping;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Orders;
using Grand.Services.Shipping;
using Grand.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HackTests
{
    public class CheckoutControllerTests
    {
        [Test]
        [AutoDomainData]
        public async Task SelectBillingAddress_AddressNotFound_RedirectsToCheckoutBillingAddress(
            Mock<IWorkContext> workContext,
            Mock<IStoreContext> storeContext,
            Mock<ICustomerService> customerService,
            Mock<IGenericAttributeService> genericAttributeService,
            Mock<IShoppingCartService> shoppingCartService)
        {
            // Arrange
            var customer = new Customer();
            workContext.Setup(x => x.CurrentCustomer).Returns(customer);

            var controller = new CheckoutController(
                workContext.Object,
                storeContext.Object,
                null, // ILocalizationService
                customerService.Object,
                shoppingCartService.Object,
                genericAttributeService.Object,
                null, // IShippingService
                null, // IPickupPointService
                null, // IPaymentService
                null, // IPluginFinder
                null, // ILogger
                null, // IOrderService
                null, // IWebHelper
                null, // IAddressAttributeParser
                null, // ICustomerActivityService
                null, // IMediator
                null, // OrderSettings
                null, // RewardPointsSettings
                null, // PaymentSettings
                null, // ShippingSettings
                null  // AddressSettings
            );

            // Act
            var result = await controller.SelectBillingAddress("nonexistent-address-id");

            // Assert
            result.Should().BeOfType<RedirectToRouteResult>();
            var redirectResult = (RedirectToRouteResult)result;
            redirectResult.RouteName.Should().Be("CheckoutBillingAddress");
        }

        [Test]
        [AutoDomainData]
        public async Task SelectBillingAddress_ValidAddress_UpdatesBillingAddress(
            Mock<IWorkContext> workContext,
            Mock<IStoreContext> storeContext,
            Mock<ICustomerService> customerService,
            Mock<IGenericAttributeService> genericAttributeService,
            Mock<IShoppingCartService> shoppingCartService)
        {
            // Arrange
            var addressId = "test-address-id";
            var customer = new Customer
            {
                Addresses = new List<Address>
                {
                    new Address { Id = addressId }
                }
            };
            workContext.Setup(x => x.CurrentCustomer).Returns(customer);

            var controller = new CheckoutController(
                workContext.Object,
                storeContext.Object,
                null, // ILocalizationService
                customerService.Object,
                shoppingCartService.Object,
                genericAttributeService.Object,
                null, // IShippingService
                null, // IPickupPointService
                null, // IPaymentService
                null, // IPluginFinder
                null, // ILogger
                null, // IOrderService
                null, // IWebHelper
                null, // IAddressAttributeParser
                null, // ICustomerActivityService
                null, // IMediator
                null, // OrderSettings
                null, // RewardPointsSettings
                null, // PaymentSettings
                null, // ShippingSettings
                null  // AddressSettings
            );

            // Act
            var result = await controller.SelectBillingAddress(addressId);

            // Assert
            customer.BillingAddress.Id.Should().Be(addressId);
            customerService.Verify(x => x.UpdateBillingAddress(It.IsAny<Address>()), Times.Once);
        }

        [Test]
        [AutoDomainData]
        public async Task SelectBillingAddress_ShipToSameAddressTrue_UpdatesShippingAddress(
            Mock<IWorkContext> workContext,
            Mock<IStoreContext> storeContext,
            Mock<ICustomerService> customerService,
            Mock<IGenericAttributeService> genericAttributeService,
            Mock<IShoppingCartService> shoppingCartService,
            Mock<IShippingService> shippingService)
        {
            // Arrange
            var addressId = "test-address-id";
            var customer = new Customer
            {
                Addresses = new List<Address>
                {
                    new Address { Id = addressId }
                }
            };
            workContext.Setup(x => x.CurrentCustomer).Returns(customer);
            shoppingCartService.Setup(x => x.GetShoppingCart(It.IsAny<string>(), It.IsAny<ShoppingCartType>(), It.IsAny<ShoppingCartType>()))
                .Returns(new List<ShoppingCartItem> { new ShoppingCartItem() });

            var controller = new CheckoutController(
                workContext.Object,
                storeContext.Object,
                null, // ILocalizationService
                customerService.Object,
                shoppingCartService.Object,
                genericAttributeService.Object,
                shippingService.Object,
                null, // IPickupPointService
                null, // IPaymentService
                null, // IPluginFinder
                null, // ILogger
                null, // IOrderService
                null, // IWebHelper
                null, // IAddressAttributeParser
                null, // ICustomerActivityService
                null, // IMediator
                null, // OrderSettings
                null, // RewardPointsSettings
                null, // PaymentSettings
                new ShippingSettings { ShipToSameAddress = true },
                null  // AddressSettings
            );

            // Act
            var result = await controller.SelectBillingAddress(addressId, true);

            // Assert
            customer.ShippingAddress.Should().Be(customer.BillingAddress);
            customerService.Verify(x => x.UpdateShippingAddress(It.IsAny<Address>()), Times.Once);
            genericAttributeService.Verify(x => x.SaveAttribute(It.IsAny<Customer>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()), Times.Exactly(4));
            result.Should().BeOfType<RedirectToRouteResult>();
            ((RedirectToRouteResult)result).RouteName.Should().Be("CheckoutShippingMethod");
        }

        [Test]
        [AutoDomainData]
        public async Task SelectBillingAddress_ShipToSameAddressFalse_RedirectsToShippingAddress(
            Mock<IWorkContext> workContext,
            Mock<IStoreContext> storeContext,
            Mock<ICustomerService> customerService,
            Mock<IGenericAttributeService> genericAttributeService,
            Mock<IShoppingCartService> shoppingCartService)
        {
            // Arrange
            var addressId = "test-address-id";
            var customer = new Customer
            {
                Addresses = new List<Address>
                {
                    new Address { Id = addressId }
                }
            };
            workContext.Setup(x => x.CurrentCustomer).Returns(customer);

            var controller = new CheckoutController(
                workContext.Object,
                storeContext.Object,
                null, // ILocalizationService
                customerService.Object,
                shoppingCartService.Object,
                genericAttributeService.Object,
                null, // IShippingService
                null, // IPickupPointService
                null, // IPaymentService
                null, // IPluginFinder
                null, // ILogger
                null, // IOrderService
                null, // IWebHelper
                null, // IAddressAttributeParser
                null, // ICustomerActivityService
                null, // IMediator
                null, // OrderSettings
                null, // RewardPointsSettings
                null, // PaymentSettings
                null, // ShippingSettings
                null  // AddressSettings
            );

            // Act
            var result = await controller.SelectBillingAddress(addressId, false);

            // Assert
            result.Should().BeOfType<RedirectToRouteResult>();
            var redirectResult = (RedirectToRouteResult)result;
            redirectResult.RouteName.Should().Be("CheckoutShippingAddress");
        }
    }
}