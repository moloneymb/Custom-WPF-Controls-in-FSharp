namespace FSharp.Chapter18

open System
open System.Windows
open System.Windows.Automation
open System.Windows.Automation.Peers
open System.Windows.Automation.Provider
open System.Windows.Controls
open System.Windows.Controls.Primitives
open System.Windows.Media

type RangeSelectorAutomationPeer(owner:RangeSelector) = 
    inherit FrameworkElementAutomationPeer(owner)
    
    let mutable currentValue = ""
    
    override this.GetPattern(patternInterface) =
        match (patternInterface) with
        | PatternInterface.Value -> this :> obj
        | _ -> base.GetPattern(patternInterface)
        
    member this.OwningRangeSelector = this.Owner :?> RangeSelector
    
    member private this.FormatValue(start:double, _end:double) = String.Format("{0:F2},{1:F2}", start, _end)
    
    member this.RaiseRangeChangedEvent(start:double, _end:double) =
        let newValue = this.FormatValue(start,_end)
        this.RaisePropertyChangedEvent(ValuePatternIdentifiers.ValueProperty, currentValue, newValue)
        currentValue <- newValue
    
    interface IValueProvider with   
        member this.SetValue(value:string) =
            let range = value.Split([|','|])
            if (range.Length = 2)
                then
                    let mutable start = 0.
                    let mutable _end = 0.
                    // set the range only if both values parse correctly
                    if (Double.TryParse(range.[0], &start) &&
                        Double.TryParse(range.[1], &_end))
                        then
                           this.OwningRangeSelector.RangeStart <- start
                           this.OwningRangeSelector.RangeEnd <- _end

        member this.Value with get() = this.FormatValue(this.OwningRangeSelector.RangeStart, this.OwningRangeSelector.RangeEnd)
        
        member this.IsReadOnly = false
    


and 
    [<TemplatePart(Name = "PART_Start", Type = typeof<Thumb>)>]
    [<TemplatePart(Name = "PART_End", Type = typeof<Thumb>)>]
    [<TemplatePart(Name = "PART_Track", Type = typeof<Canvas>)>]
    RangeSelector() as this =
    inherit Control()
    
    let mutable startThumb = None   // Thumb
    let mutable endThumb = None     // Thumb
    let mutable track = None        // Canvas
    
    let PARTID_StartThumb = "PART_Start"
    let PARTID_EndThumb = "PART_End"
    let PARTID_Track = "PART_Track"

    
    static let minimumProperty = 
        DependencyProperty.Register("Minimum", typeof<double>, typeof<RangeSelector>, 
            new PropertyMetadata(0.0, PropertyChangedCallback(RangeSelector.onMinimumChanged), CoerceValueCallback(RangeSelector.coerceMinimum)))
    static let maximumProperty = 
        DependencyProperty.Register("Maximum", typeof<double>, typeof<RangeSelector>, 
            new PropertyMetadata(0.0, PropertyChangedCallback(RangeSelector.onMaximumChanged), CoerceValueCallback(RangeSelector.coerceMinimum)))
    static let rangeStartProperty = 
        DependencyProperty.Register("RangeStart", typeof<double>, typeof<RangeSelector>, 
            new PropertyMetadata(0.0, PropertyChangedCallback(RangeSelector.onRangeStartChanged), CoerceValueCallback(RangeSelector.coerceRangeStart)))
    static let rangeEndProperty = 
        DependencyProperty.Register("RangeEnd", typeof<double>, typeof<RangeSelector>, 
            new PropertyMetadata(0.0, PropertyChangedCallback(RangeSelector.onRangeEndChanged), CoerceValueCallback(RangeSelector.coerceRangeEnd)))
    static let computedStartOffsetProperty = 
        DependencyProperty.Register("ComputedStartOffset", typeof<double>, typeof<RangeSelector>, new PropertyMetadata(0.0))
    static let computedEndOffsetProperty = 
        DependencyProperty.Register("ComputedEndOffset", typeof<double>, typeof<RangeSelector>, new PropertyMetadata(0.0))
    static let computedRangeWidthProperty = 
        DependencyProperty.Register("ComputedRangeWidth", typeof<double>, typeof<RangeSelector>)
    
    static let rangeChangedEvent = EventManager.RegisterRoutedEvent("RangeChanged", RoutingStrategy.Bubble, typeof<Event<RangeChangedEventArgs>>, typeof<RangeSelector>)
    
    static do
        Control.DefaultStyleKeyProperty.OverrideMetadata(typeof<RangeSelector>, FrameworkPropertyMetadata(typeof<RangeSelector>))
        Control.FocusVisualStyleProperty.OverrideMetadata(typeof<RangeSelector>, FrameworkPropertyMetadata(true))
        
     do 
         this.Loaded.AddHandler(RoutedEventHandler(fun _ _ -> 
            if (startThumb <> None) then this.ComputedStartOffset <- this.calcX(this.RangeStart, this.StartThumb)
            if (endThumb <> None) then this.ComputedEndOffset <- this.calcX(this.RangeEnd, this.EndThumb)
            this.ComputedRangeWidth <- this.ComputedEndOffset - this.ComputedStartOffset 
             ))
        
    
    member this.StartThumb 
        with get() = match startThumb with | Some(v) -> v | _ -> failwith "intialization failure"
        and set(x:Thumb) = startThumb <- Some(x)
        
    member this.EndThumb 
        with get() = match endThumb with | Some(v) -> v | _ -> failwith "intialization failure"
        and set(x:Thumb) = endThumb <- Some(x)
        
    member this.Track 
        with get() = match track with | Some(v) -> v | _ -> failwith "intialization failure"
        and set(x:Canvas) = track <- Some(x)
    
    member this.Minimum with get() = this.GetValue(minimumProperty) :?> double and set(x:double) = this.SetValue(minimumProperty,x) 
    member this.Maximum with get() = this.GetValue(maximumProperty) :?> double and set(x:double) = this.SetValue(maximumProperty,x) 
    
    member this.RangeStart with get() = this.GetValue(rangeStartProperty) :?> double and set(x:double) = this.SetValue(rangeStartProperty,x)
    member this.RangeEnd with get() = this.GetValue(rangeEndProperty) :?> double and set(x:double) = this.SetValue(rangeEndProperty,x)
    
    //static member ComputedStartOffsetProperty with get() = computedStartOffsetProperty
    
    member this.ComputedStartOffset with get() = this.GetValue(computedStartOffsetProperty) :?> double and set(x:double) = this.SetValue(computedStartOffsetProperty,x)
    member this.ComputedEndOffset with get() = this.GetValue(computedEndOffsetProperty) :?> double and set(x:double) = this.SetValue(computedEndOffsetProperty,x)
    member this.ComputedRangeWidth with get() = this.GetValue(computedRangeWidthProperty) :?> double and set(x:double) = this.SetValue(computedRangeWidthProperty,x)
    
    static member RangeChangedEvent with get() = rangeChangedEvent
    
    static member private onMinimumChanged  (d:DependencyObject) (basevalue:obj) =
        d.CoerceValue(rangeStartProperty)
        d.CoerceValue(rangeEndProperty)
        d.CoerceValue(maximumProperty)
        
        let selector = d :?> RangeSelector
        if (selector.IsLoaded) then
            selector.ComputedStartOffset <- selector.calcX(selector.RangeStart, selector.StartThumb)
            selector.ComputedEndOffset <- selector.calcX(selector.RangeEnd, selector.EndThumb)
            selector.ComputedRangeWidth <- selector.ComputedEndOffset - selector.ComputedStartOffset
        else ()
    
    static member private onMaximumChanged  (d:DependencyObject) (basevalue:obj) =
        d.CoerceValue(rangeStartProperty)
        d.CoerceValue(rangeEndProperty)
        
        let selector = d :?> RangeSelector
        if (selector.IsLoaded) then
            selector.ComputedStartOffset <- selector.calcX(selector.RangeStart, selector.StartThumb)
            selector.ComputedEndOffset <- selector.calcX(selector.RangeEnd, selector.EndThumb)
            selector.ComputedRangeWidth <- selector.ComputedEndOffset - selector.ComputedStartOffset
        else ()
    
    static member private coerceMinimum (d:DependencyObject) (basevalue:obj) =
        let newValue = basevalue :?> double
        let selector = d :?> RangeSelector
        if (newValue > selector.RangeStart) then selector.RangeStart :> obj else basevalue
    
    static member private coerceMaximum (d:DependencyObject) (basevalue:obj) =
        let newValue = basevalue :?> double
        let selector = d :?> RangeSelector
        if (newValue < selector.RangeEnd) then selector.RangeEnd :> obj else basevalue
    
    static  member private onRangeStartChanged (d:DependencyObject) (e:DependencyPropertyChangedEventArgs) =
        let selector = d :?> RangeSelector
        d.CoerceValue(minimumProperty)
        d.CoerceValue(rangeEndProperty)
        
        if (selector.IsLoaded) 
            then 
                selector.ComputedStartOffset <- selector.calcX((e.NewValue :?> double), selector.StartThumb)
                selector.ComputedRangeWidth <- selector.ComputedEndOffset - selector.ComputedStartOffset
                
                selector.raiseRangeChangedEvent((e.NewValue :?> double), selector.RangeEnd)
            else ()
 
    static member private onRangeEndChanged (d:DependencyObject) (e:DependencyPropertyChangedEventArgs) =
        let selector = d :?> RangeSelector
        d.CoerceValue(rangeStartProperty)
        d.CoerceValue(maximumProperty)
        
        if (selector.IsLoaded) 
            then 
                selector.ComputedEndOffset <- selector.calcX((e.NewValue :?> double), selector.EndThumb)
                selector.ComputedRangeWidth <- selector.ComputedEndOffset - selector.ComputedStartOffset
                
                selector.raiseRangeChangedEvent(selector.RangeStart, (e.NewValue :?> double))
            else ()
    
    member private this.raiseRangeChangedEvent(start:double, _end:double) =
        if (AutomationPeer.ListenerExists(AutomationEvents.PropertyChanged))
            then 
                match UIElementAutomationPeer.FromElement(this) with
                | :? RangeSelectorAutomationPeer as peer -> peer.RaiseRangeChangedEvent(start, _end)
                | _ -> ()

    
    static member private coerceRangeStart (d:DependencyObject) (basevalue:obj) =
        let selector = d :?> RangeSelector
        let newValue = basevalue :?> double
        if (newValue < selector.RangeStart)
            then selector.Minimum :> obj
            else if (newValue > selector.RangeEnd)
                then selector.Maximum :> obj
                else basevalue
    
    
    static member private coerceRangeEnd (d:DependencyObject) (basevalue:obj) =
        let selector = d :?> RangeSelector
        let newValue = basevalue :?> double
        if (newValue < selector.RangeStart)
            then selector.RangeStart :> obj
            else if (newValue > selector.Maximum)
                then selector.Maximum :> obj
                else basevalue
                
    member private this.setupThumb (thumb:Thumb) (getOffset:Unit->double) (updaterAction:double->Unit) =
        thumb.DragDelta.AddHandler(DragDeltaEventHandler(fun sender args -> 
            let mutable change = getOffset() + args.HorizontalChange
            if (change < 0.) then change <- 0.
            else if (change > this.Track.ActualWidth - thumb.ActualWidth) then change <- this.Track.ActualWidth - thumb.ActualWidth
            
            let ratio = change / (this.Track.ActualWidth - thumb.ActualWidth)
            let newValue = this.Minimum + ratio * (this.Maximum - this.Minimum)
            updaterAction(newValue)
            ))
    
    override this.OnApplyTemplate() =
        track <- Some(this.GetTemplateChild(PARTID_Track) :?> Canvas)
        startThumb <- Some(this.GetTemplateChild(PARTID_StartThumb) :?> Thumb)
        endThumb <- Some(this.GetTemplateChild(PARTID_EndThumb) :?> Thumb)
        
        if (track = None || startThumb = None || endThumb = None) then 
            this.setupThumb this.StartThumb  (fun () -> this.ComputedStartOffset)  (fun x -> this.RangeStart <- x)
            this.setupThumb this.EndThumb (fun () -> this.ComputedEndOffset) (fun x -> this.RangeEnd <- x)
                   
    
    member this.calcX(value:double, thumb:Thumb) =
        let mutable denom = this.Maximum - this.Minimum
        if (denom <= 0.) then denom <- 1.
        
        let newValue = ((value - this.Minimum) / denom) * this.Track.ActualWidth
        Math.Min(newValue, this.Track.ActualWidth - thumb.ActualWidth)
    
    override this.OnCreateAutomationPeer() = RangeSelectorAutomationPeer(this) :> AutomationPeer
   


and RangeChangedEventArgs(start:double, _end:double) as this =
    inherit RoutedEventArgs()
    
    let mutable rangeStart = start
    let mutable rangeEnd = _end
    
    do
        this.RoutedEvent <- RangeSelector.RangeChangedEvent
    
    member this.RangeStart with get() = rangeStart
    member this.RangeEnd with get() = rangeEnd
    