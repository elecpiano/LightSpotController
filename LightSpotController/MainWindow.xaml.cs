using Animation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LightSpotController
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region lifecycle

        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region window state

        //WindowState="Maximized" WindowStyle="None"


        #endregion

        #region keyboard event

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                MoveSpeed = 10;
            }
            else
            {
                MoveSpeed = 1;
            }

            if (e.Key == Key.Escape)
            {
                this.Close();
            }
            else if (e.Key == Key.Left)
            {
                MoveSpot(-1,0);
            }
            else if (e.Key == Key.Right)
            {
                MoveSpot(1, 0);
            }
            else if (e.Key == Key.Up)
            {
                MoveSpot(0,-1);
            }
            else if (e.Key == Key.Down)
            {
                MoveSpot(0,1);
            }
            else if (e.Key == Key.Oem4)
            {
                ScaletSpot(-2);
            }
            else if (e.Key == Key.Oem6)
            {
                ScaletSpot(2);
            }
            //else if (e.Key == Key.OemMinus)
            //{
            //    UpdateDuration(-0.1);
            //}
            //else if (e.Key == Key.OemPlus)
            //{
            //    UpdateDuration(0.1);
            //}
            else if (e.Key == Key.Enter)
            {
                GenerateScript();
            }
            else if (e.Key == Key.Back)
            {
                DeleteScript();
            }
            else if (e.Key == Key.Space)
            {
                PausePlay();
            }
            else if (e.Key == Key.H)
            {
                SetMode();
            }
        }

        #endregion

        #region spot position / scale

        double LightSpotSize = 100;
        double LightSpotX = 0;
        double LightSpotY = 0;
        double MoveSpeed = 1;

        private void ResetSpot()
        {
            LightSpot.SetValue(Canvas.LeftProperty, 0.0);
            LightSpot.SetValue(Canvas.TopProperty, 0.0);
            //LightSpot.Width = LightSpot.Height = 100.0;
            LightSpotScaleTransform.ScaleX = 1.0;
            LightSpotScaleTransform.ScaleY = 1.0;

            LightSpot.UpdateLayout();
        }

        private void MoveSpot(double delta_x, double delta_y)
        {
            if (IsEditMode ==  false)
                return;

            LightSpotX += delta_x * MoveSpeed;
            LightSpotY += delta_y * MoveSpeed;

            LightSpot.SetValue(Canvas.LeftProperty, LightSpotX);
            LightSpot.SetValue(Canvas.TopProperty, LightSpotY);
        }

        private void ScaletSpot(double delta)
        {
            if (IsEditMode == false)
                return;

            if ((LightSpotSize + delta * MoveSpeed) < 8)
            {
                return;
            }
            LightSpotSize += delta * MoveSpeed;
            //LightSpot.Width = LightSpot.Height = LightSpotSize;
            var scaleFactor = LightSpotSize / 100.0;
            LightSpotScaleTransform.ScaleX = scaleFactor;
            LightSpotScaleTransform.ScaleY = scaleFactor;
            //LightSpot.Margin = new Thickness(0 - LightSpotSize * 0.5);
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsEditMode == false)
                return;

            var point = e.GetPosition(this.SpotCanvas);

            LightSpotX = point.X;
            LightSpotY = point.Y;

            LightSpot.SetValue(Canvas.LeftProperty, LightSpotX);
            LightSpot.SetValue(Canvas.TopProperty, LightSpotY);
        }

        #endregion

        #region duration

        double SpotDuration = 1.0; // unit in 0.1 second

        private void UpdateDuration(double delta)
        {
            if (IsEditMode == false)
                return;

            if ((SpotDuration + delta * MoveSpeed) < 0.1)
            {
                return;
            }
            SpotDuration += delta * MoveSpeed;
            SpotDuration = Math.Round(SpotDuration, 1);

            DurationText.Text = SpotDuration.ToString();
        }

        #endregion

        #region script

        string Script = "";
        List<string> ScriptList = new List<string>();
        List<string> DurationList = new List<string>();

        private void GenerateScript()
        {
            if (IsEditMode == false)
                return;

            SpotDuration = Player.Position.TotalSeconds - SpotDuration - MOVE_DURATION.TotalSeconds;

            string newLine = "";

            newLine += LightSpotX.ToString() + ",";
            newLine += LightSpotY.ToString() + ",";
            newLine += LightSpotSize.ToString() + ",";
            newLine += SpotDuration.ToString() + ",";
            newLine += System.Environment.NewLine;

            ScriptList.Add(newLine);
            PreviewScript();
        }

        private void DeleteScript()
        {
            if (IsEditMode == false)
                return;

            if (ScriptList.Count == 0)
            {
                return;
            }

            ScriptList.RemoveAt(ScriptList.Count - 1);
            PreviewScript();
        }

        private void PreviewScript()
        {
            string script = "";
            foreach (var line in ScriptList)
            {
                script += line;
            }
            ScriptPreviewText.Text = script;
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "TXT file (*.txt)|*.txt";
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                StreamReader sr = new StreamReader(ofd.FileName, Encoding.Default);
                ScriptPreviewText.Text = Script = sr.ReadToEnd();

                ScriptList.Clear();
                string[] separator = { System.Environment.NewLine };
                var lines = Script.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    ScriptList.Add(line);
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Script = "";
            foreach (var line in ScriptList)
            {
                Script += line;
            }

            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.Filter = "TXT File(*.txt)|*.txt";
            sfd.RestoreDirectory = true;
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FileStream fs = new FileStream(sfd.FileName, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(Script);
                sw.Flush();
                sw.Close();
                fs.Close();
            }

        }

        #endregion

        #region playback

        MoveAnimation Mover = new MoveAnimation(0);
        ScaleAnimation Scaler = new ScaleAnimation(0);
        FadeAnimation Fader = new FadeAnimation();
        FadeAnimation Fader2 = new FadeAnimation();
        Stack<string> ScriptQueue = null;
        TimeSpan MOVE_DURATION = TimeSpan.FromSeconds(0.8);
        TimeSpan FADE_DURATION = TimeSpan.FromSeconds(0.4);

        SineEase Ease = new SineEase() { EasingMode = EasingMode.EaseOut };
        bool IsPlaying = false;

        private void PausePlay()
        {
            IsPlaying = !IsPlaying;

            if (IsEditMode)
            {
                if (IsPlaying)
                {
                    if (Player != null)
                        Player.Play();
                }
                else
                {
                    if (Player != null)
                        Player.Pause();
                }
            }
            else
            {
                if (IsPlaying)
                {
                    if (Player != null)
                        Player.Play();
                    PlayScript();
                }
                else
                {
                    if (Player != null)
                        Player.Stop();
                    StopScript();
                }
            }
        }

        private void PlayScript()
        {
            ResetSpot();

            ScriptQueue = new Stack<string>();
            int index = ScriptList.Count - 1;
            while (index >= 0)
            {
                ScriptQueue.Push(ScriptList[index]);
                index--;
            }

            PlayNextScript();
            //StartTick();
        }

        private void PlayNextScript()
        {
            if (ScriptList.Count == 0)
            {
                return;
            }

            if (ScriptQueue.Count == 0)
            {
                PlayScript();
                return;
            }

            string line = ScriptQueue.Pop();

            int index = 0;
            var arg_1 = line.Substring(index, line.IndexOf(",", index) - index);
            index += arg_1.Length + 1;
            var arg_2 = line.Substring(index, line.IndexOf(",", index) - index);
            index += arg_2.Length + 1;
            var arg_3 = line.Substring(index, line.IndexOf(",", index) - index);
            index += arg_3.Length + 1;
            var arg_4 = line.Substring(index, line.IndexOf(",", index) - index);

            LightSpotX = Double.Parse(arg_1);
            LightSpotY = Double.Parse(arg_2);
            LightSpotSize = Double.Parse(arg_3);
            SpotDuration = Double.Parse(arg_4);

            PlayMove();
        }

        private void StopScript()
        {
            ScriptQueue.Clear();
            Mover.Stop();
            Scaler.Stop();
            Fader.Stop();
            Fader2.Stop();

            ResetSpot();
        }

        private void PlayMove()
        {
            DurationText.Text = "";

            Mover.InstanceMoveTo(LightSpot, LightSpotX, LightSpotY, MOVE_DURATION, Ease, 
                (element)=> PlayHover());

            var scaleFactor = LightSpotSize / 100.0;
            Scaler.InstanceScaleTo(LightSpot2, scaleFactor, scaleFactor, MOVE_DURATION, null);

            Fader.InstanceFade(LightSpot, 1.0, 01, FADE_DURATION, true, null);
        }

        private void PlayHover()
        {
            TimeSpan duration = TimeSpan.FromSeconds(SpotDuration);

            ElapsedTick = 0;
            DurationText.Text = SpotDuration.ToString();

            //Fader2.InstanceFade(LightSpot2, 1.0, 1.0, duration, false,
            //    (e) => PlayNextScript());

            Mover.InstanceMoveTo(LightSpot, LightSpotX, LightSpotY, duration, null,
                (element) => PlayNextScript());
        }

        #endregion

        #region tick

        DispatcherTimer TickTimer = null;
        TimeSpan TICK_INTERVAL = TimeSpan.FromSeconds(0.05);
        double ElapsedTick = 0;

        private void StartTick()
        {
            ElapsedTick = 0;

            if (TickTimer == null)
            {
                TickTimer = new DispatcherTimer();
                TickTimer.Interval = TICK_INTERVAL;
                TickTimer.Tick += TickTimer_Tick;
            }

            if (TickTimer.IsEnabled == false)
            {
                TickTimer.Start();
            }
        }

        private void TickTimer_Tick(object sender, EventArgs e)
        {
            ElapsedTick += 0.1;
            DurationText.Text = ElapsedTick.ToString();
        }

        #endregion

        #region mode

        bool IsEditMode = true;

        private void SetMode()
        {
            IsEditMode = !IsEditMode;

            if (IsEditMode)
            {
                DurationText.Visibility = Visibility.Visible;
                ScriptPreviewText.Visibility = Visibility.Visible;
                Buttons.Visibility = Visibility.Visible;
                Mouse.OverrideCursor = null;
            }
            else
            {
                DurationText.Visibility = Visibility.Hidden;
                ScriptPreviewText.Visibility = Visibility.Hidden;
                Buttons.Visibility = Visibility.Hidden;
                Mouse.OverrideCursor = Cursors.None;
            }

        }

        #endregion

        #region media

        MediaPlayer Player = null;

        private void Media_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "MP3 file (*.mp3)|*.mp3|WAV file (*.wav)|*.wav|All files (*.*)|*.*";
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Player = new MediaPlayer();
                Player.Open(new Uri(ofd.FileName, UriKind.RelativeOrAbsolute));
                Player.MediaEnded += (ss, ee) =>
                {
                    Player.Position = new TimeSpan(0);
                };
            }
        }

        #endregion


    }


}
