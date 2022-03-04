using System;
using System.Linq;
using JetBrains.Annotations;

namespace PatchKit.SecuringByTheLicenseKey.Core
{
public struct AppSecret
{
    public readonly string Value;

    public bool IsValid
    {
        get
        {
            if (GetValidationError(Value) == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public AppSecret(string value)
    {
        Value = value;
    }

    [ContractAnnotation("null => notNull")]
    public static string GetValidationError(string value)
    {
        if (value == null)
        {
            return "Application secret cannot be null.";
        }

        if (string.IsNullOrEmpty(value))
        {
            return "Application secret cannot be empty.";
        }

        if (!value.All(c => char.IsLetterOrDigit(c)))
        {
            return
                "Application secret cannot have other characters than letters and digits.";
        }

        return null;
    }
}
}