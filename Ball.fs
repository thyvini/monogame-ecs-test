module Ball

open Events
open Game
open Garnet.Composition
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Components
open Types

let private initialSpeed = 3f
let private initialVelocity = Vector2 initialSpeed

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

            entity.Add (Rotate 0f)
            entity.Add (Scale 1f)
            entity.Add { Position = position }
            entity.Add (Velocity initialVelocity)

            eid
        |> Join.update2
        |> Join.over world
    )
    |> ignore

    world.On<Update>(
        fun (e: Update) struct (Velocity velocity as velDefault, translate: Translate, ball: Ball) ->
            let y = translate.Position.Y

            if y + ball.Size > float32 e.Game.GraphicsDevice.Viewport.Height || y < 0f
            then  velocity.WithY(-velocity.Y) |> Velocity
            else velDefault

        |> Join.update3
        |> Join.over world
    )
    |> ignore

    world.On<Update>(
        fun _ struct (translate: Translate, Velocity velocity, _: Ball) ->
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
    
    world.On(
        fun (s: ScoreIncrease) struct(transform: Translate, ball: Ball) ->
            let width = float32 s.Game.GraphicsDevice.Viewport.Width
            let height = float32 s.Game.GraphicsDevice.Viewport.Height
            let center = vect (width / 2f - ball.Size / 2f) (height / 2f - ball.Size / 2f)
            {transform with Position = center }
        |> Join.update2
        |> Join.over world) |> ignore
    
    world.On<ScoreIncrease>(
        fun _ struct(Velocity velocity, _: Ball) ->
            let x = if velocity.X > 0f then -initialSpeed else initialSpeed
            let y = if velocity.Y > 0f then -initialSpeed else initialSpeed
            Velocity.create x y
        |> Join.update2
        |> Join.over world) |> ignore


    world.On<Collisions.BallAndPaddle>
        (fun e ->
            let entity = world.Get e.BallEid

            let x =
                (- e.BallVelocity.X) +
                if e.BallVelocity.X > 0f
                then 0.1f else -0.1f

            entity.Add (Velocity.create x e.BallVelocity.Y))
    |> ignore
    
    world.On<Draw>(
        fun draw struct (translate: Translate, b: Ball) ->
            draw.SpriteBatch.Draw(
                b.Texture,
                (rect translate.Position (Vector2 b.Size)),
                Color.White)
        |> Join.iter2
        |> Join.over world
    )
    |> ignore