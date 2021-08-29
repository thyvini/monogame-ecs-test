module GameLogic

open Garnet.Composition
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open Events
open Logo
open Player
open Ball

let configureWorld (world: Container) =

    configureLogo world
    configureBall world
    configurePlayer world

    // quit game system
    world.On<Update>
        (fun e ->
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back = ButtonState.Pressed
                || Keyboard.GetState().IsKeyDown(Keys.Escape)) then
                e.Game.Exit())
    |> ignore

    world
