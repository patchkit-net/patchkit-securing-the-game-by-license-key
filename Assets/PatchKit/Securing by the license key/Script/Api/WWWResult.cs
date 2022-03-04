using System.Collections.Generic;

namespace PatchKit.SecuringByTheLicenseKey.Api
{
public class WWWResult
{
    public bool IsDone { get; set; }

    public string Text { get; set; }

    public Dictionary<string, string> ResponseHeaders { get; set; }
}
}