module Components

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Types

[<Struct>]
type Transform = { Rotation: float32; Scale: float32; Position: Vector2 }

[<Struct>]
type Velocity = { X: float32; Y: float32 }

[<Struct>]
type FSharpLogo = { Texture: Texture2D; Speed: float32 }

[<Struct>]
type Player = { Texture: Texture2D; Size: Vector2; Index: PlayerIndex }

[<Struct>]
type Ball = { Size: float32; Texture: Texture2D }