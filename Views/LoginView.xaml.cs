using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClientSideChatApp.Views
{
    public partial class LoginView : UserControl
    {
        private Border[,] _tiles;
        private int _rows = 25;
        private int _cols = 40;
        private double _time = 0;

        public LoginView()
        {
            InitializeComponent();
            Loaded += LoginView_Loaded;
            Unloaded += LoginView_Unloaded;
        }

        private void LoginView_Loaded(object sender, RoutedEventArgs e)
        {
            BuildTileSystem();
            CompositionTarget.Rendering += OnRenderTick;
        }

        private void LoginView_Unloaded(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering -= OnRenderTick;
        }

        private void BuildTileSystem()
        {
            TileGrid.Rows = _rows;
            TileGrid.Columns = _cols;
            _tiles = new Border[_rows, _cols];

            for (int y = 0; y < _rows; y++)
            {
                for (int x = 0; x < _cols; x++)
                {
                    var tile = new Border
                    {
                        // Deep, dark slate to contrast the neon
                        Background = new SolidColorBrush(Color.FromRgb(20, 20, 24)),
                        Margin = new Thickness(1.5),
                        RenderTransformOrigin = new Point(0.5, 0.5),
                        RenderTransform = new ScaleTransform(1, 1)
                    };

                    _tiles[y, x] = tile;
                    TileGrid.Children.Add(tile);
                }
            }
        }

        private void OnRenderTick(object sender, EventArgs e)
        {
            _time += 0.03; // Wave speed

            for (int y = 0; y < _rows; y++)
            {
                for (int x = 0; x < _cols; x++)
                {
                    // Pattern mapping: create a sweeping diagonal direction by adding X and Y
                    double wave = Math.Sin((x * 0.2) + (y * 0.3) - _time);

                    // Map the sine wave (-1 to 1) to physical scale (0.5 to 1.0)
                    double scale = 0.75 + (wave * 0.25);

                    var transform = (ScaleTransform)_tiles[y, x].RenderTransform;
                    transform.ScaleX = scale;
                    transform.ScaleY = scale;

                    // As the tile shrinks, drop its opacity slightly to let the neon bleed through more heavily
                    _tiles[y, x].Opacity = 0.6 + (scale * 0.4);
                }
            }
        }
    }
}