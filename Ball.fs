module Ball

open Events
open Game
open Game.Vector
open Garnet.Composition
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Components
open Types

let private initialSpeed = 3f

let private createBall (game: Game) =
    let texture = new Texture2D(game.GraphicsDevice, 1, 1)
    texture.SetData([| Color.Black |])
    { Texture = texture; Size = 40f }

let configureBall (world: Container) =
    world.On
        (fun (LoadContent game) ->
            world.Create().With(createBall game) |> ignore)
    |> ignore

    world.On(
        fun (Start game) struct (eid: Eid, ball: Ball) ->
            let entity = world.Get eid

            let position =
                Vector2(
                    float32 game.Window.ClientBounds.Width / 2f
                    - ball.Size / 2f,
                    float32 game.Window.ClientBounds.Height / 2f
                    - ball.Size / 2f
                )

            entity.Add { Rotation = 0f }
            entity.Add { Scale = 1f }
            entity.Add { Position = position }
            entity.Add { Velocity = vector initialSpeed initialSpeed  }

            eid
        |> Join.update2
        |> Join.over world
    )
    |> ignore

    world.On<Update>(
        fun (e: Update) struct ({Velocity = velocity} as v: Velocity, translate: Translate, ball: Ball) ->
            let y = translate.Position.Y

            if y + ball.Size > float32 e.Game.GraphicsDevice.Viewport.Height || y < 0f
            then { Velocity = velocity.WithY(-velocity.Y) }
            else v

        |> Join.update3
        |> Join.over world
    )
    |> ignore

    world.On<Update>(
        fun _ struct (translate: Translate, {Velocity = velocity}: Velocity, _: Ball) ->
            { translate with
                Position = Vector2(translate.Position.X + velocity.X, translate.Position.Y + velocity.Y)
            }
        |> Join.update3
        |> Join.over world
    )
    |> ignore

    world.On<Update>(
        fun (e: Update) struct (translate: Translate, ball: Ball) ->
            let player1Point =
                translate.Position.X + ball.Size > float32 e.Game.GraphicsDevice.Viewport.Width

            let player2Point = translate.Position.X < 0f

            if player1Point then
                world.Send { PlayerIndex = P1; Game = e.Game }

            elif player2Point then
                world.Send { PlayerIndex = P2; Game = e.Game }

        |> Join.iter2
        |> Join.over world
    )
    |> ignore
    
    world.On<ScoreIncrease>(
        fun (s: ScoreIncrease) struct(transform: Translate, ball: Ball) ->
            let width = float32 s.Game.GraphicsDevice.Viewport.Width
            let height = float32 s.Game.GraphicsDevice.Viewport.Height
            {transform with
                Position = Vector2(
                            width / 2f - ball.Size / 2f,
                            height / 2f - ball.Size / 2f)
            }
        |> Join.update2
        |> Join.over world) |> ignore
    
    world.On<ScoreIncrease>(
        fun _ struct({Velocity = velocity}: Velocity, _: Ball) ->
            let x = if velocity.X > 0f then -initialSpeed else initialSpeed
            let y = if velocity.Y > 0f then -initialSpeed else initialSpeed
            { Velocity = Vector2(x,y)  }
        |> Join.update2
        |> Join.over world) |> ignore

    world.On<Collisions.BallAndPaddle>
        (fun e ->
            let entity = world.Get e.BallEid

            let x =
                match e.BallVelocity.X with
                | x when x > 0f -> -(x + 0.1f)
                | x -> -(x - 0.1f)

            entity.Add { Velocity = vector x e.BallVelocity.Y })
    |> ignore
    
    world.On<Draw>(
        fun e struct (translate: Translate, b: Ball) ->
            e.SpriteBatch.Draw(
                b.Texture,
                (rect translate.Position (Vector2 b.Size)),
                Color.White)
        |> Join.iter2
        |> Join.over world
    )
    |> ignore