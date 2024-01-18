module ECommerce.Reactor.Metrics

let baseName stat = "Ecommerce_reactor_" + stat
let baseDesc desc = "Ecommerce: Reactor " + desc

module private Counter =

    let private make (config : Prometheus.CounterConfiguration) name desc =
        let ctr = Prometheus.Metrics.CreateCounter(name, desc, config)
        fun tagValues (c : float) -> ctr.WithLabels(tagValues).Inc(c)

    let create (tagNames, tagValues) stat desc =
        let config = Prometheus.CounterConfiguration(LabelNames = tagNames)
        make config (baseName stat) (baseDesc desc) tagValues

let observeOutcomeStatus s =    Counter.create  ([| "status" |],[| s |])    "outcome_total"     "Outcome"

[<RequireQualifiedAccess>]
type Outcome =
    /// Handler processed the span, with counts of used vs unused known event types
    | Ok of used : int * unused : int
    /// Handler processed the span, but idempotency checks resulted in no writes being applied; includes count of decoded events
    | Skipped of count : int
    /// Handler determined the events were not relevant to its duties and performed no actions
    /// e.g. wrong category, events that dont imply a state change
    | NotApplicable of count : int

let observeReactorOutcome = function
    | Outcome.Ok (used, _)->            observeOutcomeStatus "ok" (float used)
    | Outcome.Skipped c ->              observeOutcomeStatus "ignored" (float c)
    | Outcome.NotApplicable c ->        observeOutcomeStatus "handled" (float c)
