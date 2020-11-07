using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.App.Components.Forms
{
    public class AdvancedFormComponent<T> : ComponentBase where T : new()
    {
        protected T Model { get; set; } = new T();
        protected EditContext EditContext { get; private set; }
        protected Boolean FormIsValid { get; private set; } = false;

        protected void ResetEditContext()
        {
            EditContext.OnFieldChanged -= HandleFieldChanged;

            EditContext = new EditContext(Model);
            EditContext.OnFieldChanged += HandleFieldChanged;
            FormIsValid = EditContext.Validate();
        }

        private void HandleFieldChanged(object sender, FieldChangedEventArgs e)
        {
            FormIsValid = EditContext.Validate();
            StateHasChanged();
        }

        protected override void OnInitialized()
        {
            EditContext = new EditContext(Model);
            EditContext.OnFieldChanged += HandleFieldChanged;

            base.OnInitialized();
            FormIsValid = EditContext.Validate();
        }

        public virtual void Dispose()
        {
            EditContext.OnFieldChanged -= HandleFieldChanged;
        }

    }
}

