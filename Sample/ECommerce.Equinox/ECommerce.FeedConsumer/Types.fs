namespace ECommerce.FeedConsumer.Domain

open FSharp.UMX // see https://github.com/fsprojects/FSharp.UMX - % operator and ability to apply units of measure to Guid+strings

type [<Measure>] fcId
type FcId = string<fcId>
module FcId =
    let toString (value : FcId) : string = %value
    let parse (value : string) : FcId = let raw = value in % raw
    let (|Parse|) = parse

type [<Measure>] ticketId
type TicketId = string<ticketId>
module TicketId =
    let toString (value : TicketId) : string = %value
    let parse (value : string) : TicketId = let raw = value in % raw
    let (|Parse|) = parse
