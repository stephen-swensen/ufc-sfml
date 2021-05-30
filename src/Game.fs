namespace Swensen.SFML.Game

// originally adapted from xcvd's question at https://stackoverflow.com/questions/22072603/f-game-development-modifying-state-variables/22076855#22076855

open SFML.System
open SFML.Window

type World = PollableWindow * GameState * InputCommands

module Game =

    ///Create the World with a BIG BANG
    let bang () =
        let state =
            { Player = { Position = Vector2f(0f, 0f) }
              WindowDimensions = (800u, 600u)
              HudHeight = 60u
              WallCrossings = 0u }

        let windowWidth, windowHeight = state.WindowDimensions

        let window =
            let window =
                new PollableWindow(new VideoMode(windowWidth, windowHeight), "Stephen's first game!")
            //https://www.sfml-dev.org/tutorials/2.5/window-window.php#controlling-the-framerate
            //per docs "Never use both setVerticalSyncEnabled and setFramerateLimit at the same time! They would badly mix and make things worse."
            //window.SetVerticalSyncEnabled(true)
            window.SetFramerateLimit(60u)
            window

        let commands =
            { ChangeDirection = None
              CloseWindow = false }

        window, state, commands

    [<EntryPoint>]
    let main args =
        let assets = Assets.load ()

        let rec loop ((window, state, commands): World) =
            if not window.IsOpen then
                ()
            else
                let commands = Input.pollEvents window commands
                let state = State.update commands state

                if commands.CloseWindow then
                    window.Dispose()
                else
                    Drawing.drawState assets window state
                    loop (window, state, commands)

        loop (bang ())
        0