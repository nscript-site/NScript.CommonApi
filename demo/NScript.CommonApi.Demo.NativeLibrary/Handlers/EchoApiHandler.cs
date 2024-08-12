using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using NScript.CommonApi.Demo.NativeLibrary.Sdk;

namespace NScript.CommonApi.Demo.NativeLibrary.Handlers;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Serialization | JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(EchoInput))]
[JsonSerializable(typeof(EchoOutput))]
internal partial class EchoSerializeOnlyContext : JsonSerializerContext
{
}

public class EchoApiHandler : TypedApiHandler<EchoInput, EchoOutput>
{
    protected override EchoOutput Handle(EchoInput? input)
    {
        if (input == null) return new EchoOutput();
        EchoOutput output = new EchoOutput();
        var msg = input.message ?? String.Empty;
        output.echo = $"{msg}";
        return output;
    }

    protected override (JsonTypeInfo<EchoInput>, JsonTypeInfo<EchoOutput>) GetTypeInfos()
    {
        return (EchoSerializeOnlyContext.Default.EchoInput, EchoSerializeOnlyContext.Default.EchoOutput);
    }
}
