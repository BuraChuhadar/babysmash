using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Speech.Synthesis;
using System.Windows.Media.Animation;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Input;

namespace Baby_Smash
{
    public partial class MainWindow : Window
    {
        private Random random = new Random();
        private bool isDarkModeEnabled = false;
        private double currentShapeSize; // Field to store the size of the current shape
        private int fadeSpeed = 5;  // Default fade speed

        public MainWindow()
        {
            InitializeComponent();
            this.MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;
            this.MouseMove += MainWindow_MouseMove;
            this.KeyDown += MainWindow_KeyDown;
            this.KeyUp += MainWindow_KeyUp;
            // Load settings
            isDarkModeEnabled = Properties.Settings.Default.IsDarkModeEnabled;
            fadeSpeed = Properties.Settings.Default.FadeSpeed;
            UpdateTheme();

            SetFullScreen();
        }

        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            isKeyDown = false;
        }

        private bool isKeyDown = false;

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (isKeyDown)
            {
                return; // Do not create a new shape if a key is already held down
            }

            isKeyDown = true;

            if (e.Key == Key.LWin || e.Key == Key.RWin)
            {
                e.Handled = true; // Prevents system key actions like Windows Start menu
            }

            Shape newShape = CreateRandomShape();
            Canvas shapeContainer = new Canvas();
            shapeContainer.Width = newShape.Width;
            shapeContainer.Height = newShape.Height;

            MainCanvas.Children.Add(shapeContainer);
            shapeContainer.Children.Add(newShape);

            PositionShapeRandomly(shapeContainer, newShape);
            AddFaceToShape(shapeContainer, newShape);
            AnnounceShape(newShape);
            ApplyFadeAnimation(shapeContainer);
        }

        private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Shape shape = CreateRandomShape();
            Canvas shapeContainer = new Canvas();
            shapeContainer.Width = shape.Width;
            shapeContainer.Height = shape.Height;

            MainCanvas.Children.Add(shapeContainer);
            shapeContainer.Children.Add(shape);

            Point clickPosition = e.GetPosition(this.MainCanvas);
            Canvas.SetLeft(shapeContainer, clickPosition.X - shape.Width / 2);
            Canvas.SetTop(shapeContainer, clickPosition.Y - shape.Height / 2);

            AddFaceToShape(shapeContainer, shape);
            AnnounceShape(shape);
            ApplyFadeAnimation(shapeContainer);
        }

        private void PositionShapeRandomly(Canvas shapeContainer, Shape shape)
        {
            Random random = new Random();
            double maxX = MainCanvas.ActualWidth - shapeContainer.Width;
            double maxY = MainCanvas.ActualHeight - shapeContainer.Height;

            double randomX = random.NextDouble() * Math.Max(1, maxX);
            double randomY = random.NextDouble() * Math.Max(1, maxY);

            Canvas.SetLeft(shapeContainer, randomX);
            Canvas.SetTop(shapeContainer, randomY);
        }

        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(this.MainCanvas);
            foreach (Canvas shapeCanvas in MainCanvas.Children.OfType<Canvas>())
            {
                foreach (Canvas eyeCanvas in shapeCanvas.Children.OfType<Canvas>().Where(c => c.Tag != null && c.Tag.ToString() == "Eye"))
                {
                    Ellipse pupil = eyeCanvas.Children.OfType<Ellipse>().FirstOrDefault(p => p.Tag != null && p.Tag.ToString() == "Pupil");
                    if (pupil != null)
                    {
                        Point localMousePos = e.GetPosition(eyeCanvas);
                        double eyeRadius = eyeCanvas.Width / 2;
                        double maxOffset = eyeRadius - pupil.Width / 2;
                        Point eyeCenter = new Point(eyeRadius, eyeRadius);  // center of the eyeCanvas
                        Vector direction = Point.Subtract(localMousePos, eyeCenter);
                        if (direction.Length > maxOffset)
                        {
                            direction.Normalize();
                            direction *= maxOffset;
                        }
                        Canvas.SetLeft(pupil, eyeRadius + direction.X - pupil.Width / 2);
                        Canvas.SetTop(pupil, eyeRadius + direction.Y - pupil.Height / 2);
                    }
                }
            }
        }

        private void SetFullScreen()
        {
            this.WindowState = WindowState.Normal;
            this.WindowStyle = WindowStyle.None;
            this.Topmost = true;
            this.Left = SystemParameters.VirtualScreenLeft;
            this.Top = SystemParameters.VirtualScreenTop;
            this.Width = SystemParameters.VirtualScreenWidth;
            this.Height = SystemParameters.VirtualScreenHeight;
        }

        private void ApplyFadeAnimation(Canvas shapeContainer)
        {
            DoubleAnimation fadeAnimation = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromSeconds(11 - fadeSpeed)));
            fadeAnimation.Completed += (s, _) => MainCanvas.Children.Remove(shapeContainer);
            shapeContainer.BeginAnimation(UIElement.OpacityProperty, fadeAnimation);
        }

        private Shape CreateRandomShape()
        {
            currentShapeSize = random.Next(30, 201); // Sizes between 30 and 200
            Shape shape;
            int shapeType = random.Next(7); // Increase range to accommodate more shapes
            Brush randomBrush = GetRandomGradientBrush();
            switch (shapeType)
            {
                case 0:
                    shape = new Rectangle { Width = currentShapeSize, Height = currentShapeSize, Fill = randomBrush };
                    break;
                case 1:
                    shape = new Ellipse { Width = currentShapeSize, Height = currentShapeSize, Fill = randomBrush };
                    break;
                case 2:
                    double width = currentShapeSize * Math.Sqrt(3); // Equilateral triangle
                    double height = currentShapeSize;
                    shape = new Polygon
                    {
                        Points = new PointCollection {
                            new Point(0, height),
                            new Point(width / 2, 0),
                            new Point(width, height)
                        },
                        Fill = randomBrush,
                        Width = width,
                        Height = height
                    };
                    break;
                case 3:
                    shape = CreateStar(currentShapeSize, randomBrush);
                    break;
                case 4:
                    shape = CreatePolygon(6, currentShapeSize, randomBrush); // Hexagon
                    break;
                case 5:
                    shape = CreatePolygon(5, currentShapeSize, randomBrush); // Pentagon
                    break;
                case 6:
                    shape = CreateHeart(currentShapeSize);
                    shape.Fill = randomBrush;
                    break;
                default:
                    shape = new Rectangle { Width = currentShapeSize, Height = currentShapeSize, Fill = randomBrush }; // Default case
                    break;
            }
            return shape;
        }

        private Brush GetRandomGradientBrush()
        {
            GradientStopCollection gradientStops = new GradientStopCollection();
            gradientStops.Add(new GradientStop(GetRandomColor(), 0.0));
            gradientStops.Add(new GradientStop(GetRandomColor(), 1.0));
            LinearGradientBrush gradientBrush = new LinearGradientBrush(gradientStops);
            gradientBrush.StartPoint = new Point(0, 0);
            gradientBrush.EndPoint = new Point(1, 1);
            return gradientBrush;
        }

        private Color GetRandomColor()
        {
            byte[] colorBytes = new byte[3];
            random.NextBytes(colorBytes);
            return Color.FromRgb(colorBytes[0], colorBytes[1], colorBytes[2]);
        }

        private Canvas CreateEye(double eyeRadius)
        {
            Canvas eyeCanvas = new Canvas
            {
                Width = eyeRadius * 2,
                Height = eyeRadius * 2,
                Tag = "Eye"  // Correctly identifies the canvas as the container of an eye
            };

            Ellipse eye = new Ellipse
            {
                Width = eyeRadius * 2,
                Height = eyeRadius * 2,
                Fill = Brushes.White,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            eyeCanvas.Children.Add(eye);
            Canvas.SetTop(eye, 0);
            Canvas.SetLeft(eye, 0);

            Ellipse pupil = new Ellipse
            {
                Width = eyeRadius * 0.6,
                Height = eyeRadius * 0.6,
                Fill = Brushes.Black,
                Tag = "Pupil"  // Correctly identifies the pupil
            };
            eyeCanvas.Children.Add(pupil);
            // Center the pupil more accurately
            Canvas.SetTop(pupil, (eyeRadius - pupil.Height) / 2);
            Canvas.SetLeft(pupil, (eyeRadius - pupil.Width) / 2);

            return eyeCanvas;
        }

        private void AddFaceToShape(Canvas shapeContainer, Shape shape)
        {
            double faceWidth = shape.Width;
            double faceHeight = shape.Height;
            double eyeRadius = faceWidth * 0.1; // size of the eye
            double eyeSpacing = faceWidth * 0.5; // distance between eyes
            double mouthWidth = faceWidth * 0.6; // making the mouth wider for better visibility
            double mouthOffsetY = faceHeight * 0.7; // adjust this if mouth overlaps with eyes

            // Create eyes and position them
            Canvas leftEyeCanvas = CreateEye(eyeRadius);
            Canvas rightEyeCanvas = CreateEye(eyeRadius);
            shapeContainer.Children.Add(leftEyeCanvas);
            shapeContainer.Children.Add(rightEyeCanvas);

            // Positioning eyes relatively
            Canvas.SetLeft(leftEyeCanvas, faceWidth / 2 - eyeSpacing / 2 - eyeRadius);
            Canvas.SetTop(leftEyeCanvas, faceHeight * 0.2);
            Canvas.SetLeft(rightEyeCanvas, faceWidth / 2 + eyeSpacing / 2 - eyeRadius);
            Canvas.SetTop(rightEyeCanvas, faceHeight * 0.2);

            // Create and add the mouth
            Path mouth = CreateMouth(mouthWidth);
            shapeContainer.Children.Add(mouth);
            Canvas.SetLeft(mouth, (faceWidth - mouthWidth) / 2);  // Center the mouth horizontally
            Canvas.SetTop(mouth, mouthOffsetY - mouth.Height / 2);  // Adjust the vertical position to avoid overlap
        }

        private Path CreateMouth(double width)
        {
            // Adjusting the height of the curve to be more pronounced
            double mouthHeight = width * 0.3;  // Increase the height for better visibility

            // Path data defines a quadratic Bezier curve for the mouth
            // Control point adjustment to make the smile wider and more pronounced
            string pathData = $"M 0,0 Q {width / 2},{mouthHeight} {width},0";

            Path mouth = new Path
            {
                Data = Geometry.Parse(pathData),
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                Fill = Brushes.Red,
                Width = width,
                Height = mouthHeight
            };

            // The mouth is translated to be placed correctly below the eyes
            TranslateTransform translate = new TranslateTransform(0, mouthHeight / 2);
            mouth.RenderTransform = translate;

            return mouth;
        }

        private Polygon CreateStar(double size, Brush fill)
        {
            PointCollection points = new PointCollection();
            double outerRadius = size / 2;
            double innerRadius = outerRadius / 2.5; // Adjust to change the inner size of the star
            double angle = -Math.PI / 2;

            for (int i = 0; i < 5; i++)
            {
                // Outer point
                points.Add(new Point(
                    outerRadius + outerRadius * Math.Cos(angle),
                    outerRadius + outerRadius * Math.Sin(angle)
                ));
                angle += Math.PI / 5;

                // Inner point
                points.Add(new Point(
                    outerRadius + innerRadius * Math.Cos(angle),
                    outerRadius + innerRadius * Math.Sin(angle)
                ));
                angle += Math.PI / 5;
            }

            return new Polygon
            {
                Points = points,
                Fill = fill,
                Width = size,
                Height = size
            };
        }

        private Polygon CreatePolygon(int sides, double size, Brush fill)
        {
            PointCollection points = new PointCollection();
            double radius = size / 2;
            double angleStep = 2 * Math.PI / sides;
            double angle = -Math.PI / 2;

            for (int i = 0; i < sides; i++)
            {
                points.Add(new Point(
                    radius + radius * Math.Cos(angle),
                    radius + radius * Math.Sin(angle)
                ));
                angle += angleStep;
            }

            return new Polygon
            {
                Points = points,
                Fill = fill,
                Width = size,
                Height = size
            };
        }

        private Path CreateHeart(double size)
        {
            // Define the path data for the heart shape
            string pathData = "M 0.5,0.3 C 0.5,0.1 0.7,0 0.9,0.2 C 1.2,0.5 1,0.9 0.5,1.2 C 0,0.9 -0.2,0.5 0.1,0.2 C 0.3,0 0.5,0.1 0.5,0.3 Z";

            // Create a Path object
            Path heart = new Path
            {
                Data = Geometry.Parse(pathData),
                Stretch = Stretch.Fill,
                Width = size,
                Height = size,
                Tag = "Heart"
            };

            return heart;
        }

        private async Task AnnounceShape(Shape shape)
        {
            string shapeName = shape.GetType().Name;

            if (shape is Ellipse && shape.Width == shape.Height)
            {
                shapeName = "Circle";
            }
            else if (shape is Rectangle && shape.Width == shape.Height)
            {
                shapeName = "Square";
            }
            else if (shape is Rectangle)
            {
                shapeName = "Rectangle";
            }
            else if (shape is Polygon polygon)
            {
                switch (polygon.Points.Count)
                {
                    case 3:
                        shapeName = "Triangle";
                        break;
                    case 5:
                        shapeName = "Pentagon";
                        break;
                    case 6:
                        shapeName = "Hexagon";
                        break;
                    case 10:
                        shapeName = "Star";
                        break;
                    default:
                        shapeName = "Polygon";
                        break;
                }
            }
            else if (shape is Path path && path.Tag != null && path.Tag.ToString() == "Heart")
            {
                shapeName = "Heart";
            }
            else
            {
                shapeName = "Unknown Shape";
            }

            await Task.Run(() =>
            {
                using (var synthesizer = new SpeechSynthesizer())
                {
                    synthesizer.SelectVoiceByHints(VoiceGender.Neutral);
                    synthesizer.Speak($"{shapeName.ToLower()}.");
                }
            });
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var settingsDialog = new SettingsDialog(isDarkModeEnabled, fadeSpeed);
            settingsDialog.Owner = this;
            if (settingsDialog.ShowDialog() == true)
            {
                isDarkModeEnabled = settingsDialog.DarkMode;
                fadeSpeed = settingsDialog.FadeSpeed;
                UpdateTheme();
            }
        }

        private void UpdateTheme()
        {
            MainCanvas.Background = isDarkModeEnabled ? Brushes.Black : Brushes.White;
        }
    }
}

