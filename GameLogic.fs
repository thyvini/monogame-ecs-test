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
type Movable = { Velocity: Vector2 }

[<Struct>]
type Player =
    {
        Texture: Texture2D
        Size: Vector2
        Index: PlayerIndex
    }


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
    
let clampPosition (player: Player) (game: Game) (transform: Transform) =
    let x = transform.Position.X
    let minY = 0f
    let maxY = game.GraphicsDevice.Viewport.Height |> float32
    let curY = transform.Position.Y
    match curY with
    | y when y < minY -> {transform with Position = Vector2(x, 0f)}
    | y when y + player.Size.Y > maxY -> {transform with Position = Vector2(x, maxY - player.Size.Y)}
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

            entity.Add { Velocity = Vector2(0f, 5f) }
            eid
        |> Join.update2
        |> Join.over world
    )
    |> ignore

    // update logo system
    world.On<Update>(
        fun e struct (tr: Transform, logo: FSharpLogo) -> updateLogo logo tr (float32 e.DeltaTime.TotalSeconds)

        |> Join.update2
        |> Join.over world
    )
    |> ignore

    world.On<Update>(
        fun e struct (tr: Transform, v: Movable, p: Player) ->
            let state = Keyboard.GetState()

            match p.Index with
            | PlayerIndex.One ->
                match state with
                | KeyDown Keys.W ->
                    { tr with
                        Position = Vector2(tr.Position.X, tr.Position.Y - v.Velocity.Y)
                    }
                | KeyDown Keys.S ->
                    { tr with
                        Position = Vector2(tr.Position.X, tr.Position.Y + v.Velocity.Y)
                    }
                | _ -> tr
            | PlayerIndex.Two ->
                match state with
                | KeyDown Keys.Up ->
                    { tr with
                        Position = Vector2(tr.Position.X, tr.Position.Y - v.Velocity.Y)
                    }
                | KeyDown Keys.Down ->
                    { tr with
                        Position = Vector2(tr.Position.X, tr.Position.Y + v.Velocity.Y)
                    }
                | _ -> tr
            | _ -> raise (Exception())
            |> clampPosition p e.Game

        |> Join.update3
        |> Join.over world
    )
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

    // quit game system
    world.On<Update>
        (fun e ->
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back = ButtonState.Pressed
                || Keyboard.GetState().IsKeyDown(Keys.Escape)) then
                e.Game.Exit())
    |> ignore

    world
