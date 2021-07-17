using AutoFixture;
using Flow.Contracts;
using Flow.Exceptions;
using Flow.Extensions;
using FluentAssertions;
using System;
using System.Linq;
using Xunit;

namespace FlowTests.Extensions
{
    public class FlowMapValidationErrorExtensionsTests
    {
        private readonly Fixture fixture;

        public FlowMapValidationErrorExtensionsTests()
        {
            fixture = new Fixture();
        }

        [Fact]
        public void ToFlowException_ShoulUseFlowMapError()
        {
            // Arrange
            var flowMapError = fixture.Create<FlowMapValidationError>();
            flowMapError.FlowNodeErrors = Array.Empty<FlowNodeValidationError>();

            // Act
            Action act = () => throw flowMapError.ToFlowException();


            // Assert
            var error = act.Should().Throw<FlowException>();
            var expected = $"{flowMapError.Message}"
                + $"\r\n    - FlowNodeIndex: {flowMapError.FlowNodeIndex}.";
            error.WithMessage(expected);
        }

        [Fact]
        public void ToFlowException_ShoulUseFlowNodeError()
        {
            // Arrange
            var flowMapError = fixture.Create<FlowMapValidationError>();

            // Act
            Action act = () => throw flowMapError.ToFlowException();

            // Assert
            var error = act.Should().Throw<FlowException>();
            var expected = $"{flowMapError.Message}. {flowMapError.FlowNodeErrors[0].Message}."
                + $"\r\n    - FlowNodeIndex: {flowMapError.FlowNodeIndex}."
                + $"\r\n    - Found {flowMapError.FlowNodeErrors.Length} possible flow routes:";
            foreach (var flowRoute in flowMapError.FlowNodeErrors.Select(x => x.FlowRoute).ToArray())
                expected += $"\r\n        {string.Join(" -> ", flowRoute)}";
            error.WithMessage(expected);
        }
    }
}
