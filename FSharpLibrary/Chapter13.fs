namespace FSharp.Chapter13

open System
open System.Windows
open System.Reflection
open System.Windows.Media
open System.Windows.Media.Effects

type Global() =
    
    static let assemblyShorName = typeof<Global>.Assembly.ToString().Split(',').[0]
    
    static member MakePackUri(relativeFile:string) =
        Uri("pack://application:,,,/" + assemblyShorName + ";component/" + relativeFile)


type GrayscaleEffect() =
    inherit ShaderEffect()
    
    static let shaderInstance = new PixelShader(UriSource = new Uri(@"pack://application:,,,/Chapters;component/Chapter13/Shaders/Grayscale.ps"))
    
    static let inputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof<GrayscaleEffect>, 0)
    
    do
        base.PixelShader <- shaderInstance;
        base.UpdateShaderValue(inputProperty)

    member this.Input with get() = this.GetValue(inputProperty) :?> Brush and set(x:Brush) = this.SetValue(inputProperty,x)

type DisplacementEffect() =
    inherit ShaderEffect()
    
    static let shaderInstance = new PixelShader(UriSource = new Uri(@"pack://application:,,,/Chapters;component/Chapter13/Shaders/Displacement.ps"))
    
    
    static let inputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof<DisplacementEffect>, 0)
    
    static let input2Property = ShaderEffect.RegisterPixelShaderSamplerProperty("Input2", typeof<DisplacementEffect>, 1)
    
    static let scaleXProperty = DependencyProperty.Register("ScaleX", typeof<double>, typeof<DisplacementEffect>, new UIPropertyMetadata(1., ShaderEffect.PixelShaderConstantCallback(0)))
    
    static let scaleYProperty = DependencyProperty.Register("ScaleY", typeof<double>, typeof<DisplacementEffect>, new UIPropertyMetadata(1., ShaderEffect.PixelShaderConstantCallback(1)))
    
    do
        base.PixelShader <- shaderInstance;
        base.UpdateShaderValue(inputProperty)
        base.UpdateShaderValue(input2Property)
        base.UpdateShaderValue(scaleXProperty)
        base.UpdateShaderValue(scaleYProperty)
    
    member this.Input with get() = this.GetValue(inputProperty) :?> Brush and set(x:Brush) = this.SetValue(inputProperty,x)
    
    member this.Input2 with get() = this.GetValue(input2Property) :?> Brush and set(x:Brush) = this.SetValue(input2Property,x)
    
    member this.ScaleX  with get() = this.GetValue(scaleXProperty) :?> float and set(x:float) = this.SetValue(scaleXProperty,x)
    
    member this.ScaleY  with get() = this.GetValue(scaleYProperty) :?> float and set(x:float) = this.SetValue(scaleYProperty,x) 
    
type TwirlEffect() =
    inherit ShaderEffect()
    
    static let shaderInstance = new PixelShader(UriSource = new Uri(@"pack://application:,,,/Chapters;component/Chapter13/Shaders/Twirl.ps"))
    
    
    static let inputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof<TwirlEffect>, 0)
    
    static let radiusProperty = DependencyProperty.Register("Radius", typeof<double>, typeof<TwirlEffect>, new UIPropertyMetadata(ShaderEffect.PixelShaderConstantCallback(0)))
    
    static let angleProperty = DependencyProperty.Register("Angle", typeof<double>, typeof<TwirlEffect>, new UIPropertyMetadata( ShaderEffect.PixelShaderConstantCallback(1)))
    
    do
        base.PixelShader <- shaderInstance;
        base.UpdateShaderValue(inputProperty)
        base.UpdateShaderValue(radiusProperty)
        base.UpdateShaderValue(radiusProperty)
    
    member this.Input with get() = this.GetValue(inputProperty) :?> Brush and set(x:Brush) = this.SetValue(inputProperty,x)
    
    member this.Radius with get() = this.GetValue(radiusProperty) :?> float and set(x:float) = this.SetValue(radiusProperty,x)
    
    member this.Angle  with get() = this.GetValue(angleProperty) :?> float and set(x:float) = this.SetValue(angleProperty,x) 
    
    


type SqueezeTransform() =
    inherit GeneralTransform()
    
    let mutable isInverse = false
    
    static let leftProperty = DependencyProperty.Register("Left", typeof<double>, typeof<SqueezeTransform>, new UIPropertyMetadata(0.))
    
    static let rightProperty = DependencyProperty.Register("Right", typeof<double>, typeof<SqueezeTransform>, new UIPropertyMetadata(0.))
    
    member this.IsInverse with get() = isInverse and set(x:bool) = isInverse <- x
    
    member this.Left  with get() = this.GetValue(leftProperty) :?> float and set(x:float) = this.SetValue(leftProperty,x)
    
    member this.Right  with get() = this.GetValue(rightProperty) :?> float and set(x:float) = this.SetValue(rightProperty,x) 
    
    override this.CreateInstanceCore() = SqueezeTransform() :> Freezable
    
    override this.TryTransform(inPoint, result) = 
        let mutable _result = Point()
        if (isInverse)
            then
                if (inPoint.X < this.Left || inPoint.X > this.Right)
                    then false
                    else
                        let ratio = (inPoint.X - this.Left) / (this.Right - this.Left)
                        _result.X <- inPoint.X * ratio
                        _result.Y <- inPoint.Y
                        result <- _result
                        true
            else
                let ratio = inPoint.X
                _result.X <- this.Left + (this.Right - this.Left) * ratio
                _result.Y <- inPoint.Y
                result <- _result
                true
        
    override this.TransformBounds(rect) = failwith "Not Implemented"
    
    override this.CloneCore(sourceFreezable) =
        let transform = sourceFreezable :?> SqueezeTransform
        base.CloneCore(transform)
        transform.copyProperties(this)
        
    override this.Inverse 
        with get() = 
            let transform = this.Clone() :?> SqueezeTransform
            transform.IsInverse <- not this.IsInverse
            transform :> GeneralTransform
            
    member private this.copyProperties(transform:SqueezeTransform) =
        this.IsInverse <- transform.IsInverse
        this.Left <- transform.Left
        this.Right <- transform.Right
    
    interface ICloneable with
        member this.Clone() =
            let transform = new SqueezeTransform()
            transform.copyProperties(this)
            transform :> obj
    



type SqueezeEffect() =
    inherit ShaderEffect()
    
    let transform = SqueezeTransform()
        
    static let shaderInstance = new PixelShader(UriSource = new Uri(@"pack://application:,,,/Chapters;component/Chapter13/Shaders/Squeeze.ps"))
    
    static let inputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof<DisplacementEffect>, 0)
    
    static let leftProperty = DependencyProperty.Register("Left", typeof<double>, typeof<SqueezeEffect>, new UIPropertyMetadata(ShaderEffect.PixelShaderConstantCallback(0)))
    
    static let rightProperty = DependencyProperty.Register("Right", typeof<double>, typeof<SqueezeEffect>, new UIPropertyMetadata(ShaderEffect.PixelShaderConstantCallback(1)))
    
    do
        base.PixelShader <- shaderInstance;
        base.UpdateShaderValue(inputProperty)
        base.UpdateShaderValue(leftProperty)
        base.UpdateShaderValue(rightProperty)
    
    override this.EffectMapping with get() = transform.Left <- this.Left; transform.Right <- this.Right; transform :> GeneralTransform
    
    member this.Input with get() = this.GetValue(inputProperty) :?> Brush and set(x:Brush) = this.SetValue(inputProperty,x)
    
    member this.Left  with get() = this.GetValue(leftProperty) :?> float and set(x:float) = this.SetValue(leftProperty,x)
    
    member this.Right  with get() = this.GetValue(rightProperty) :?> float and set(x:float) = this.SetValue(rightProperty,x) 


