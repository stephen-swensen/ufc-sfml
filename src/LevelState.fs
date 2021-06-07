namespace Swensen.SFML.Game

open System
open SFML.System
open SFML.Graphics

module private StateHelpers =
    let boxPosToCenterPos (v: Vector2f) r =
        let x = v.X + r
        let y = v.Y + r
        Vector2f(x, y)

open StateHelpers

type Player =
    { ///The position of the top left corner of the bounding box
      Position: Vector2f
      Radius: float32
      Color: Color }
    ///The position of the center of the circle
    member this.CenterPosition =
        boxPosToCenterPos this.Position this.Radius

type Enemy =
    { ///The position of the top left corner of the bounding box
      Position: Vector2f
      Radius: float32
      AliveColor: Color
      EatenColor: Color
      Eaten: bool
      Direction: Direction option }
    ///The position of the center of the circle
    member this.CenterPosition =
        boxPosToCenterPos this.Position this.Radius

type LevelState =
    { Player: Player
      Enemies: Enemy list
      WallCrossings: uint
      EnemyCount: int
      ElapsedMs: int64 }

module LevelState =

    ///Calc new position from old position and directional movement
    ///(new pos, true|false wrapped around window)
    let calcNewPosition (pos: Vector2f) direction (x, y) =
        let moveUnit = 4f

        let pos' =
            match direction with
            | Up -> Vector2f(pos.X, pos.Y - moveUnit)
            | Left -> Vector2f(pos.X - moveUnit, pos.Y)
            | Down -> Vector2f(pos.X, pos.Y + 4f)
            | Right -> Vector2f(pos.X + moveUnit, pos.Y)

        let pos'' =
            Vector2f(pos'.X %% (float32 x), pos'.Y %% (float32 y))

        pos'', pos' <> pos''

    //check if the two (circles) intersect https://stackoverflow.com/a/8367547/236255
    let checkCollision (player: Player) (enemy: Enemy) =
        if enemy.Radius <= 0f then
            false
        else
            let r' = pown (player.Radius - enemy.Radius) 2
            let r'' = pown (player.Radius + enemy.Radius) 2

            let d' =
                (pown (player.CenterPosition.X - enemy.CenterPosition.X) 2)
                + (pown (player.CenterPosition.Y - enemy.CenterPosition.Y) 2)

            r' <= d' && d' <= r''

    let update (boardDimensions: uint * uint) commands state =
        let pos = state.Player.Position

        let state =
            match commands.ChangeDirection with
            | Some direction ->
                let pos, wrapped =
                    calcNewPosition pos direction boardDimensions

                { state with
                      Player = { state.Player with Position = pos }
                      WallCrossings =
                          if wrapped then
                              state.WallCrossings + 1u
                          else
                              state.WallCrossings }
            | None -> state

        let enemies =
            state.Enemies
            //move enemies and reduce radius if needed
            |> Seq.map
                (fun e ->
                    match e.Eaten, e.Direction with
                    | true, _
                    | _, None -> e
                    | _, Some (direction) ->
                        let pos, _ =
                            calcNewPosition e.Position direction boardDimensions

                        let radius =
                            e.Radius
                            - ((float32 state.ElapsedMs) / 1_000_000f)

                        { e with
                              Position = pos
                              Radius = radius })
            //check collisions with player
            |> Seq.map
                (fun e ->
                    if e.Eaten then
                        e
                    else
                        let collision = checkCollision state.Player e
                        { e with Eaten = collision })
            |> Seq.filter (fun e -> e.Radius > 0f)
            |> Seq.toList

        { state with Enemies = enemies }

    let genEnemies (directions: ((Direction option) list)) (rnd: unit -> int) count (x, y) =
        let radius = 20f
        let x = x - ((uint radius) * 2u)
        let y = y - ((uint radius) * 2u)

        let genDirection () =
            let i = rnd () %% directions.Length
            directions.[i]

        let genRandomCoord c = ((rnd () |> uint) %% c) |> float32

        [ for _ in 1 .. count do
              //n.b. circles are drawn from top left corner of bounding box
              let pos =
                  Vector2f(genRandomCoord x, genRandomCoord y)

              { Position = pos
                AliveColor = Color.Red
                EatenColor = Color.Blue
                Eaten = false
                Radius = radius
                Direction = genDirection () } ]

    let init rnd boardDimensions (level: Level) =

        let state =
            { Player =
                  { Position = Vector2f(0f, 0f)
                    Color = Color.Green
                    Radius = 10f }
              WallCrossings = 0u
              Enemies = []
              EnemyCount = level.EnemyCount
              ElapsedMs = 0L }

        let state =
            { state with
                  Enemies = genEnemies (level.EnemyDirections) rnd state.EnemyCount boardDimensions }

        state