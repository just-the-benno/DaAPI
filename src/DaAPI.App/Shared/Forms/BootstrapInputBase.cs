using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.App.Shared.Forms
{
    public abstract class BootstrapInputBase<T> : CssAwareInputBase<T>
    {
        [CascadingParameter(Name = "DisableFormElements")]
        protected Boolean DisableFormElements { get; set; } = false;

        public override string InvalidCssClass { get; set; } = "is-invalid";
        public override string ValidCssClass { get; set; } = "is-valid";
        public override Boolean AddCssClassesOnlyWhenModified { get; set; } = true;

        protected virtual void AdddId(RenderTreeBuilder builder, Int32 seq)
        {
            if(AdditionalAttributes == null || AdditionalAttributes.ContainsKey("id") == false)
            {
                builder.AddAttribute(seq, "id", base.FieldIdentifier.FieldName);
            }
        }

        protected BootstrapInputBase()
        {
        }

     
    }
}
