module Streams

open Serilog

module Codec =

    let dec<'E when 'E :> TypeShape.UnionContract.IUnionContract> : Propulsion.Sinks.Codec<'E> =
        FsCodec.SystemTextJson.Codec.Create<'E>() // options = Options.Default

    let private renderBody (x: Propulsion.Sinks.EventBody) = System.Text.Encoding.UTF8.GetString(x.Span)

    // Uses the supplied codec to decode the supplied event record (iff at LogEventLevel.Debug, failures are logged, citing `stream` and `.Data`)
    let internal tryDecode<'E> (codec: Propulsion.Sinks.Codec<'E>) (streamName: FsCodec.StreamName) event =
        match codec.Decode event with
        | ValueNone when Log.IsEnabled Serilog.Events.LogEventLevel.Debug ->
            Log.ForContext("eventData", renderBody event.Data)
                .Debug("Codec {type} Could not decode {eventType} in {stream}", codec.GetType().FullName, event.EventType, streamName)
            ValueNone
        | x -> x

let (|Decode|) codec struct (stream, events: Propulsion.Sinks.Event[]): 'E[] =
    events |> Propulsion.Internal.Array.chooseV (Codec.tryDecode codec stream)
