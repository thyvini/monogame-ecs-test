[<RequireQualifiedAccess>]
module Collisions

open Garnet.Composition
open Microsoft.Xna.Framework

[<Struct>] type BallAndPaddle = { BallEid: Eid; BallVelocity: Vector2 }
