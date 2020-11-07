using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Host
{
    public static class IActionResultExtentions
    {
        public static T EnsureOkObjectResult<T>(this IActionResult uncastedResult, Boolean shouldHaveValue)
        {
            Assert.IsAssignableFrom<OkObjectResult>(uncastedResult);

            OkObjectResult actionResult = (OkObjectResult)uncastedResult;

            if (shouldHaveValue == true)
            {
                Assert.NotNull(actionResult.Value);
            }
            Assert.IsAssignableFrom<T>(actionResult.Value);

            T value = (T)actionResult.Value;
            return value;
        }

        public static void EnsureNoContentResult(this IActionResult uncastedResult)
        {
            Assert.IsAssignableFrom<NoContentResult>(uncastedResult);
        }

        public static T EnsureBadRequestObjectResult<T>(this IActionResult uncastedResult)
        {
            Assert.IsAssignableFrom<BadRequestObjectResult>(uncastedResult);
            BadRequestObjectResult castedResult = (BadRequestObjectResult)uncastedResult;

            Assert.NotNull(castedResult.Value);
            Assert.IsAssignableFrom<T>(castedResult.Value);

            T value = (T)castedResult.Value;

            return value;
        }

        public static void EnsureBadRequestObjectResult(this IActionResult uncastedResult, String expectedErrorMsg)
        {
            String actual = EnsureBadRequestObjectResult<String>(uncastedResult);
            Assert.Equal(expectedErrorMsg, actual);
        }

        public static void EnsureBadRequestObjectResultForError(this IActionResult actionResult,
          String expectedErrorKey, String expectedErrorMsg)
        {
            SerializableError content = actionResult.EnsureBadRequestObjectResult<SerializableError>();
            Assert.True(content.ContainsKey(expectedErrorKey));

            Object rawElement = content[expectedErrorKey];
            Assert.IsAssignableFrom<IEnumerable<String>>(rawElement);

            IEnumerable<String> errorElements = (IEnumerable<String>)rawElement;
            Assert.Single(errorElements);
            Assert.Equal(expectedErrorMsg, errorElements.First());
        }

        public static void EnsureNotFoundObjectResult<T>(this IActionResult uncastedResult, T expectedValue)
        {
            Assert.IsAssignableFrom<NotFoundObjectResult>(uncastedResult);
            NotFoundObjectResult castedResult = (NotFoundObjectResult)uncastedResult;

            Assert.NotNull(castedResult.Value);
            Assert.IsAssignableFrom<T>(castedResult.Value);

            T value = (T)castedResult.Value;

            Assert.Equal(expectedValue, value);
        }
    }
}
