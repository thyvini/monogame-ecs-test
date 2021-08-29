module Components

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Types

[<Struct>] type Translate = { Position: Vector2 }
[<Struct>] type Rotate =  Rotate of float32
[<Struct>] type Scale =  Scale of float32
[<Struct>] type Velocity = Velocity of  Vector2
module Velocity =
    let create x y = Vector2(x,y) |> Velocity

[<Struct>] type Score = { Player: PlayerIndex; Value: byte }
[<Struct>] type GameText = { SpriteFont: SpriteFont; Position: Vector2 }
[<Struct>] type FSharpLogo = { Texture: Texture2D; Speed: float32 }
[<Struct>] type Player = { Texture: Texture2D; Size: Vector2; Index: PlayerIndex }
[<Struct>] type Ball = { Size: float32; Texture: Texture2D }
