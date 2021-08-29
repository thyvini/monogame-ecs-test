module Ball

open Events
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

            entity.Add
                {
                    Position = position
                    Scale = 1f
                    Rotation = 0f
                }

            entity.Add { X = initialSpeed; Y = initialSpeed }
            eid
        |> Join.update2
        |> Join.over world
    )
    |> ignore

    world.On<Update>(
        fun (e: Update) struct (velocity: Velocity, transform: Transform, ball: Ball) ->
            let y = transform.Position.Y

            match y with
            | y when
                y + ball.Size > float32 e.Game.GraphicsDevice.Viewport.Height
                || y < 0f
                ->
                { velocity with Y = -velocity.Y }
            | _ -> velocity
        |> Join.update3
        |> Join.over world
    )
    |> ignore

    world.On<Update>(
        fun _ struct (transform: Transform, velocity: Velocity, _: Ball) ->
            { transform with
                Position = Vector2(transform.Position.X + velocity.X, transform.Position.Y + velocity.Y)
            }
        |> Join.update3
        |> Join.over world
    )
    |> ignore

    world.On<Update>(
        fun (e: Update) struct (transform: Transform, ball: Ball) ->
            let player1Point =
                transform.Position.X + ball.Size > float32 e.Game.GraphicsDevice.Viewport.Width

            let player2Point = transform.Position.X < 0f

            if player1Point then
                world.Send { PlayerIndex = P1; Game = e.Game }

            elif player2Point then
                world.Send { PlayerIndex = P2; Game = e.Game }

        |> Join.iter2
        |> Join.over world
    )
    |> ignore
    
    world.On<ScoreIncrease>(
        fun (s: ScoreIncrease) struct(transform: Transform, ball: Ball) ->
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
        fun _ struct(velocity: Velocity, _: Ball) ->
            {velocity with
                X = if velocity.X > 0f then -initialSpeed else initialSpeed
                Y = if velocity.Y > 0f then -initialSpeed else initialSpeed
            }
        |> Join.update2
        |> Join.over world) |> ignore

    world.On<Collisions.BallAndPaddle>
        (fun e ->
            let entity = world.Get e.BallEid

            let x =
                match e.BallVelocity.X with
                | x when x > 0f -> -(x + 0.1f)
                | x -> -(x - 0.1f)

            entity.Add({ X = x; Y = e.BallVelocity.Y }))
    |> ignore
    
    world.On<Draw>(
        fun e struct (tr: Transform, b: Ball) ->
            e.SpriteBatch.Draw(
                b.Texture,
                Rectangle(Point(tr.Position.X |> int, tr.Position.Y |> int), Point(b.Size |> int, b.Size |> int)),
                Color.White
            )
        |> Join.iter2
        |> Join.over world
    )
    |> ignore