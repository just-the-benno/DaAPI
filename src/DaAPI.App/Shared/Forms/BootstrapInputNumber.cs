using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.App.Shared.Forms
{
    public class BootstrapInputNumber<T> :  BootstrapInputBase<T>
    {
        private static readonly string _stepAttributeValue; // Null by default, so only allows whole numbers as per HTML spec

        static BootstrapInputNumber()
        {
            // Unwrap Nullable<T>, because InputBase already deals with the Nullable aspect
            // of it for us. We will only get asked to parse the T for nonempty inputs.
            var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            if (targetType == typeof(int) ||
                targetType == typeof(long) ||
                targetType == typeof(float) ||
                targetType == typeof(double) ||
                targetType == typeof(byte) ||
                targetType == typeof(decimal))
            {
                _stepAttributeValue = "any";
            }
            else
            {
                throw new InvalidOperationException($"The type '{targetType}' is not a supported numeric type.");
            }
        }
        /// <summary>
        /// Gets or sets the error message used when displaying an a parsing error.
        /// </summary>
        [Parameter] public string ParsingErrorMessage { get; set; } = "The {0} field must be a number.";

        /// <inheritdoc />
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            Int32 elementCounter = 0;

            builder.OpenElement(elementCounter++, "input");
            if(AdditionalAttributes.ContainsKey("step") == false)
            {
                builder.AddAttribute(elementCounter++, "step", _stepAttributeValue);
            }
            
            builder.AddMultipleAttributes(elementCounter++, AdditionalAttributes);
            if (AdditionalAttributes.ContainsKey("type") == false)
            {
                builder.AddAttribute(elementCounter++, "type", "number");
            }
          
            builder.AddAttribute(elementCounter++, "class", CssClass);
            builder.AddAttribute(elementCounter++, "value", BindConverter.FormatValue(CurrentValueAsString));
            builder.AddAttribute(elementCounter++, "oninput", EventCallback.Factory.CreateBinder<string>(this, __value => CurrentValueAsString = __value, CurrentValueAsString));
            builder.AddAttribute(elementCounter++, "disabled", base.DisableFormElements);
            AdddId(builder, elementCounter++);

            builder.CloseElement();
        }

        /// <inheritdoc />
        protected override bool TryParseValueFromString(string value, out T result, out string validationErrorMessage)
        {
            if (BindConverter.TryConvertTo<T>(value, CultureInfo.InvariantCulture, out result))
            {
                validationErrorMessage = null;
                return true;
            }
            else
            {
                validationErrorMessage = string.Format(ParsingErrorMessage, FieldIdentifier.FieldName);
                return false;
            }
        }

        /// <summary>
        /// Formats the value as a string. Derived classes can override this to determine the formating used for <c>CurrentValueAsString</c>.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <returns>A string representation of the value.</returns>
        protected override string FormatValueAsString(T value)
        {
            // Avoiding a cast to IFormattable to avoid boxing.
            return value switch
            {
                null => null,
                int @int => BindConverter.FormatValue(@int, CultureInfo.InvariantCulture),
                long @long => BindConverter.FormatValue(@long, CultureInfo.InvariantCulture),
                float @float => BindConverter.FormatValue(@float, CultureInfo.InvariantCulture),
                double @double => BindConverter.FormatValue(@double, CultureInfo.InvariantCulture),
                decimal @decimal => BindConverter.FormatValue(@decimal, CultureInfo.InvariantCulture),
                _ => throw new InvalidOperationException($"Unsupported type {value.GetType()}"),
            };
        }
    }
}