﻿namespace MyGame

open Garnet.Composition
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input

open Events


type Game1() as this =
    inherit Game()

    let graphics = new GraphicsDeviceManager(this)
    let mutable spriteBatch = Unchecked.defaultof<_>

    let world =
        Container() |> GameLogic.configureWorld

    do this.Content.RootDirectory <- "Content"
       this.IsMouseVisible <- true

    override this.LoadContent() =
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)
        world.Run (LoadContent this)

    override this.Initialize() =
        graphics.PreferredBackBufferWidth <- 1024
        graphics.PreferredBackBufferHeight <- 768
        graphics.ToggleFullScreen()
        graphics.ApplyChanges()
        base.Initialize()
        world.Run (Start this)

    override this.Update gameTime =
        world.Run { DeltaTime = gameTime.ElapsedGameTime; Game = this }
        base.Update gameTime

    override this.Draw gameTime =
        this.GraphicsDevice.Clear Color.Gray
        spriteBatch.Begin()
        world.Run { Time = gameTime.ElapsedGameTime
                    SpriteBatch = spriteBatch }
        spriteBatch.End()

        base.Draw(gameTime)
