module Logo

open Events
open Garnet.Composition
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Components

let private createLogo (game: Game) =
    {
        Texture = game.Content.Load("logo")
        Speed = 100f
    }
    
let private startPosition (game: Game) =
    {
        Position = Vector2(float32 game.Window.ClientBounds.Width / 2f, float32 game.Window.ClientBounds.Height / 2f)
        Rotation = 0f
        Scale = 0f
    }

let private updateLogo logo logoTransform deltaTime =
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

let private drawLogo (spriteBatch: SpriteBatch) (logo: FSharpLogo) (transform: Transform) =
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

let configureLogo (world: Container) =
    world.On
        (fun (LoadContent game) ->
            world.Create().With(createLogo game) |> ignore
        ) |> ignore

    world.On(
        fun (Start game) struct (eid: Eid, logo: FSharpLogo) ->
            let entity = world.Get eid
            entity.Add(startPosition game)
            eid
        |> Join.update2
        |> Join.over world
    )
    |> ignore
    
    world.On<Update>(
        fun e struct (tr: Transform, logo: FSharpLogo) -> updateLogo logo tr (float32 e.DeltaTime.TotalSeconds)

        |> Join.update2
        |> Join.over world
    )
    |> ignore
    
    world.On<Draw>(
        fun e struct (tr: Transform, logo: FSharpLogo) ->
            drawLogo e.SpriteBatch logo tr
        |> Join.iter2
        |> Join.over world
    )
    |> ignore