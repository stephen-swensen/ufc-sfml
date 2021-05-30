namespace Swensen.SFML.Game

open SFML.System
open SFML.Graphics

module Drawing =

    let drawState assets (window: PollableWindow) state =
        window.Clear()

        use circle =
            new CircleShape(10.0f, FillColor = Color.Green, Position = state.Player.Position)

        window.Draw(circle)

        let hudPos =
            Vector2f(
                0f,
                ((snd >> float32) state.WindowDimensions)
                - (float32 state.HudHeight)
            )

        use hud =
            new RectangleShape(
                Vector2f((fst >> float32) state.WindowDimensions, float32 state.HudHeight),
                FillColor = Color.Cyan,
                Position = hudPos
            )

        window.Draw(hud)

        use hudText = new SFML.Graphics.Text()

        do
            hudText.Font <- assets.Fonts.DejaVuSansMono
            hudText.DisplayedString <- sprintf $"Wall Crossings: %u{state.WallCrossings}"
            hudText.CharacterSize <- 30u
            hudText.Position <- hudPos
            hudText.FillColor <- Color.Black

        window.Draw(hudText)
        window.Display()