using Flow.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Flow.Extensions
{
    public static class ValidationErrorsExtensions
    {
        public static void Actualize<TValidationError>(
            this List<TValidationError> validationErrors,
            Func<TValidationError, bool> searchRule,
            Action<TValidationError> onAdd,
            Action<TValidationError> onUpdate = null)
            where TValidationError : ValidationError, new()
        {
            var existedValidationError = validationErrors.FirstOrDefault(searchRule);
            if (existedValidationError != null && onUpdate != null)
                onUpdate(existedValidationError);
            else if (existedValidationError == null && onAdd != null)
            {
                var validationError = new TValidationError();
                onAdd(validationError);
                validationErrors.Add(validationError);
            }
        }

        public static void Remove<TValidationError>(
            this List<TValidationError> validationErrors,
            Func<TValidationError, bool> searchRule)
            where TValidationError : ValidationError
        {
            var existedValidationError = validationErrors.FirstOrDefault(searchRule);
            if (existedValidationError != null)
                validationErrors.Remove(existedValidationError);
        }
    }
}
