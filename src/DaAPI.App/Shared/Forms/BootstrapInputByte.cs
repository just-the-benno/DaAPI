using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.App.Shared.Forms
{
    public class BootstrapInputByte :  BootstrapInputBase<Byte>
    {
        /// Gets or sets the error message used when displaying an a parsing error.
        /// </summary>
        [Parameter] public string ParsingErrorMessage { get; set; } = "The {0} field must be a number.";

        /// <inheritdoc />
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "input");
            builder.AddAttribute(1, "step", "1");
            builder.AddMultipleAttributes(2, AdditionalAttributes);
            builder.AddAttribute(3, "type", "number");
            builder.AddAttribute(4, "maximum", "255");
            builder.AddAttribute(5, "minimum", "0");
            builder.AddAttribute(6, "class", CssClass);
            builder.AddAttribute(7, "value", BindConverter.FormatValue(CurrentValueAsString));
            builder.AddAttribute(8, "oninput", EventCallback.Factory.CreateBinder<string>(this, __value => CurrentValueAsString = __value, CurrentValueAsString));
            builder.AddAttribute(9, "disabled", base.DisableFormElements);
            AdddId(builder, 10);

            builder.CloseElement();
        }


        /// <inheritdoc />
        protected override bool TryParseValueFromString(string value, out Byte result, out string validationErrorMessage)
        {
            if(Byte.TryParse(value,out result) == true)
            {
                validationErrorMessage = null;
                return true;
            }

            validationErrorMessage = ParsingErrorMessage;
            return false;
        }

        protected override string FormatValueAsString(Byte value) => value.ToString(); 
    }
}