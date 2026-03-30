using System;
using UnityEngine;

namespace Rusleo.Utils.Runtime.Attributes.Validation
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class RequiredAttribute : PropertyAttribute
    {
        public RequiredAttribute(string message = null)
        {
            Message = string.IsNullOrWhiteSpace(message)
                ? "Required reference is missing."
                : message;
        }

        public string Message { get; }
    }
}