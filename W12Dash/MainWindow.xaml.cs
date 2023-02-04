using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.Win32;
using W12Dash.iRacingSDK;

namespace W12Dash
{
    public partial class MainWindow
    {
        private readonly Connection _connection = new Connection();
        private readonly Timer _overlayTimer = new Timer();
        private Brush _white;
        private Brush _dimGray;
        private Brush _orangeRed;
        private Brush _deepSkyBlue;
        private Brush _transparent;
        private Brush _lawnGreen;
        private Brush _darkRed;
        private Brush _limeGreen;
        private Brush _customBlue;
        private Brush _customBlack;
        private Brush _customGreen;
        private RegistryKey _key;
        private double _opacity = 0.5;
        private double _opacityLast = 0.5;
        private double _scale = 1.0;
        private double _scaleLast = 1.0;
        private double _defaultHeight, _defaultWidth;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public MainWindow()
        {
            InitializeComponent();
            Topmost = true;
            lblExit.Visibility = Visibility.Hidden;
            lblMinimize.Visibility = Visibility.Hidden;
            
            Task.Run(Update);
        }

        private void Update()
        {
            var bc = new BrushConverter();
            _white = (Brush)bc.ConvertFrom("White");
            _dimGray = (Brush)bc.ConvertFrom("DimGray");
            _orangeRed = (Brush)bc.ConvertFrom("OrangeRed");
            _deepSkyBlue = (Brush)bc.ConvertFrom("DeepSkyBlue");
            _transparent = (Brush)bc.ConvertFrom("Transparent");
            _lawnGreen = (Brush)bc.ConvertFrom("LawnGreen");
            _darkRed = (Brush)bc.ConvertFrom("DarkRed");
            _limeGreen = (Brush)bc.ConvertFrom("LimeGreen");
            _customBlue = (Brush)bc.ConvertFrom("#FF1C49BF");
            _customBlue?.Freeze();
            _customBlack = (Brush)bc.ConvertFrom("#FF0A0A0A");
            _customBlack?.Freeze();
            _customGreen = (Brush)bc.ConvertFrom("#FF1EA757");
            _customGreen?.Freeze();

            var data = _connection.QueryData();

            var fuelLast = 0.0f;
            var fuelDelta = 0.0f;
            var laps = 0;
            var onTrackLast = false;
            var deployRemain = 4;
            var deployLast = Translator.DeployMode(0.0f);
            var bbLast = data.Get<float>("dcBrakeBias");
            var bbMigLast = data.Get<float>("dcPeakBrakeBias");
            var diffEntryLast = data.Get<float>("dcDiffEntry");
            var diffMiddleLast = data.Get<float>("dcDiffMiddle");
            var diffExitLast = data.Get<float>("dcDiffExit");
            var engBrkLast = data.Get<float>("dcEngineBraking");

            var token = _cancellationTokenSource.Token;
            
            while (!token.IsCancellationRequested)
            {
                data = _connection.QueryData();

                var onTrack = data.Get<bool>("IsOnTrack");
                var brake = data.Get<float>("Brake");
                var brakeScaled = brake > 0.5f ? 2 * brake - 1 : 0;
                var gear = Translator.Gear(data.Get<int>("Gear"));
                var deploy = Translator.DeployMode(data.Get<float>("dcMGUKDeployMode"));
                var battery = (int)(data.Get<float>("EnergyERSBattery") / 40000);
                var rpm = (int)(data.Get<float>("RPM"));
                var drs = data.Get<int>("DRS_Status");
                var pitLim = data.Get<bool>("dcPitSpeedLimiterToggle");
                var llt = Translator.LapTime(data.Get<float>("LapLastLapTime"));
                var fuel = data.Get<float>("FuelLevel") / 1.333333f;
                var deltaSession = data.Get<float>("LapDeltaToSessionBestLap");
                var deltaOptimal = data.Get<float>("LapDeltaToSessionOptimalLap");
                var deltaBestLap = deltaSession != 0 ? deltaSession : deltaOptimal;
                var delta = Translator.Delta(deltaBestLap);
                var lapsRemain = data.Get<int>("SessionLapsRemainEx");
                var tar = 0 < lapsRemain && lapsRemain < 0x7fff ? fuel / lapsRemain : 0;
                var lastLapTime = data.Get<float>("LapLastLapTime");
                var lapsCompleted = data.Get<int>("LapCompleted");
                var speed = data.Get<float>("Speed");
                var bb = data.Get<float>("dcBrakeBias");
                var bbFine = data.Get<float>("dcBrakeBiasFine");
                var bbMig = data.Get<float>("dcPeakBrakeBias");
                var bbAll = $"{bb + bbFine + (bbMig - 1.0f) * brakeScaled:0.0}".Replace(",", ".");
                var diffEntry = data.Get<float>("dcDiffEntry");
                var diffMiddle = data.Get<float>("dcDiffMiddle");
                var diffExit = data.Get<float>("dcDiffExit");
                var engBrk = data.Get<float>("dcEngineBraking");


                if (lapsCompleted > laps)
                {
                    if (lastLapTime > 0)
                        fuelDelta = fuelLast - fuel;
                    
                    if (fuelDelta < 0 || lastLapTime < 0)
                        fuelDelta = 0.0f;

                    deployRemain = 4;

                    laps = lapsCompleted;
                    fuelLast = fuel;
                }

                if (speed > 21.4 && deploy != deployLast)
                    deployRemain--;

                if (deployRemain < 0)
                    deployRemain = 0;

                deployLast = deploy;

                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    switch (onTrack)
                    {
                        case false when onTrackLast:
                            WindowState = WindowState.Minimized;
                            break;
                        case true when !onTrackLast:
                            WindowState = WindowState.Normal;
                            break;
                    }

                    onTrackLast = onTrack;

                    if (Math.Abs(bb - bbLast) > TOLERANCE)
                    {
                        bbLast = bb;
                        StartOverlay(bbAll, null, _limeGreen, _customBlack);
                    }

                    if (Math.Abs(bbMig - bbMigLast) > TOLERANCE)
                        bbMigLast = (float)StartOverlay(bbMig, "Brk MIG", _customGreen, _white);

                    if (Math.Abs(diffEntry - diffEntryLast) > TOLERANCE)
                        diffEntryLast = (float)StartOverlay(diffEntry, "Diff Entry", _customBlue, _white);

                    if (Math.Abs(diffMiddle - diffMiddleLast) > TOLERANCE)
                        diffMiddleLast = (float)StartOverlay(diffMiddle, "Diff Mid", _customBlue, _white);

                    if (Math.Abs(diffExit - diffExitLast) > TOLERANCE)
                        diffExitLast = (float)StartOverlay(diffExit, "Diff Hispd", _customBlue, _white);

                    if (Math.Abs(engBrk - engBrkLast) > TOLERANCE)
                        engBrkLast = (float)StartOverlay(engBrk, "Eng Brk", _customBlue, _white);

                    overlay.Visibility = _overlayTimer.Q ? Visibility.Hidden : Visibility.Visible;

                    lblPitLim.Visibility = pitLim ? Visibility.Visible : Visibility.Hidden;
                    lblGear.Content = gear;
                    lblDeploy.Content = deploy;
                    lblBattery.Content = battery;
                    lblBrakeBias.Content = bbAll;
                    lblLast.Content = llt;
                    lblLL.Content = $"{fuelLast:0.00}".Replace(",", ".");
                    lblTar.Content = $"{tar:0.00}".Replace(",", ".");
                    lblLap.Content = laps;
                    lblDeployRemain.Content = deployRemain;
                    lblDelta.Content = delta;
                    lblDelta.Foreground = deltaBestLap > 0 ? _darkRed : _limeGreen;

                    if (pitLim)
                    {
                        RevLightsGreen(false);
                        RevLightsRed(false);
                        RevLightsBlue(false);

                        RevLightGreen(led_5, true);
                        RevLightRed(led_8, true);
                        RevLightBlue(led_11, true);
                        return;
                    }

                    RevLightGreen(led_1, drs > 0);
                    RevLightGreen(led_2, drs > 1);
                    RevLightGreen(led_3, drs > 2);
                    RevLightGreen(led_4, drs > 2);
                    RevLightGreen(led_5, false);

                    switch (gear)
                    {
                        case "R":
                            RevLightRed(led_6, rpm > 4900);
                            RevLightRed(led_7, rpm > 6100);
                            RevLightRed(led_8, rpm > 7300);
                            RevLightRed(led_9, rpm > 8500);
                            RevLightRed(led_10, rpm > 9300);
                            RevLightsBlue(rpm > 10100);
                            break;
                        case "N":
                            RevLightsRed(false);
                            RevLightsBlue(false);
                            break;
                        case "1":
                            RevLightRed(led_6, rpm > 4900);
                            RevLightRed(led_7, rpm > 6100);
                            RevLightRed(led_8, rpm > 7300);
                            RevLightRed(led_9, rpm > 8500);
                            RevLightRed(led_10, rpm > 9300);
                            RevLightsBlue(rpm > 10100);
                            break;
                        case "2":
                            RevLightRed(led_6, rpm > 4400);
                            RevLightRed(led_7, rpm > 5600);
                            RevLightRed(led_8, rpm > 7000);
                            RevLightRed(led_9, rpm > 8200);
                            RevLightRed(led_10, rpm > 9100);
                            RevLightsBlue(rpm > 10000);
                            break;
                        case "3":
                            RevLightRed(led_6, rpm > 5600);
                            RevLightRed(led_7, rpm > 6700);
                            RevLightRed(led_8, rpm > 7700);
                            RevLightRed(led_9, rpm > 8800);
                            RevLightRed(led_10, rpm > 9500);
                            RevLightsBlue(rpm > 10200);
                            break;
                        case "4":
                            RevLightRed(led_6, rpm > 10200);
                            RevLightRed(led_7, rpm > 10500);
                            RevLightRed(led_8, rpm > 10800);
                            RevLightRed(led_9, rpm > 11000);
                            RevLightRed(led_10, rpm > 11300);
                            RevLightsBlue(rpm > 11500);
                            break;
                        case "5":
                            RevLightRed(led_6, rpm > 10500);
                            RevLightRed(led_7, rpm > 10800);
                            RevLightRed(led_8, rpm > 11000);
                            RevLightRed(led_9, rpm > 11300);
                            RevLightRed(led_10, rpm > 11500);
                            RevLightsBlue(rpm > 11700);
                            break;
                        case "6":
                            RevLightRed(led_6, rpm > 10800);
                            RevLightRed(led_7, rpm > 11000);
                            RevLightRed(led_8, rpm > 11200);
                            RevLightRed(led_9, rpm > 11400);
                            RevLightRed(led_10, rpm > 11500);
                            RevLightsBlue(rpm > 11600);
                            break;
                        case "7":
                            RevLightRed(led_6, rpm > 11400);
                            RevLightRed(led_7, rpm > 11450);
                            RevLightRed(led_8, rpm > 11500);
                            RevLightRed(led_9, rpm > 11540);
                            RevLightRed(led_10, rpm > 11570);
                            RevLightsBlue(rpm > 11600);
                            break;
                        case "8":
                            RevLightsRed(false);
                            RevLightsBlue(false);
                            break;
                    }
                }));
            }
        }

        private const double TOLERANCE = 0.000001;

        private object StartOverlay(object value, object text, Brush background, Brush foreground)
        {
            lblOverlay.Content = value;
            lblOverlay.Background = background;
            lblOverlay.Foreground = foreground;
            lblOverlayTxt.Content = text;
            lblOverlayTxt.Background = background;
            lblOverlayTxt.Foreground = foreground;
            _overlayTimer.Start(1500);
            return value;
        }

        private void RevLightGreen(Shape led, bool on)
        {
            if (on)
            {
                led.Stroke = _lawnGreen;
                led.Fill = _lawnGreen;
                led.Opacity = 1.0;
            }
            else
            {
                led.Stroke = _transparent;
                led.Fill = _dimGray;
                led.Opacity = 0.5;
            }
        }

        private void RevLightRed(Shape led, bool on)
        {
            if (on)
            {
                led.Stroke = _orangeRed;
                led.Fill = _orangeRed;
                led.Opacity = 1.0;
            }
            else
            {
                led.Stroke = _transparent;
                led.Fill = _dimGray;
                led.Opacity = 0.5;
            }
        }

        private void RevLightBlue(Shape led, bool on)
        {
            if (on)
            {
                led.Stroke = _deepSkyBlue;
                led.Fill = _deepSkyBlue;
                led.Opacity = 1.0;
            }
            else
            {
                led.Stroke = _transparent;
                led.Fill = _dimGray;
                led.Opacity = 0.5;
            }
        }

        private void RevLightsGreen(bool on)
        {
            RevLightBlue(led_1, on);
            RevLightBlue(led_2, on);
            RevLightBlue(led_3, on);
            RevLightBlue(led_4, on);
            RevLightBlue(led_5, on);
        }

        private void RevLightsRed(bool on)
        {
            RevLightBlue(led_6, on);
            RevLightBlue(led_7, on);
            RevLightBlue(led_8, on);
            RevLightBlue(led_9, on);
            RevLightBlue(led_10, on);
        }

        private void RevLightsBlue(bool on)
        {
            RevLightBlue(led_11, on);
            RevLightBlue(led_12, on);
            RevLightBlue(led_13, on);
            RevLightBlue(led_14, on);
            RevLightBlue(led_15, on);
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private bool _overButton;

        private void Rectangle_MouseEnter(object sender, MouseEventArgs e)
        {
            lblExit.Visibility = Visibility.Visible;
            lblMinimize.Visibility = Visibility.Visible;
        }

        private void Rectangle_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_overButton) return;
            lblExit.Visibility = Visibility.Hidden;
            lblMinimize.Visibility = Visibility.Hidden;
        }

        private void LblExit_MouseEnter(object sender, MouseEventArgs e)
        {
            lblExit.Visibility = Visibility.Visible;
            lblMinimize.Visibility = Visibility.Visible;
            lblExit.FontWeight = FontWeights.Bold;
            _overButton = true;
        }

        private void LblExit_MouseLeave(object sender, MouseEventArgs e)
        {
            lblExit.Visibility = Visibility.Hidden;
            lblMinimize.Visibility = Visibility.Hidden;
            lblExit.FontWeight = FontWeights.Normal;
            _overButton = false;
        }

        private void LblExit_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) Close();
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            _defaultHeight = Height;
            _defaultWidth = Width;

            _key = Registry.CurrentUser.OpenSubKey("Software", true);

            if (_key?.OpenSubKey("W12Dash") == null)
            {
                _key?.CreateSubKey("W12Dash");
                _key = _key?.OpenSubKey("W12Dash", true);

                _key?.SetValue("Left", BitConverter.GetBytes(Left));
                _key?.SetValue("Top", BitConverter.GetBytes(Left));
                _key?.SetValue("Opacity", BitConverter.GetBytes(border.Background.Opacity));
                _key?.SetValue("Scale", BitConverter.GetBytes(_scale));

                return;
            }

            _key = _key.OpenSubKey("W12Dash", true);
            Left = BitConverter.ToDouble((byte[])_key?.GetValue("Left") ?? Array.Empty<byte>(), 0);
            Top = BitConverter.ToDouble((byte[])_key?.GetValue("Top") ?? Array.Empty<byte>(), 0);
            _opacity = _opacityLast = BitConverter.ToDouble((byte[])_key?.GetValue("Opacity") ?? Array.Empty<byte>(), 0);
            _scale = _scaleLast = BitConverter.ToDouble((byte[])_key?.GetValue("Scale") ?? Array.Empty<byte>(), 0);

            border.Background.Opacity = _opacity;

            var scaleTransform = new ScaleTransform(_scale, _scale);
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(scaleTransform);
            border.RenderTransform = transformGroup;

            if (!(_scale > 1.0)) return;
            Height *= _scale;
            Width *= _scale;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _key.SetValue("Left", BitConverter.GetBytes(Left));
            _key.SetValue("Top", BitConverter.GetBytes(Top));
            _key.SetValue("Opacity", BitConverter.GetBytes(_opacity));
            _key.SetValue("Scale", BitConverter.GetBytes(_scale));
        }

        private void LblMinimize_MouseEnter(object sender, MouseEventArgs e)
        {
            lblExit.Visibility = Visibility.Visible;
            lblMinimize.Visibility = Visibility.Visible;
            lblMinimize.FontWeight = FontWeights.Bold;
            _overButton = true;
        }

        private void LblMinimize_MouseLeave(object sender, MouseEventArgs e)
        {
            lblExit.Visibility = Visibility.Hidden;
            lblMinimize.Visibility = Visibility.Hidden;
            lblMinimize.FontWeight = FontWeights.Normal;
            _overButton = false;
        }

        private void LblMinimize_MouseUp(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.S) || Keyboard.IsKeyDown(Key.O)) return;
            lblSettings.Visibility = Visibility.Hidden;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    if (Keyboard.IsKeyDown(Key.O))
                        _opacity += 0.01;
                    if (Keyboard.IsKeyDown(Key.S))
                        _scale += 0.01;
                    break;
                case Key.Down:
                    if (Keyboard.IsKeyDown(Key.O))
                        _opacity -= 0.01;
                    if (Keyboard.IsKeyDown(Key.S))
                        _scale -= 0.01;
                    break;
            }

            if (Math.Abs(_opacity - _opacityLast) > TOLERANCE)
            {
                if (_opacity > 1.0) _opacity = 1.0;
                if (_opacity < 0.01) _opacity = 0.01;
                
                _opacityLast = _opacity;

                border.Background.Opacity = _opacity;
                lblSettings.Visibility = Visibility.Visible;
                lblSettings.Content = $"Opacity {(int)(_opacity * 100.0)}%";
                return;
            }

            if (Math.Abs(_scale - _scaleLast) < TOLERANCE) return;

            if (_scale > 5.0) _scale = 5.0;
            if (_scale < 0.5) _scale = 0.5;

            var scaleTransform = new ScaleTransform(_scale, _scale);
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(scaleTransform);
            border.RenderTransform = transformGroup;

            if (_scale > 1.0)
            {
                Height = _defaultHeight * _scale;
                Width = _defaultWidth * _scale;
            }

            _scaleLast = _scale;

            lblSettings.Visibility = Visibility.Visible;
            lblSettings.Content = $"Scale {(int)(_scale * 100.0)}%";
        }
    }
}
