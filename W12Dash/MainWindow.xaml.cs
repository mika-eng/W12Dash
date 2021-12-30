using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Threading;
using Microsoft.Win32;

namespace W12Dash
{
    public partial class MainWindow : Window
    {
        BackgroundWorker worker = new BackgroundWorker();
        iRacingConnection connection = new iRacingConnection();
        Brush dimGray;
        Brush orangeRed;
        Brush deepSkyBlue;
        Brush transparent;
        Brush lawnGreen;
        Brush darkRed;
        Brush limeGreen;
        RegistryKey key;
        double opacity = 0.5;
        double opacity_last = 0.5;
        double scale = 1.0;
        double scale_last = 1.0;
        double defaultHeigth, defaultWidth;

        public MainWindow()
        {
            InitializeComponent();
            this.Topmost = true;
            lblExit.Visibility = Visibility.Hidden;
            lblMinimize.Visibility = Visibility.Hidden;

            worker.DoWork += Worker_DoWork;
            worker.RunWorkerAsync();           
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BrushConverter bc = new BrushConverter();
            dimGray = (Brush)bc.ConvertFrom("DimGray");
            orangeRed = (Brush)bc.ConvertFrom("OrangeRed");
            deepSkyBlue = (Brush)bc.ConvertFrom("DeepSkyBlue");
            transparent = (Brush)bc.ConvertFrom("Transparent");
            lawnGreen = (Brush)bc.ConvertFrom("LawnGreen");
            darkRed = (Brush)bc.ConvertFrom("DarkRed");
            limeGreen = (Brush)bc.ConvertFrom("LimeGreen");

            float fuel_last = 0.0f;
            float fuel_delta = 0.0f;
            int laps = 0;
            bool onTrack_last = false;
            int deployRemain = 4;
            var deployLast = Translator.MGUKDeployMode(0.0f);

            while (true)
            {
                var data = connection.QueryData();

                var onTrack = data.Get<bool>("IsOnTrack");
                var gear = Translator.Gear(data.Get<int>("Gear"));
                var deploy = Translator.MGUKDeployMode(data.Get<float>("dcMGUKDeployMode"));
                var batt = (int)(data.Get<float>("EnergyERSBattery") / 40000);
                var bb = data.Get<float>("dcBrakeBias") + data.Get<float>("dcBrakeBiasFine");
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

                if (lapsCompleted > laps)
                {
                    if (lastLapTime > 0)
                        fuel_delta = fuel_last - fuel;
                    
                    if (fuel_delta < 0 || lastLapTime < 0)
                        fuel_delta = 0.0f;

                    deployRemain = 4;

                    laps = lapsCompleted;
                    fuel_last = fuel;
                }

                if (speed > 21.4 && deploy != deployLast)
                    deployRemain--;

                if (deployRemain < 0)
                    deployRemain = 0;

                deployLast = deploy;

                this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    if (!onTrack && onTrack_last)
                        this.WindowState = WindowState.Minimized;

                    if (onTrack && !onTrack_last)
                        this.WindowState = WindowState.Normal;

                    onTrack_last = onTrack;

                    lblPitLim.Visibility = pitLim ? Visibility.Visible : Visibility.Hidden;
                    lblGear.Content = gear;
                    lblDeploy.Content = deploy;
                    lblBattery.Content = batt;
                    lblBrakeBias.Content = $"{bb:0.0}".Replace(",", ".");
                    lblLast.Content = llt;
                    lblLL.Content = $"{fuel_delta:0.00}".Replace(",", ".");
                    lblTar.Content = $"{tar:0.00}".Replace(",", ".");
                    lblLap.Content = laps;
                    lblDeployRemain.Content = deployRemain;
                    lblDelta.Content = delta;

                    if (deltaBestLap > 0)
                        lblDelta.Foreground = darkRed;
                    else
                        lblDelta.Foreground = limeGreen;

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

        private void RevLightGreen(Ellipse led, bool on)
        {
            if (on)
            {
                led.Stroke = lawnGreen;
                led.Fill = lawnGreen;
                led.Opacity = 1.0;
            }
            else
            {
                led.Stroke = transparent;
                led.Fill = dimGray;
                led.Opacity = 0.5;
            }
        }

        private void RevLightRed(Ellipse led, bool on)
        {
            if (on)
            {
                led.Stroke = orangeRed;
                led.Fill = orangeRed;
                led.Opacity = 1.0;
            }
            else
            {
                led.Stroke = transparent;
                led.Fill = dimGray;
                led.Opacity = 0.5;
            }
        }

        private void RevLightBlue(Ellipse led, bool on)
        {
            if (on)
            {
                led.Stroke = deepSkyBlue;
                led.Fill = deepSkyBlue;
                led.Opacity = 1.0;
            }
            else
            {
                led.Stroke = transparent;
                led.Fill = dimGray;
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
                this.DragMove();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        bool overButton;

        private void Rectangle_MouseEnter(object sender, MouseEventArgs e)
        {
            lblExit.Visibility = Visibility.Visible;
            lblMinimize.Visibility = Visibility.Visible;
        }

        private void Rectangle_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!overButton)
            {
                lblExit.Visibility = Visibility.Hidden;
                lblMinimize.Visibility = Visibility.Hidden;
            }
        }

        private void LblExit_MouseEnter(object sender, MouseEventArgs e)
        {
            lblExit.Visibility = Visibility.Visible;
            lblMinimize.Visibility = Visibility.Visible;
            lblExit.FontWeight = FontWeights.Bold;
            overButton = true;
        }

        private void LblExit_MouseLeave(object sender, MouseEventArgs e)
        {
            lblExit.Visibility = Visibility.Hidden;
            lblMinimize.Visibility = Visibility.Hidden;
            lblExit.FontWeight = FontWeights.Normal;
            overButton = false;
        }

        private void LblExit_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.Close();
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            defaultHeigth = this.Height;
            defaultWidth = this.Width;

            key = Registry.CurrentUser.OpenSubKey("Software", true);

            if (key.OpenSubKey("W12Dash") == null)
            {
                key.CreateSubKey("W12Dash");
                key = key.OpenSubKey("W12Dash", true);

                key.SetValue("Left", BitConverter.GetBytes(this.Left));
                key.SetValue("Top", BitConverter.GetBytes(this.Left));
                key.SetValue("Opacity", BitConverter.GetBytes(this.border.Background.Opacity));
                key.SetValue("Scale", BitConverter.GetBytes(scale));

                return;
            }

            key = key.OpenSubKey("W12Dash", true);
            this.Left = BitConverter.ToDouble((byte[])key.GetValue("Left"), 0);
            this.Top = BitConverter.ToDouble((byte[])key.GetValue("Top"), 0);
            opacity = opacity_last = BitConverter.ToDouble((byte[])key.GetValue("Opacity"), 0);
            scale = scale_last = BitConverter.ToDouble((byte[])key.GetValue("Scale"), 0);

            this.border.Background.Opacity = opacity;

            ScaleTransform scaleTransform = new ScaleTransform(scale, scale);
            TransformGroup transformGroup = new TransformGroup();
            transformGroup.Children.Add(scaleTransform);
            border.RenderTransform = transformGroup;

            if (scale > 1.0)
            {
                this.Height *= scale;
                this.Width *= scale;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            key.SetValue("Left", BitConverter.GetBytes(this.Left));
            key.SetValue("Top", BitConverter.GetBytes(this.Top));
            key.SetValue("Opacity", BitConverter.GetBytes(opacity));
            key.SetValue("Scale", BitConverter.GetBytes(scale));
        }

        private void LblMinimize_MouseEnter(object sender, MouseEventArgs e)
        {
            lblExit.Visibility = Visibility.Visible;
            lblMinimize.Visibility = Visibility.Visible;
            lblMinimize.FontWeight = FontWeights.Bold;
            overButton = true;
        }

        private void LblMinimize_MouseLeave(object sender, MouseEventArgs e)
        {
            lblExit.Visibility = Visibility.Hidden;
            lblMinimize.Visibility = Visibility.Hidden;
            lblMinimize.FontWeight = FontWeights.Normal;
            overButton = false;
        }

        private void LblMinimize_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.S) || Keyboard.IsKeyDown(Key.O))
                return;
            lblSettings.Visibility = Visibility.Hidden;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    if (Keyboard.IsKeyDown(Key.O))
                        opacity += 0.01;
                    if (Keyboard.IsKeyDown(Key.S))
                        scale += 0.01;
                    break;
                case Key.Down:
                    if (Keyboard.IsKeyDown(Key.O))
                        opacity -= 0.01;
                    if (Keyboard.IsKeyDown(Key.S))
                        scale -= 0.01;
                    break;
            }

            if (opacity != opacity_last)
            {
                if (opacity > 1.0)
                    opacity = 1.0;

                if (opacity < 0.01)
                    opacity = 0.01;

                opacity_last = opacity;

                this.border.Background.Opacity = opacity;
                lblSettings.Visibility = Visibility.Visible;
                lblSettings.Content = $"Opacity {(int)(opacity * 100.0)}%";
                return;
            }

            if (scale == scale_last)
                return;

            if (scale > 5.0)
                scale = 5.0;

            if (scale < 0.5)
                scale = 0.5;

            ScaleTransform scaleTransform = new ScaleTransform(scale, scale);
            TransformGroup transformGroup = new TransformGroup();
            transformGroup.Children.Add(scaleTransform);
            border.RenderTransform = transformGroup;

            if (scale > 1.0)
            {
                this.Height = defaultHeigth * scale;
                this.Width = defaultWidth * scale;
            }

            scale_last = scale;

            lblSettings.Visibility = Visibility.Visible;
            lblSettings.Content = $"Scale {(int)(scale * 100.0)}%";
        }
    }
}
