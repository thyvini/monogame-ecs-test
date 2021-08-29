module Score

open Components
open Events
open Garnet.Composition
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Types

let private fontSize = 64f

let private createScore index = { Player = index; Value = 0uy }

let private createText (game: Game) index =
    let width =
        float32 game.GraphicsDevice.Viewport.Width

    let position =
        match index with
        | P1 -> Vector2(width / 4f - fontSize / 2f, 50f)
        | P2 -> Vector2(width * 3f / 4f - fontSize / 2f, 50f)
        
    let spriteFont = game.Content.Load<SpriteFont>("sourcecodepro64")
    
    { SpriteFont = spriteFont
      Position = position }

let configureScore (world: Container) =

    world.On
        (fun (LoadContent game) ->
            world
                .Create()
                .With(createScore P1)
                .With(createText game P1)
            |> ignore

            world
                .Create()
                .With(createScore P2)
                .With(createText game P2)
            |> ignore)
    |> ignore

    world.On<ScoreIncrease>(
        fun (e: ScoreIncrease) (score: Score) ->
            match e.PlayerIndex with
            | x when x = score.Player ->
                { score with Value = score.Value + 1uy }
            | _ -> score
        |> Join.update1
        |> Join.over world
    )
    |> ignore

    world.On<Draw>(
        fun e struct (score: Score, text: GameText) ->
            e.SpriteBatch.DrawString(text.SpriteFont, score.Value |> string, text.Position, Color.Black)
        |> Join.iter2
        |> Join.over world
    )
    |> ignore
