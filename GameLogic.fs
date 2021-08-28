module GameLogic

open System
open Garnet.Composition
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open Events

// components
[<Struct>]
type Transform =
    {
        Rotation: float32
        Scale: float32
        Position: Vector2
    }

[<Struct>]
type FSharpLogo = { Texture: Texture2D; Speed: float32 }

[<Struct>]
type Velocity = { X: float32; Y: float32 }

[<Struct>]
type Player =
    {
        Texture: Texture2D
        Size: Vector2
        Index: PlayerIndex
    }

[<Struct>]
type Ball = { Size: float32; Texture: Texture2D }

let (|KeyDown|_|) k (state: KeyboardState) =
    if state.IsKeyDown k then
        Some()
    else
        None

let movementVector =
    function
    | KeyDown Keys.W & KeyDown Keys.A -> Vector2(-1.f, -1.f)
    | KeyDown Keys.W & KeyDown Keys.D -> Vector2(1.f, -1.f)
    | KeyDown Keys.S & KeyDown Keys.A -> Vector2(-1.f, 1.f)
    | KeyDown Keys.S & KeyDown Keys.D -> Vector2(1.f, 1.f)
    | KeyDown Keys.W -> Vector2(0.f, -1.f)
    | KeyDown Keys.S -> Vector2(0.f, 1.f)
    | KeyDown Keys.A -> Vector2(-1.f, 0.f)
    | KeyDown Keys.D -> Vector2(1.f, -0.f)
    | _ -> Vector2.Zero

let createLogo (game: Game) =
    {
        Texture = game.Content.Load("logo")
        Speed = 100f
    }

let createPlayer (game: Game) index =
    let texture = new Texture2D(game.GraphicsDevice, 1, 1)
    texture.SetData([| Color.Black |])

    {
        Texture = texture
        Size = Vector2(40f, 200f)
        Index = index
    }
    
let createBall (game: Game) =
    let texture = new Texture2D(game.GraphicsDevice, 1, 1)
    texture.SetData([| Color.Black |])
    {
        Texture = texture
        Size = 40f
    }

let startPosition (game: Game) =
    {
        Position = Vector2(float32 game.Window.ClientBounds.Width / 2f, float32 game.Window.ClientBounds.Height / 2f)
        Rotation = 0f
        Scale = 0f
    }

let updateLogo logo logoTransform deltaTime =
    let {
            Scale = scale
            Rotation = rot
            Position = pos
        } =
        logoTransform

    { logoTransform with
        Rotation = rot + 0.01f
        Scale =
            if (scale < 2f) then
                scale + 0.04f
            else
                scale
        Position = pos
    }

let drawLogo (spriteBatch: SpriteBatch) (logo: FSharpLogo) transform =
    let logoCenter =
        Vector2(float32 logo.Texture.Bounds.Width, float32 logo.Texture.Bounds.Height)
        / 2f

    spriteBatch.Draw(
        logo.Texture,
        transform.Position,
        logo.Texture.Bounds,
        Color(255, 255, 255, 80),
        transform.Rotation,
        logoCenter,
        transform.Scale,
        SpriteEffects.None,
        0f
    )

let clampPosition (size: Vector2) (game: Game) (transform: Transform) =
    let x = transform.Position.X
    let minY = 0f

    let maxY =
        game.GraphicsDevice.Viewport.Height |> float32

    let curY = transform.Position.Y

    match curY with
    | y when y < minY ->
        { transform with
            Position = Vector2(x, 0f)
        }
    | y when y + size.Y > maxY ->
        { transform with
            Position = Vector2(x, maxY - size.Y)
        }
    | _ -> transform


let configureWorld (world: Container) =

    // load content
    world.On
        (fun (LoadContent game) ->
            world.Create().With(createLogo game) |> ignore

            world
                .Create()
                .With(createPlayer game PlayerIndex.One)
            |> ignore

            world
                .Create()
                .With(createPlayer game PlayerIndex.Two)
            |> ignore
            
            world.Create()
                .With(createBall game)
            |> ignore)
    |> ignore

    // start logo system
    world.On(
        fun (Start game) struct (eid: Eid, logo: FSharpLogo) ->
            let entity = world.Get eid
            entity.Add(startPosition game)
            eid
        |> Join.update2
        |> Join.over world
    )
    |> ignore

    world.On(
        fun (Start game) struct (eid: Eid, player: Player) ->
            let entity = world.Get eid

            let position =
                match player.Index with
                | PlayerIndex.One ->
                    Vector2(
                        player.Size.X,
                        float32 game.Window.ClientBounds.Height / 2f
                        - player.Size.Y / 2f
                    )
                | PlayerIndex.Two ->
                    Vector2(
                        float32 game.Window.ClientBounds.Width
                        - player.Size.X * 2f,
                        float32 game.Window.ClientBounds.Height / 2f
                        - player.Size.Y / 2f
                    )
                | _ -> raise (Exception())

            entity.Add
                {
                    Rotation = 0f
                    Scale = 1f
                    Position = position
                }

            entity.Add { X = 0f; Y = 5f }
            eid
        |> Join.update2
        |> Join.over world
    )
    |> ignore
    
    world.On(
        fun (Start game) struct (eid: Eid, ball: Ball) ->
            let entity = world.Get eid
            
            let position = Vector2(
                            float32 game.Window.ClientBounds.Width / 2f
                            - ball.Size / 2f,
                            float32 game.Window.ClientBounds.Height / 2f
                            - ball.Size / 2f)
            entity.Add {Position = position; Scale = 1f; Rotation = 0f}
            entity.Add { X = 3f; Y = 3f }
            eid
        |> Join.update2
        |> Join.over world) |> ignore

    // update logo system
    world.On<Update>(
        fun e struct (tr: Transform, logo: FSharpLogo) -> updateLogo logo tr (float32 e.DeltaTime.TotalSeconds)

        |> Join.update2
        |> Join.over world
    )
    |> ignore

    world.On<Update>(
        fun e struct (transform: Transform, velocity: Velocity, player: Player) ->
            let state = Keyboard.GetState()

            match player.Index with
            | PlayerIndex.One ->
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
            | PlayerIndex.Two ->
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
            | _ -> raise (Exception())
            |> clampPosition player.Size e.Game

        |> Join.update3
        |> Join.over world
    )
    |> ignore
    
    world.On<Update>(
        fun e struct (velocity: Velocity, transform: Transform, ball: Ball) ->
            let y = transform.Position.Y
            match y with
            | y when y + ball.Size > float32 e.Game.GraphicsDevice.Viewport.Height || y < 0f -> {velocity with Y = -velocity.Y}
            | _ -> velocity
        |> Join.update3
        |> Join.over world) |> ignore
    
    world.On<Update>(
        fun e struct (transform: Transform, velocity: Velocity, ball: Ball) ->
            {transform with Position = Vector2(transform.Position.X + velocity.X, transform.Position.Y + velocity.Y)}
        |> Join.update3
        |> Join.over world) |> ignore

    world.On<Update>(
        fun _ struct (player: Player, transform: Transform) ->
            for r in world.Query<Eid, Ball, Velocity, Transform>() do
                let struct (eid, ball, ballVelocity, ballTransform) = r.Values

                let rectangle =
                    Rectangle(ballTransform.Position.ToPoint(), Point(int ball.Size, int ball.Size))

                if
                    rectangle.Intersects
                        (Rectangle(transform.Position.ToPoint(), Point(int player.Size.X, int player.Size.Y)))
                then
                    world.Send(
                        {
                            BallEid = eid
                            BallVelocity = Vector2(ballVelocity.X, ballVelocity.Y)
                        }
                    )
        |> Join.iter2
        |> Join.over world
    )
    |> ignore

    world.On<BallAndPaddleCollision>
        (fun e ->
            let entity = world.Get e.BallEid
            let x =
                match e.BallVelocity.X with
                | x when x > 0f -> -(x + 0.1f)
                | x -> -(x - 0.1f)

            entity.Add(
                {
                    X = x
                    Y = e.BallVelocity.Y
                }
            ))
    |> ignore

    // drawlogo system
    world.On<Draw>(
        fun e struct (tr: Transform, logo: FSharpLogo) -> drawLogo e.SpriteBatch logo tr

        |> Join.iter2
        |> Join.over world
    )
    |> ignore

    world.On<Draw>(
        fun e struct (tr: Transform, p: Player) ->
            e.SpriteBatch.Draw(
                p.Texture,
                Rectangle(Point(tr.Position.X |> int, tr.Position.Y |> int), Point(p.Size.X |> int, p.Size.Y |> int)),
                Color.White
            )
        |> Join.iter2
        |> Join.over world
    )
    |> ignore
    
    world.On<Draw>(
        fun e struct (tr: Transform, b: Ball) ->
            e.SpriteBatch.Draw(
                b.Texture,
                Rectangle(Point(tr.Position.X |> int, tr.Position.Y |> int), Point(b.Size |> int, b.Size |> int)),
                Color.White)
        |> Join.iter2
        |> Join.over world) |> ignore

    // quit game system
    world.On<Update>
        (fun e ->
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back = ButtonState.Pressed
                || Keyboard.GetState().IsKeyDown(Keys.Escape)) then
                e.Game.Exit())
    |> ignore

    world
