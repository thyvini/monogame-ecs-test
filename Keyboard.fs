module Keyboard

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input

let (|KeyDown|_|) k (state: KeyboardState) =
    if state.IsKeyDown k then Some() else None

let movementVector =
    function
    | KeyDown Keys.K & KeyDown Keys.H -> Vector2(-1.f, -1.f)
    | KeyDown Keys.K & KeyDown Keys.L -> Vector2(1.f, -1.f)
    | KeyDown Keys.J & KeyDown Keys.H -> Vector2(-1.f, 1.f)
    | KeyDown Keys.J & KeyDown Keys.L -> Vector2(1.f, 1.f)
    | KeyDown Keys.K -> Vector2(0.f, -1.f)
    | KeyDown Keys.J -> Vector2(0.f, 1.f)
    | KeyDown Keys.H -> Vector2(-1.f, 0.f)
    | KeyDown Keys.L -> Vector2(1.f, -0.f)
    | _ -> Vector2.Zero
