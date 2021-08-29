module Components

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Types

[<Struct>] type Translate = { Position: Vector2 }
[<Struct>] type Rotation = { Rotation: float32 }
[<Struct>] type Scale = { Scale: float32 }
[<Struct>] type Velocity = { Velocity: Vector2 }
[<Struct>] type Score = { Player: PlayerIndex; Value: byte }
[<Struct>] type GameText = { SpriteFont: SpriteFont; Position: Vector2 }
[<Struct>] type FSharpLogo = { Texture: Texture2D; Speed: float32 }
[<Struct>] type Player = { Texture: Texture2D; Size: Vector2; Index: PlayerIndex }
[<Struct>] type Ball = { Size: float32; Texture: Texture2D }