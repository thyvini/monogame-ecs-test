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
    { Position = Vector2(float32 game.Window.ClientBounds.Width / 2f, float32 game.Window.ClientBounds.Height / 2f) }

let private updateLogoRot rot deltaTime =
    Rotate (rot + 0.1f * deltaTime)

let private updateLogoScale scale deltaTime =
    if (scale < 2f)
    then scale + 0.4f * deltaTime
    else scale
   |> Scale


let private drawLogo (spriteBatch: SpriteBatch) (logo: FSharpLogo) (pos: Translate) (Rotate rot) (Scale scale) =
    let logoCenter =
        Vector2(float32 logo.Texture.Bounds.Width, float32 logo.Texture.Bounds.Height)
        / 2f

    spriteBatch.Draw(
        logo.Texture,
        pos.Position,
        logo.Texture.Bounds,
        Color(255, 255, 255, 80),
        rot,
        logoCenter,
        scale,
        SpriteEffects.None,
        0f
    )

let configureLogo (world: Container) =
    world.On
        (fun (LoadContent game) ->
            world.Create().With(createLogo game) |> ignore) |> ignore

    world.On(
        fun (Start game) struct (eid: Eid, _: FSharpLogo) ->
            let entity = world.Get eid
            entity.Add(startPosition game)
            entity.Add(Rotate 0f)
            entity.Add(Scale 0f)
            eid
        |> Join.update2
        |> Join.over world
    )
    |> ignore
    
    world.On<Update>(
        fun e struct (Scale scale, _: FSharpLogo) ->
            let time = (float32 e.DeltaTime.TotalSeconds)
            updateLogoScale scale time
        |> Join.update2
        |> Join.over world
    )
    |> ignore

    world.On<Update>(
        fun e struct (Rotate rot, _: FSharpLogo) ->
            let time = (float32 e.DeltaTime.TotalSeconds)
            updateLogoRot rot time
        |> Join.update2
        |> Join.over world
    )
    |> ignore

    world.On<Draw>(
        fun e struct (rot: Rotate, scale: Scale, pos: Translate, logo: FSharpLogo) ->
            drawLogo e.SpriteBatch logo pos rot scale
        |> Join.iter4
        |> Join.over world
    )
    |> ignore