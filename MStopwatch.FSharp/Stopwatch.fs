namespace MStopwatch.FSharp

open Prism.Mvvm
open System
open System.Collections.Generic
open System.Collections.ObjectModel
open System.Linq
open System.Reactive.Concurrency
open System.Reactive.Linq
open System.Text
open System.Threading.Tasks
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Quotations.DerivedPatterns
open System.ComponentModel
open Reactive.Bindings

type Stopwatch(timerScheduler : IScheduler) = 
    inherit BindableBase()
    let mutable mode = StopwatchMode.Init
    let mutable startTime = DateTime()
    let mutable now = DateTime()
    let itemsSource = new ObservableCollection<LapTime>()
    let mutable timerSubscription : option<IDisposable> = None
    let mutable items = new ReadOnlyObservableCollection<LapTime>(itemsSource)
    
    do 
        match timerScheduler with
        | null -> raise (ArgumentException("TimerScheduler"))
        | _ -> ()
    
    new() = Stopwatch(Scheduler.Default)
    
    member x.Set<'T>((field : 'T byref), value, propertyExpression) = 
        if (System.Object.Equals(field, value)) then false
        else 
            field <- value
            match propertyExpression with
            | PropertyGet(_, info, _) -> x.OnPropertyChanged(PropertyChangedEventArgs(info.Name))
            | _ -> ()
            true
    
    member x.Mode 
        with get () = mode
        and set (value) = x.Set(&mode, value, <@ x.Mode @>) |> ignore
    
    member x.StartTime 
        with get () = startTime.ToLocalTime()
        and set (value) = 
            match x.Set(&startTime, value, <@ x.StartTime @>) with
            | true -> x.RaisePropertyChanged("NowSpan")
            | false -> ()
    
    member x.Now 
        with get () = now.ToLocalTime()
        and set (value) = 
            match x.Set(&now, value, <@ x.Now @>) with
            | true -> x.RaisePropertyChanged("NowSpan")
            | false -> ()
    
    member x.NowSpan = x.Now - x.StartTime
    member x.Items = items
    member x.MaxLapTime = items.Max(fun u -> u.Span)
    member x.MinLapTime = items.Min(fun u -> u.Span)
    
    member x.Start() = 
        match x.Mode with
        | StopwatchMode.Start -> raise (InvalidOperationException())
        | _ -> 
            x.StartTime <- timerScheduler.Now.DateTime
            itemsSource.Clear()
            timerSubscription <- Some
                                     (Observable.Interval(TimeSpan.FromMilliseconds(10.), timerScheduler)
                                                .Subscribe(fun _ -> x.Now <- timerScheduler.Now.DateTime))
            x.Mode <- StopwatchMode.Start
    
    member x.Stop() = 
        match x.Mode with
        | StopwatchMode.Stop -> raise (InvalidOperationException())
        | _ -> 
            match timerSubscription with
            | Some s -> s.Dispose()
            | None -> ()
            timerSubscription <- None
            x.Lap()
            x.Mode <- StopwatchMode.Stop
    
    member x.Reset() = 
        let nowDateTime = timerScheduler.Now.DateTime
        x.StartTime <- nowDateTime
        x.Now <- nowDateTime
        itemsSource.Clear()
        x.Mode <- StopwatchMode.Init
    
    member x.Lap() = 
        let prevLap = 
            match x.Items.LastOrDefault() with
            | t when x.Items.Any() -> t.Time
            | _ -> x.StartTime
        { Time = x.Now
          Span = timerScheduler.Now.DateTime.ToLocalTime() - prevLap }
        |> itemsSource.Add
