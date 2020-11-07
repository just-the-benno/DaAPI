using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DaAPI.Shared.Validation
{
    public class IsUniqueInCollection : ValidationAttribute
    {
        private readonly string _otherItemsPropertyName;

        public IsUniqueInCollection(String otherItemsPropertyName)
        {
            this._otherItemsPropertyName = otherItemsPropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            Boolean isValid = true;

            var ohterItemsProperty = validationContext.ObjectType.GetProperty(_otherItemsPropertyName);
            var currentProperty = validationContext.ObjectType.GetProperty(validationContext.MemberName);
            var equalsMethod = validationContext.ObjectType.GetMethod("Equals", new[] { value.GetType() });

            if (ohterItemsProperty != null)
            {
                IEnumerable otherItems = (IEnumerable)ohterItemsProperty.GetValue(validationContext.ObjectInstance);
                foreach (var item in otherItems)
                {
                    if(Object.ReferenceEquals(item,validationContext.ObjectInstance) == true)
                    {
                        continue;
                    }

                    var otherValue = currentProperty.GetValue(item);

                    var equalsResult =  (Boolean)equalsMethod.Invoke(value, new[] { otherValue });
                   
                    if (equalsResult == true)
                    {
                        isValid = false;
                        break;
                    }
                }
            }

            if(isValid == true)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage, new[] { validationContext.MemberName });
        }
    }
}
