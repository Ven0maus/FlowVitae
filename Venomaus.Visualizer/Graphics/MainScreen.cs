using SadConsole;
using SadConsole.UI;
using SadConsole.UI.Controls;
using SadRogue.Primitives;
using Venomaus.Visualizer.Core;

namespace Venomaus.Visualizer.Graphics
{
    internal class MainScreen : ControlsConsole
    {
        public MainScreen() : base(Constants.ScreenSettings.Width, Constants.ScreenSettings.Height)
        {
            var titleFragments = @"
____   ____.__                    .__  .__                     
\   \ /   /|__| ________ _______  |  | |__|_______ ___________ 
 \   Y   / |  |/  ___/  |  \__  \ |  | |  \___   // __ \_  __ \
  \     /  |  |\___ \|  |  // __ \|  |_|  |/    /\  ___/|  | \/
   \___/   |__/____  >____/(____  /____/__/_____ \\___  >__|   
                   \/           \/              \/    \/       "
            .Replace("\r", string.Empty).Split('\n');

            int startPosX = (Width / 2) - (titleFragments.OrderByDescending(a => a.Length).First().Length / 2);
            int startPosY = 4;

            // Print title fragments
            for (int y = 0; y < titleFragments.Length; y++)
            {
                for (int x = 0; x < titleFragments[y].Length; x++)
                {
                    Surface.SetGlyph(startPosX + x, startPosY + y, titleFragments[y][x], Color.White, Color.Transparent);
                }
            }

            var staticBtn = new Button(20, 2)
            {
                Position = new Point(Width / 2 - 10, Height / 2 - 1),
                Text = "Static grid",
            };
            staticBtn.Click += StaticBtn_Click;

            var proceduralBtn = new Button(20, 2)
            {
                Position = new Point(Width / 2 - 10, Height / 2 + 3),
                Text = "Procedural grid"
            };
            proceduralBtn.Click += ProceduralBtn_Click;

            var buttons = new [] { staticBtn, proceduralBtn };
            foreach (var button in buttons)
                Controls.Add(button);
        }

        private void ProceduralBtn_Click(object? sender, EventArgs e)
        {
            GameLoop.Instance.InitProceduralGrid();
        }

        private void StaticBtn_Click(object? sender, EventArgs e)
        {
            GameLoop.Instance.InitStaticGrid();
        }
    }
}
