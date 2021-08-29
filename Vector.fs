[<AutoOpen>]
module Game.VectorModule

open Microsoft.Xna.Framework
let vect x y = Vector2(x, y)
let rect (location:Vector2) (size:Vector2) = Rectangle(location.ToPoint(), size.ToPoint())
module Vector2 =
    let toTuple (vector: Vector2) = vector.X, vector.Y

type Vector2 with
    member this.WithX x = vect x this.Y
    member this.WithY y = vect this.X y

let (|Vec|_|) v = v |> Vector2.toTuple |> Some