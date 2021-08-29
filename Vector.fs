[<AutoOpen>]
module Game.Vector

open Microsoft.Xna.Framework
let vector x y = Vector2(x, y)
let rect (location:Vector2) (size:Vector2) = Rectangle(location.ToPoint(), size.ToPoint())
let int2 x y = Vector2(float32 x, float32 y)

module Vector2 =
    let toTuple (vector: Vector2) = vector.X, vector.Y
    let ofTuple (x, y) = Vector2(x, y)

type Vector2 with
    member this.WithX x = vector x this.Y
    member this.WithY y = vector this.X y

let (|Vec|_|) v = v |> Vector2.toTuple |> Some