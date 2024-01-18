module Streams

type Event = FsCodec.ITimelineEvent<EventBody>
and EventBody = System.ReadOnlyMemory<byte>

module Event =

    let private serdes = FsCodec.NewtonsoftJson.Serdes(Newtonsoft.Json.JsonSerializerSettings())
    let userContext (e : Event) = serdes.Deserialize<Domain.Types.UserContext>(e.Meta)

module Codec =

    open FsCodec.NewtonsoftJson
    open Serilog
    type UserContext = Domain.Types.UserContext

    let dec<'E when 'E :> TypeShape.UnionContract.IUnionContract> =
        let up _ (typed: 'E) = typed
        let down (_event: 'E) = failwith "only supports deserialization"
        Codec.Create(up, down)

    let render (x : EventBody) : string = System.Text.Encoding.UTF8.GetString(x.Span)
    /// Uses the supplied codec to decode the supplied event record `x` (iff at LogEventLevel.Debug, detail fails to `log` citing the `stream` and content)
    let tryDecode (codec : FsCodec.IEventCodec<'Event, EventBody, unit>) (streamName : FsCodec.StreamName) (x : Event): ('Event * UserContext) voption =
        match codec.Decode x with
        | ValueNone ->
            if Log.IsEnabled Serilog.Events.LogEventLevel.Debug then
                Log.ForContext("event", render x.Data, true)
                    .Debug("Codec {type} Could not decode {eventType} in {stream}", codec.GetType().FullName, x.EventType, streamName)
            ValueNone
        | ValueSome d ->
            ValueSome (d, Event.userContext x)
    let decode codec struct (stream, events: Event[]) : ('e * UserContext) array =
        events |> Array.chooseV (tryDecode codec stream)
let (|Decode|) = Codec.decode
