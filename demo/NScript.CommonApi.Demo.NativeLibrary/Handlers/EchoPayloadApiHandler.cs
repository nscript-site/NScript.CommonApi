using NScript.CommonApi.Demo.NativeLibrary.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace NScript.CommonApi.Demo.NativeLibrary.Handlers;

public class EchoPayloadApiHandler : TypedPayloadApiHandler<EchoInput, EchoOutput>
{
    protected override EchoOutput Handle(EchoInput? input, Payload payload)
    {
        if (input == null) return new EchoOutput();
        EchoOutput output = new EchoOutput();
        var msg = input.message ?? String.Empty;
        output.echo = $"{msg}, payload: {payload.Length} bytes";
        var bytes = payload.ToArray();
        for (int i = 0; i < bytes.Length; i++)
        {
            output.sum += bytes[i];
        }
        return output;
    }

    protected override (JsonTypeInfo<EchoInput>, JsonTypeInfo<EchoOutput>) GetTypeInfos()
    {
        return (EchoSerializeOnlyContext.Default.EchoInput, EchoSerializeOnlyContext.Default.EchoOutput);
    }
}