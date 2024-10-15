using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.NUnit3;
using Grand.Core;
using Moq;

namespace HackTests
 {
    internal static class AutoDomainDataUtils
    {
        public static IFixture BuildFixtureWithCustomisations()
        {
            return new Fixture()
                .Customize(new AutoMoqCustomization { ConfigureMembers = true })
                .Customize(new GrandWebCustomization());
        }
    }

    public class AutoDomainDataAttribute : AutoDataAttribute
    {
        public AutoDomainDataAttribute() : base(() => AutoDomainDataUtils.BuildFixtureWithCustomisations())
        {
        }
    }

    public class InlineAutoDomainDataAttribute : InlineAutoDataAttribute
    {
        public InlineAutoDomainDataAttribute(params object[] arguments) 
            : base(() => AutoDomainDataUtils.BuildFixtureWithCustomisations(), arguments)
        {
        }
    }

    public class GrandWebCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            // Add any specific customizations for your Grand.Web project here
            // For example:
            fixture.Customize<Mock<IWorkContext>>(composer => composer
                .Do(mock => mock.Setup(m => m.CurrentCustomer).Returns(fixture.Create<Grand.Domain.Customers.Customer>())));

            fixture.Customize<Mock<IStoreContext>>(composer => composer
                .Do(mock => mock.Setup(m => m.CurrentStore).Returns(fixture.Create<Grand.Domain.Stores.Store>())));

            // Add more customizations as needed for other services or contexts
        }
    }
}
