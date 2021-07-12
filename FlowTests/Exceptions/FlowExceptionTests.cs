using AutoFixture;
using Flow.Contracts;
using Flow.Contracts.Enums;
using Flow.Exceptions;
using FluentAssertions;
using System.Linq;
using Xunit;

namespace FlowTests.Exceptions
{
    public class FlowExceptionTests
    {
        private readonly Fixture fixture;

        public FlowExceptionTests()
        {
            fixture = new Fixture();
        }

        [Fact]
        public void Constructor_ShouldCreateFullMessage()
        {
            // Assert
            var exceptionContext = fixture.Create<FlowExceptionContext>();

            // Act
            var error = new FlowException(exceptionContext);

            // Arrange
            var expected = $"{exceptionContext.Message}"
                + $"\r\n    - FlowNodeIndex: {exceptionContext.FlowNodeIndex}."
                + $"\r\n    - FlowNodeType: {exceptionContext.FlowNodeType}."
                + $"\r\n    - Found {exceptionContext.FlowRoutes.Length} possible flow routes:";
            foreach (var route in exceptionContext.FlowRoutes)
                expected += $"\r\n        {string.Join(" -> ", route)}";
            error.Message.Should().Be(expected);
        }

        [Fact]
        public void Constructor_ShouldCreateMessage()
        {
            // Assert
            var exceptionContext = new FlowExceptionContext { Message = fixture.Create<string>() };

            // Act
            var error = new FlowException(exceptionContext);

            // Arrange
            error.Message.Should().Be(exceptionContext.Message);
        }

        [Fact]
        public void Constructor_ShouldCreateMessageWithFlowIndex()
        {
            // Assert
            var exceptionContext = new FlowExceptionContext { FlowNodeIndex = fixture.Create<string>() };

            // Act
            var error = new FlowException(exceptionContext);

            // Arrange
            var expected = $"\r\n    - FlowNodeIndex: {exceptionContext.FlowNodeIndex}.";
            error.Message.Should().Be(expected);
        }

        [Fact]
        public void Constructor_ShouldCreateMessageWithFlowType()
        {
            // Assert
            var exceptionContext = new FlowExceptionContext { FlowNodeType = fixture.Create<FlowNodeType>() };

            // Act
            var error = new FlowException(exceptionContext);

            // Arrange
            var expected = $"\r\n    - FlowNodeType: {exceptionContext.FlowNodeType}.";
            error.Message.Should().Be(expected);
        }

        [Fact]
        public void Constructor_ShouldCreateMessageWithFlowRoutes()
        {
            // Assert
            var exceptionContext = new FlowExceptionContext { FlowRoutes = fixture.CreateMany<string[]>(1).ToArray() };

            // Act
            var error = new FlowException(exceptionContext);

            // Arrange
            var expected = $"\r\n    - Found {exceptionContext.FlowRoutes.Length} possible flow routes:"
                + $"\r\n        {string.Join(" -> ", exceptionContext.FlowRoutes[0])}";
            error.Message.Should().Be(expected);
        }
    }
}
