using AutoFixture;
using Flow.Contracts;
using Flow.Extensions;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FlowTests.Extensions
{
    public class ValidationErrorsExtensionsTests
    {
        private readonly Fixture fixture;

        public ValidationErrorsExtensionsTests()
        {
            fixture = new Fixture();
        }

        #region Actualize
        [Fact]
        public void Actualize_ShouldAddNewValidationError()
        {
            // Arrange
            var validationErrors = new List<ValidationError>();
            bool searchRule(ValidationError sr) => true;
            var newMessage = fixture.Create<string>();
            Action<ValidationError> onAdd = (ve) => ve.Message = newMessage;

            // Act
            validationErrors.Actualize(searchRule, onAdd);

            // Assert
            validationErrors.Should().HaveCount(1);
            validationErrors[0].Message.Should().Be(newMessage);
        }

        [Fact]
        public void Actualize_ShouldUpdateValidationError()
        {
            // Arrange
            var existedMessage = fixture.Create<string>();
            var validationErrors = fixture.CreateMany<ValidationError>(1).ToList();
            validationErrors[0].Message = existedMessage;
            bool searchRule(ValidationError sr) => sr.Message == existedMessage;
            var newMessage = fixture.Create<string>();
            Action<ValidationError> onUpdate = (ve) => ve.Message = newMessage;

            // Act
            validationErrors.Actualize(searchRule, null, onUpdate);

            // Assert
            validationErrors.Should().HaveCount(1);
            validationErrors[0].Message.Should().Be(newMessage);
        }

        [Fact]
        public void Actualize_ShouldNotUpdateValidationError()
        {
            // Arrange
            var existedMessage = fixture.Create<string>();
            var validationErrors = fixture.CreateMany<ValidationError>(1).ToList();
            validationErrors[0].Message = existedMessage;
            bool searchRule(ValidationError sr) => sr.Message == fixture.Create<string>();
            var newMessage = fixture.Create<string>();
            Action<ValidationError> onUpdate = (ve) => ve.Message = newMessage;

            // Act
            validationErrors.Actualize(searchRule, null, onUpdate);

            // Assert
            validationErrors.Should().HaveCount(1);
            validationErrors[0].Message.Should().Be(existedMessage);
        }

        [Fact]
        public void Actualize_ShouldNotUpdateValidationError_IfNoUpdateAction()
        {
            // Arrange
            var existedMessage = fixture.Create<string>();
            var validationErrors = fixture.CreateMany<ValidationError>(1).ToList();
            validationErrors[0].Message = existedMessage;
            bool searchRule(ValidationError sr) => sr.Message == existedMessage;
            var newMessage = fixture.Create<string>();
            Action<ValidationError> onAdd = (ve) => ve.Message = newMessage;

            // Act
            validationErrors.Actualize(searchRule, onAdd);

            // Assert
            validationErrors.Should().HaveCount(1);
            validationErrors[0].Message.Should().Be(existedMessage);
        }
        #endregion

        #region Remove
        [Fact]
        public void Remove_ShouldRemoveValidationError()
        {
            // Arrange
            var existedMessage = fixture.Create<string>();
            var validationErrors = fixture.CreateMany<ValidationError>(1).ToList();
            validationErrors[0].Message = existedMessage;
            bool searchRule(ValidationError sr) => sr.Message == existedMessage;

            // Act
            validationErrors.Remove(searchRule);

            // Assert
            validationErrors.Should().HaveCount(0);
        }

        [Fact]
        public void Remove_ShouldNotRemoveValidationError()
        {
            // Arrange
            var existedMessage = fixture.Create<string>();
            var validationErrors = fixture.CreateMany<ValidationError>(1).ToList();
            validationErrors[0].Message = existedMessage;
            bool searchRule(ValidationError sr) => sr.Message == fixture.Create<string>();

            // Act
            validationErrors.Remove(searchRule);

            // Assert
            validationErrors.Should().HaveCount(1);
            validationErrors[0].Message.Should().Be(existedMessage);
        }
        #endregion
    }
}
