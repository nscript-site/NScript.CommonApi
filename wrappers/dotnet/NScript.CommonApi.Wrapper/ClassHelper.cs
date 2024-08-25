using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NScript.CommonApi;

public static class ClassHelper
{
    public static string ToJsonString(this BaseResult? result)
    {
        if (result == null) return $"Empty Object";
        else return JsonSerializer.Serialize(result, result.GetType());
    }

    public static void WriteLine(this BaseResult? result)
    {
        Console.WriteLine(result.ToJsonString());
    }

    public static void WriteLine(this BaseResult? result, string prefix)
    {
        Console.WriteLine($"[{prefix}]:");
        Console.WriteLine(result.ToJsonString());
    }
}
