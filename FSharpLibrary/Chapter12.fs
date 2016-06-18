namespace FSharp.Chapter12

open System
open System.Windows
open System.Windows.Media
open System.Windows.Media.Media3D
open System.Windows.Media.Animation

type SurfaceType =
    | Nil
    | Plane
    | Sphere 
    | Cylinder
    | Cone

type MeshCreator() =
    
    static member CreateMesh (surface:SurfaceType) (hPoints:int) (vPoints:int) =
        let mesh = new MeshGeometry3D()
        mesh.Positions <- MeshCreator.CreatePositions surface hPoints vPoints
        mesh.TextureCoordinates <- MeshCreator.CreateTextureCoordinates hPoints vPoints
        mesh.TriangleIndices <- MeshCreator.CreateTriangleIndices hPoints vPoints
        mesh

    static member CreatePositions (surface:SurfaceType) (hPoints:int) (vPoints:int) =
        MeshCreator.VerifyHVPoints hPoints vPoints
        
        match surface with 
        | Plane     -> MeshCreator.CreatePlane hPoints vPoints
        | Sphere    -> MeshCreator.CreateSphere hPoints vPoints
        | Cylinder  -> MeshCreator.CreateCylinder hPoints vPoints
        | Cone      -> MeshCreator.CreateCone hPoints vPoints
        | _         -> MeshCreator.CreatePlane hPoints vPoints

    static member CreateTriangleIndices (hPoints:int) (vPoints:int) =
        MeshCreator.VerifyHVPoints hPoints vPoints
        
        let indices = Int32Collection()
        for y in 0..vPoints-2 do
            for x in 0..hPoints-2 do
                let v1 = x + y * hPoints
                let v2 = v1 + 1
                let v3 = v1 + hPoints
                let v4 = v3 + 1
                
                [v1;v3;v4;v1;v4;v2] |> List.iter (fun v -> indices.Add(v))
        indices

    static member CreateTextureCoordinates (hPoints:int) (vPoints:int) =
        let points = PointCollection()
        for i in [0..vPoints-1] do
            let v = (float i) / (float vPoints - 1.)
            for j in [0..hPoints-1] do
                let h = (float j) / (float hPoints - 1.)
                points.Add(Point(h,v))
        points
    
    static member VerifyHVPoints (hPoints:int) (vPoints:int) =
        if (hPoints < 2 || vPoints < 2)
            then
                failwith "hPoints and vPoints have to be greater than or equal to 2"

    static member CreatePlane (hPoints:int) (vPoints:int) =
        let points = new Point3DCollection()
        for i in [0..vPoints-1] do
            let s = (float i) / (float vPoints - 1.)
            let y = 1. - 2.*s
            for j in [0..hPoints-1] do
                let t = (float j) / (float hPoints - 1.)
                
                let x = -1. + 2.*t
                let z = 0.
                                
                points.Add(Point3D(x,y,z))
        points

    static member CreateSphere (hPoints:int) (vPoints:int) =
        let points = new Point3DCollection()
        for i in [0..vPoints-1] do
            let s = (float i) / (float vPoints - 1.)
            for j in [0..hPoints-1] do
                let t = (float j) / (float hPoints - 1.)
                
                let z = -Math.Cos(t*2.*Math.PI)*Math.Sin(s*Math.PI)
                let x = -Math.Sin(t*2.*Math.PI)*Math.Sin(s*Math.PI)
                let y = Math.Cos(s*Math.PI)
                
                points.Add(Point3D(x,y,z))
        points

    static member CreateCylinder (hPoints:int) (vPoints:int) =
        let points = new Point3DCollection()
        for i in [0..vPoints-1] do
            let s = (float i) / (float vPoints - 1.)
            for j in [0..hPoints-1] do
                let t = (float j) / (float hPoints - 1.)
                
                let z = -Math.Cos(t*2.*Math.PI)
                let x = -Math.Sin(t*2.*Math.PI)
                let y = Math.Cos(s*Math.PI)
                
                points.Add(Point3D(x,y,z))
        points
        
    static member CreateCone (hPoints:int) (vPoints:int) =
        let points = new Point3DCollection()
        for i in [0..vPoints-1] do
            let s = (float i) / (float vPoints - 1.)
            for j in [0..hPoints-1] do
                let t = (float j) / (float hPoints - 1.)
                
                let z = -Math.Cos(t*2.*Math.PI)*s
                let x = -Math.Sin(t*2.*Math.PI)*s
                let y = 1. - 2. * s
                
                points.Add(Point3D(x,y,z))
        points

[<AbstractClass>]
type Point3DCollectionAnimationBase() =
    inherit AnimationTimeline()
    
    override this.TargetPropertyType with get() = typeof<Point3DCollection>
    
    override this.GetCurrentValue(defaultOriginValue,defaultDestinationValue,animationClock) =
        if (defaultOriginValue = null || defaultDestinationValue = null || animationClock = null)
            then
                failwith "Null Argument Exception"
            else
                this.GetCurrentValueCore (defaultOriginValue :?> Point3DCollection) (defaultDestinationValue :?> Point3DCollection)  animationClock :> obj
    
    abstract member GetCurrentValueCore: Point3DCollection -> Point3DCollection -> AnimationClock -> Point3DCollection

type MeshMorphAnimation() =
    inherit Point3DCollectionAnimationBase()
    
    static let startSurfaceProperty = DependencyProperty.Register("StartSurface", typeof<SurfaceType>, typeof<MeshMorphAnimation>, PropertyMetadata(SurfaceType.Nil))
    
    static let endSurfaceProperty = DependencyProperty.Register("EndSurface", typeof<SurfaceType>, typeof<MeshMorphAnimation>, PropertyMetadata(SurfaceType.Nil))
    
    static let horizontalPointsProperty = DependencyProperty.Register("HorizontalPoints", typeof<int>, typeof<MeshMorphAnimation>)
    
    static let verticalPointsProperty = DependencyProperty.Register("VerticalPoints", typeof<int>, typeof<MeshMorphAnimation>)
    
    let mutable startPoints = Point3DCollection()
    let mutable endPoints = Point3DCollection()
    let mutable collectionsCreated = false
    
    override this.CreateInstanceCore() = MeshMorphAnimation() :> Freezable
    
    member this.StartSurface
        with get() = this.GetValue(startSurfaceProperty) :?> SurfaceType
        and set(x:SurfaceType) = this.SetValue(startSurfaceProperty, x)
        
    member this.EndSurface
        with get() = this.GetValue(endSurfaceProperty) :?> SurfaceType
        and set(x:SurfaceType) = this.SetValue(endSurfaceProperty, x)    
        
    member this.HorizontalPoints
        with get() = this.GetValue(horizontalPointsProperty) :?> int
        and set(x:int) = this.SetValue(horizontalPointsProperty, x)   

    member this.VerticalPoints
        with get() = this.GetValue(verticalPointsProperty) :?> int
        and set(x:int) = this.SetValue(verticalPointsProperty, x)   
    
    member private this.Interpolate (progress:float) =
        let points = Point3DCollection()
        for i in [0..startPoints.Count - 1] do
            let s = startPoints.[i]
            let e = endPoints.[i]
            
            let x = s.X + (e.Y - s.X) * progress
            let y = s.Y + (e.Y - s.Y) * progress
            let z = s.Z + (e.Z - s.Z) * progress
            
            let p = Point3D(x,y,z)
            points.Add(p)
        points
    
    override this.GetCurrentValueCore src dest clock =
        if (not collectionsCreated)
            then
                startPoints <- (
                    match this.StartSurface with
                    | Nil -> MeshCreator.CreatePositions this.StartSurface this.HorizontalPoints this.VerticalPoints
                    | _ -> src)
                
                endPoints <- (
                    match this.EndSurface with
                    | Nil -> MeshCreator.CreatePositions this.EndSurface this.HorizontalPoints this.VerticalPoints
                    | _ -> src)
                
                collectionsCreated <- true
                startPoints
            else
                if (float clock.CurrentProgress >=  1.)
                    then endPoints
                    else
                        this.Interpolate(clock.CurrentProgress.Value)