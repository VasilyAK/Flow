using Flow.Extensions;
using FlowTests.Fakes;
using FluentAssertions;
using Xunit;

namespace FlowTests.Extensions
{
    public class EnumExtensionsTests
    {
        [Fact]
        public void FullName_ShouldReturnNameSpaceAndName()
        {
            // Act
            var fullName = FakeNodeIndex.Index1.FullName();

            // Assert
            fullName.Should().Be("FlowTests.Fakes.FakeNodeIndex.Index1");
        }
    }
}
