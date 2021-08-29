module Player

open Events
open Game.Vector
open Garnet.Composition
open Logo
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open Components
open Types
open Keyboard
open type Collisions.BallAndPaddle

let private createPlayer (game: Game) index =
    let texture = new Texture2D(game.GraphicsDevice, 1, 1)
    texture.SetData([| Color.Black |])

    { Texture = texture
      Size = Vector2(40f, 200f)
      Index = index }

let private clampPosition (size: Vector2) (game: Game) (translate: Translate) =
    let x = translate.Position.X
    let minY = 0f

    let maxY =
        game.GraphicsDevice.Viewport.Height |> float32

    let curY = translate.Position.Y

    match curY with
    | y when y < minY ->
        { translate with Position = Vector2(x, 0f) }
    | y when y + size.Y > maxY ->
        { translate with Position = Vector2(x, maxY - size.Y) }
    | _ -> translate

let configurePlayer (world: Container) =
    world.On
        (fun (LoadContent game) ->
            world.Create().With(createPlayer game P1)
            |> ignore

            world.Create().With(createPlayer game P2)
            |> ignore)
    |> ignore
    
    world.On(
        fun (Start game) struct (eid: Eid, player: Player) ->
            let entity = world.Get eid

            let position =
                match player.Index with
                | P1 ->
                    Vector2(
                        player.Size.X,
                        float32 game.Window.ClientBounds.Height / 2f
                        - player.Size.Y / 2f
                    )
                | P2 ->
                    Vector2(
                        float32 game.Window.ClientBounds.Width
                        - player.Size.X * 2f,
                        float32 game.Window.ClientBounds.Height / 2f
                        - player.Size.Y / 2f
                    )

            entity.Add { Rotation = 0f }
            entity.Add { Scale = 1f }
            entity.Add { Position = position }
            entity.Add { Velocity = vector 0f 10f }

            eid
        |> Join.update2
        |> Join.over world
    )
    |> ignore
    
    world.On<Update>(
        fun (e: Update) struct (transform: Translate, {Velocity = velocity}: Velocity, player: Player) ->
            let state = Keyboard.GetState()

            match player.Index with
            | P1 ->
                match state with
                | KeyDown Keys.W ->
                    { transform with
                        Position = Vector2(transform.Position.X, transform.Position.Y - velocity.Y)
                    }
                | KeyDown Keys.S ->
                    { transform with
                        Position = Vector2(transform.Position.X, transform.Position.Y + velocity.Y)
                    }
                | _ -> transform
            | P2 ->
                match state with
                | KeyDown Keys.Up ->
                    { transform with
                        Position = Vector2(transform.Position.X, transform.Position.Y - velocity.Y)
                    }
                | KeyDown Keys.Down ->
                    { transform with
                        Position = Vector2(transform.Position.X, transform.Position.Y + velocity.Y)
                    }
                | _ -> transform
            |> clampPosition player.Size e.Game

        |> Join.update3
        |> Join.over world
    )
    |> ignore
    
    world.On<Update>(
        fun _ struct (player: Player, transform: Translate) ->
            for r in world.Query<Eid, Ball, Velocity, Translate>() do
                let struct (eid, ball, { Velocity = ballVelocity }, ballTransform) = r.Values

                let rectangle = rect ballTransform.Position (Vector2 ball.Size)
                if rectangle.Intersects (rect transform.Position player.Size)
                then world.Send(
                        {
                            BallEid = eid
                            BallVelocity = Vector2(ballVelocity.X, ballVelocity.Y)
                        }
                    )
        |> Join.iter2
        |> Join.over world
    )
    |> ignore
    
    world.On<Draw>(
        fun e struct (tr: Translate, p: Player) ->
            e.SpriteBatch.Draw(
                p.Texture,
                (rect tr.Position p.Size),
                Color.White
            )
        |> Join.iter2
        |> Join.over world
    )
    |> ignore
