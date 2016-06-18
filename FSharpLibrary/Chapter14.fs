namespace FSharp.Chapter14

open System
open System.Windows.Input
open System.Windows
open System.Windows.Controls

type TradeExecutionCommands() =
    static let sendNegotiation = RoutedCommand("SendNegotiation", typeof<TradeExecutionCommands>)
    static let startTreaming   = RoutedCommand("StartStreaming", typeof<TradeExecutionCommands>)
    static let bid             = RoutedCommand("Bid", typeof<TradeExecutionCommands>)
    static let ask             = RoutedCommand("Ask", typeof<TradeExecutionCommands>)
    static let buy             = RoutedCommand("Buy", typeof<TradeExecutionCommands>)
    
    static member SendNegotiation with get() = sendNegotiation
    static member StartStreaming with get() = startTreaming
    static member Bid with get() = bid
    static member Ask with get() = ask
    static member Buy with get() = buy

type DataSource() =   
    inherit ContentControl() 
    let (handler,event) = Event.create<EventArgs>()
    member this.DataChanged = event
    member this.DoSomethingtoRaiseEvent() = handler(EventArgs.Empty)
    
type DataChangedEventManager =
    inherit WeakEventManager
    
    [<DefaultValue(false)>]
    static val mutable private mgr:DataChangedEventManager
    
    private new() as this = {inherit WeakEventManager()}
    

    
    static member private CurrentManager 
        with get() =
            let mgrType = typeof<DataChangedEventManager>
            
            match WeakEventManager.GetCurrentManager(mgrType) with
            | :? DataChangedEventManager -> ()
            | _ -> 
                    DataChangedEventManager.mgr <- DataChangedEventManager()
                    WeakEventManager.SetCurrentManager(mgrType,DataChangedEventManager.mgr)
            DataChangedEventManager.mgr    
    
    static member AddListener((source:DataSource), (listener:IWeakEventListener)) = DataChangedEventManager.CurrentManager.ProtectedAddListener(source,listener)
    
    static member RemoveListener((source:DataSource), (listener:IWeakEventListener)) = DataChangedEventManager.CurrentManager.ProtectedRemoveListener(source, listener)
    
    member private this.PrivateOnDataChanged (sender:obj) (e:EventArgs) = base.DeliverEvent(sender, e)

    override this.StartListening(source:obj) =
        let evtSource = source :?> DataSource
        evtSource.DataChanged.AddHandler(Handler<EventArgs>(this.PrivateOnDataChanged))
        
    override this.StopListening(source:obj) =
        let evtSource = source :?> DataSource
        evtSource.DataChanged.RemoveHandler(Handler<EventArgs>(this.PrivateOnDataChanged))

   
    
type SpikeControl() =
    inherit ContentControl() 
    
    let mutable commandTarget = None
    
    static let commandProperty = DependencyProperty.Register("Command", typeof<ICommand>, typeof<SpikeControl>, new PropertyMetadata(null, PropertyChangedCallback(SpikeControl.onCommandChanged)))
    
    static let commandParameterProperty = DependencyProperty.Register("CommandParameter", typeof<obj>, typeof<SpikeControl>)
    
    static member private onCommandChanged (d:DependencyObject) (e:DependencyPropertyChangedEventArgs) =
        (d :?> SpikeControl).HookUpCommand((e.OldValue :?> ICommand), (e.NewValue :?> ICommand))
        
    member this.DoSomethingToInvokeCommand() = this.InvokeCommand()
    
    member private this.InvokeCommand() =
        if ((this :> ICommandSource).Command <> null) 
            then
                match (this :> ICommandSource).Command with
                | :? RoutedCommand as v -> v.Execute(this, (this :> ICommandSource).CommandTarget)
                | _ -> (this :> ICommandSource).Command.Execute((this :> ICommandSource).CommandParameter)
            
                
                
    
    member private this.HookUpCommand(oldCommand:ICommand, newCommand:ICommand) =
        if (oldCommand <> null) then oldCommand.CanExecuteChanged.RemoveHandler(EventHandler(this.command_CanExecuteChanged))
        if (newCommand <> null) then newCommand.CanExecuteChanged.AddHandler(EventHandler(this.command_CanExecuteChanged))
    
    member private this.command_CanExecuteChanged (sender:obj) (e:EventArgs) =
        if ((this :> ICommandSource).Command <> null)
            then
                match (this :> ICommandSource).Command with
                | :? RoutedCommand as v -> 
                    let inputElt = match commandTarget with | Some(v) -> v | None -> this :> IInputElement
                    if (v.CanExecute((this :> ICommandSource).CommandParameter, inputElt))
                        then
                            this.IsEnabled <- true
                        else
                            this.IsEnabled <- false
                | _ -> 
                    if ((this :> ICommandSource).Command.CanExecute((this :> ICommandSource).CommandParameter))
                        then
                            this.IsEnabled <- true
                        else
                            this.IsEnabled <- false
    

    
    member this.Command with set(x:ICommand) = this.SetValue(commandProperty,x)
    member this.CommandParameter with set(x:obj) = this.SetValue(commandParameterProperty,x)
    member this.CommandTarget with set(x:IInputElement) = commandTarget <- Some(x)   
         
    interface ICommandSource with
        member this.Command with get() = this.GetValue(commandProperty) :?> ICommand
        
        member this.CommandParameter with get() = this.GetValue(commandParameterProperty)
        
        member this.CommandTarget 
            with get() = match commandTarget with | Some(v) -> v | _ -> failwith "initialization failure"

type TextBoxWatermarkHelper() =
    inherit DependencyObject()
    
    static let isWatermarkVisibleProperty = DependencyProperty.RegisterAttached("IsWatermarkVsibile", typeof<bool>, typeof<TextBoxWatermarkHelper>)
    
    static let mutable watermarkTextProperty = DependencyProperty.RegisterAttached("WatermarkText", typeof<string>, typeof<TextBoxWatermarkHelper>, new PropertyMetadata("Watermark", PropertyChangedCallback(TextBoxWatermarkHelper.onWatermarkTextChanged)))
    
  //  static member WatermarkTextProperty with get() =  watermarkTextProperty and set(x) = watermarkTextProperty <- x
    
    static member GetWatermarkText(control:TextBox) = control.GetValue(watermarkTextProperty) :?> string
    static member SetWatermarkText((control:TextBox), (text:string)) = control.SetValue(watermarkTextProperty, text)
    
    static member GetIsWatermarkVisible(control:TextBox) = control.GetValue(isWatermarkVisibleProperty) :?> bool
    
    static member private onWatermarkTextChanged (d:DependencyObject) (e:DependencyPropertyChangedEventArgs) =
        match d with
        | :? TextBox as control -> 
            control.SetValue(isWatermarkVisibleProperty, true)
            control.LostFocus.AddHandler(RoutedEventHandler(TextBoxWatermarkHelper.onControlLostFocus))
            control.GotFocus.AddHandler(RoutedEventHandler(TextBoxWatermarkHelper.onControlGotFocus))
        | _ -> ()
        
    static member private onControlGotFocus (sender:obj) (e:RoutedEventArgs) =
        (sender :?> TextBox).SetValue(isWatermarkVisibleProperty, false)
        
    static member private onControlLostFocus (sender:obj) (e:RoutedEventArgs) =
        match sender with
        | :? TextBox as control ->
            if (String.IsNullOrEmpty(control.Text))
                then
                    control.SetValue(isWatermarkVisibleProperty, true)
        | _ -> ()
    
   