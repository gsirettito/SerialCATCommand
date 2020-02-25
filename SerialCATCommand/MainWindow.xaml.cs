using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SerialCATCommand {
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private SerialPort sp;
        private DateTime startTime;
        private bool isReady = true;
        private string sendc;
        public MainWindow() {
            InitializeComponent();
            List<string> _comp = new List<string>();
            _comp.Add("None");
            _comp.AddRange(SerialPort.GetPortNames());
            comports.ItemsSource = _comp;
            comports.SelectedIndex = 0;
            comports.SelectionChanged += comports_SelectionChanged;
            isReady = true;
            this.Loaded += MainWindow_Loaded;
            dial.MouseUp += Dial_MouseUp;
        }

        private void Dial_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (!IsPower) return;
            ZZFA(dial.Value * 1E+6);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            dial.Value = 0.0;
        }

        private void CATCommandSend(string catc) {
            if (sp != null && sp.IsOpen) {
                dispCat.Text = catc;
                sendc = catc;
                sp.WriteLine(catc);
            }
        }

        private void ZZFA(double freq) {
            DateTime now = DateTime.Now;
            if (now - startTime > TimeSpan.FromMilliseconds(10)) isReady = true;
            if (!isReady) return;
            string value = string.Format("{0:N3}", freq).Replace(".", "").Replace(",", "");
            while (value.Length < 11) value = value.Insert(0, "0");
            isReady = false;
            CATCommandSend(string.Format("ZZFA{0};", value));
            dial.ValueChanged -= slider_ValueChanged;
            disp.Text = String.Format("{0:N3}", freq * 1E-6).Replace(",", ".");
            dial.ValueChanged += slider_ValueChanged;

            if (freq >= 1.800E+6 && freq <= 1.850E+6) (bandRA.Children[0] as RadioButton).IsChecked = true;
            if (freq >= 3.500E+6 && freq <= 4.000E+6) (bandRA.Children[1] as RadioButton).IsChecked = true;
            if (freq >= 7.000E+6 && freq <= 7.300E+6) (bandRA.Children[2] as RadioButton).IsChecked = true;
            if (freq >= 10.100E+6 && freq <= 10.150E+6) (bandRA.Children[3] as RadioButton).IsChecked = true;
            if (freq >= 14.000E+6 && freq <= 14.350E+6) (bandRA.Children[4] as RadioButton).IsChecked = true;
            if (freq >= 18.068E+6 && freq <= 18.168E+6) (bandRA.Children[5] as RadioButton).IsChecked = true;
            if (freq >= 21.000E+6 && freq <= 21.450E+6) (bandRA.Children[6] as RadioButton).IsChecked = true;
            if (freq >= 24.890E+6 && freq <= 24.990E+6) (bandRA.Children[7] as RadioButton).IsChecked = true;
            if (freq >= 28.000E+6 && freq <= 29.700E+6) (bandRA.Children[8] as RadioButton).IsChecked = true;
        }

        private void comports_DropDownOpened(object sender, EventArgs e) {
            List<string> _comp = new List<string>();
            _comp.Add("None");
            _comp.AddRange(SerialPort.GetPortNames());
            comports.ItemsSource = _comp;
            comports.SelectedIndex = 0;
        }

        void comports_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
        }

        void sp_DataReceived(object sender, SerialDataReceivedEventArgs e) {
            try {
                string read = sp.ReadLine().Substring(4, 11);
                if (read == sendc.Substring(4, 11))
                    isReady = true;
            } catch { }
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (!IsPower) return;
            if (DateTime.Now - startTime < TimeSpan.FromMilliseconds(40)) return;
            startTime = DateTime.Now;
            Thread.Sleep(10);
            ZZFA(dial.Value * 1E+6);
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e) {
            RadioButton rb = (RadioButton)sender;
            if (IsPower)
                switch (rb.Content.ToString()) {
                    case "160m": ZZFA(1.825E+6); break;
                    case "80m": ZZFA(3.750E+6); break;
                    case "40m": ZZFA(7.150E+6); break;
                    case "30m": ZZFA(10.125E+6); break;
                    case "20m": ZZFA(14.175E+6); break;
                    case "17m": ZZFA(18.118E+6); break;
                    case "15m": ZZFA(21.225E+6); break;
                    case "12m": ZZFA(24.940E+6); break;
                    case "10m": ZZFA(28.850E+6); break;
                }
        }

        private void SerialConnect() {
            if (comports.SelectedValue == null || combaud.SelectedValue == null || comports.SelectedIndex == 0) {
                if (sp != null && sp.IsOpen) sp.Close();
                return;
            }
            if (sp != null && sp.IsOpen) sp.Close();
            sp = new SerialPort(comports.SelectedValue.ToString());
            sp.BaudRate = Convert.ToInt32(combaud.SelectedValue.ToString());
            sp.DataReceived += sp_DataReceived;
            try {
                sp.Open();
                sp.Write("r");
                Thread.Sleep(100);
                ZZFA(dial.Value * 1E+6);
            } catch (Exception ex) {
                comports.SelectedIndex = 0;
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SerialDisconnect() {
            if (sp != null && sp.IsOpen) sp.Close();
        }

        private void _power_Checked(object sender, RoutedEventArgs e) {
            IsPower = true;
            SerialConnect();
            screen.Background = new LinearGradientBrush() {
                EndPoint = new Point(1, 0.5), StartPoint = new Point(0, 0.5)
            };
            (screen.Background as LinearGradientBrush).GradientStops = new GradientStopCollection(){
                new GradientStop(FromHtml("#FFFBD610"), 1),
                new GradientStop(FromHtml("#FFFEC53D"), 0.047),
                new GradientStop(FromHtml("#FFFBD610"), 0),
                new GradientStop(FromHtml("#FFFDAC20"), 0.937),
            };
        }

        public bool IsPower { get; set; }

        private void _power_Unchecked(object sender, RoutedEventArgs e) {
            IsPower = false;
            SerialDisconnect();
            screen.Background = new LinearGradientBrush() {
                EndPoint = new Point(1, 0.5), StartPoint = new Point(0, 0.5)
            };
            (screen.Background as LinearGradientBrush).GradientStops = new GradientStopCollection(){
                new GradientStop(FromHtml("#FF366C3E"), 1),
                new GradientStop(FromHtml("#FF3E5B42"), 0.047),
                new GradientStop(FromHtml("#FF366C3E"), 0),
                new GradientStop(FromHtml("#FF3A6140"), 0.937),
            };
        }

        private Color FromHtml(string htmlcolor) {
            if (string.IsNullOrEmpty(htmlcolor)) return Colors.Black;
            if (htmlcolor[0] == '#' && htmlcolor.Length == 9) {
                byte A = Convert.ToByte(htmlcolor.Substring(1, 2), 16);
                byte R = Convert.ToByte(htmlcolor.Substring(3, 2), 16);
                byte G = Convert.ToByte(htmlcolor.Substring(5, 2), 16);
                byte B = Convert.ToByte(htmlcolor.Substring(7, 2), 16);
                return Color.FromArgb(A, R, G, B);
            }
            return Colors.Black;
        }
    }
}
