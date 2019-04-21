using Animation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Timers;
using System.Windows.Threading;

namespace LightSpotController
{
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
                AddScriptLine();
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

        #region script

        string Script = "";
        List<ScriptLine> ScriptList = new List<ScriptLine>();

        private void AddScriptLine()
        {
            if (IsEditMode == false)
                return;

            ScriptLine newLine = new ScriptLine();

            newLine.X = LightSpotX;
            newLine.Y = LightSpotY;
            newLine.Size = LightSpotSize;
            newLine.Time = Media.Position.TotalSeconds;

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
                script += line.X.ToString() + ",";
                script += line.Y.ToString() + ",";
                script += line.Size.ToString() + ",";
                script += line.Time.ToString() + ",";
                script += System.Environment.NewLine;
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
                var lineStrArray = Script.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                foreach (var lineStr in lineStrArray)
                {
                    var line = new ScriptLine();

                    int index = 0;
                    var arg_1 = lineStr.Substring(index, lineStr.IndexOf(",", index) - index);
                    index += arg_1.Length + 1;
                    var arg_2 = lineStr.Substring(index, lineStr.IndexOf(",", index) - index);
                    index += arg_2.Length + 1;
                    var arg_3 = lineStr.Substring(index, lineStr.IndexOf(",", index) - index);
                    index += arg_3.Length + 1;
                    var arg_4 = lineStr.Substring(index, lineStr.IndexOf(",", index) - index);

                    line.X = Double.Parse(arg_1);
                    line.Y = Double.Parse(arg_2);
                    line.Size = Double.Parse(arg_3);
                    line.Time = Double.Parse(arg_4);

                    ScriptList.Add(line);
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Script = "";
            foreach (var line in ScriptList)
            {
                Script += line.X.ToString() + ",";
                Script += line.Y.ToString() + ",";
                Script += line.Size.ToString() + ",";
                Script += line.Time.ToString() + ",";
                Script += System.Environment.NewLine;
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
        Stack<ScriptLine> ScriptQueue = new Stack<ScriptLine>();
        TimeSpan MOVE_DURATION = TimeSpan.FromSeconds(0.6);
        TimeSpan FADE_DURATION_1 = TimeSpan.FromSeconds(0.3);
        TimeSpan FADE_DURATION_2 = TimeSpan.FromSeconds(0.3);

        SineEase Ease = new SineEase() { EasingMode = EasingMode.EaseOut };
        bool IsPlaying = false;

        ScriptLine NextScriptLine = null;

        private void PausePlay()
        {
            IsPlaying = !IsPlaying;

            if (IsEditMode)
            {
                if (IsPlaying)
                {
                    if (Media != null)
                        Media.Play();
                }
                else
                {
                    if (Media != null)
                        Media.Pause();
                }
            }
            else
            {
                if (IsPlaying)
                {
                    if (Media != null)
                        Media.Play();
                    PlayScript();
                }
                else
                {
                    if (Media != null)
                        Media.Stop();
                    StopScript();
                }
            }
        }

        private void PlayScript()
        {
            ResetSpot();

            ScriptQueue.Clear();
            int index = ScriptList.Count - 1;
            while (index >= 0)
            {
                ScriptQueue.Push(ScriptList[index]);
                index--;
            }

            NextScriptLine = ScriptQueue.Pop();

            StartTick();
        }

        private void PlayNextScriptLine()
        {
            if (ScriptList.Count == 0)
            {
                return;
            }

            if (NextScriptLine != null)
            {
                LightSpotX = NextScriptLine.X;
                LightSpotY = NextScriptLine.Y;
                LightSpotSize = NextScriptLine.Size;

                PlayMove();
            }

            if (ScriptQueue.Count > 0)
            {
                NextScriptLine = ScriptQueue.Pop();
            }
            else
            {
                NextScriptLine = null;
                //PlayScript();
            }
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
            Mover.InstanceMoveTo(LightSpot, LightSpotX, LightSpotY, MOVE_DURATION, Ease, null);
                //(element)=> PlayHover());

            var scaleFactor = LightSpotSize / 100.0;
            Scaler.InstanceScaleTo(LightSpot2, scaleFactor, scaleFactor, MOVE_DURATION, null);

            Fader.InstanceFade(LightSpot, 1.0, 0.5, FADE_DURATION_1, false, 
                                            element=> {
                                                Fader.InstanceFade(LightSpot, 0.5, 1.0, FADE_DURATION_2, false, null);
                                            });
        }

        #endregion

        #region tick

        TimeSpan TICK_INTERVAL = TimeSpan.FromSeconds(0.01);
        double TimeElapsed = 0;
        Timer timer = null;
        double OFFSET = 1.0;

        private void StartTick()
        {
            TimeElapsed = 0;

            if (timer == null)
            {
                timer = new Timer();
                timer.Interval = 10;
                timer.Elapsed += Timer_Elapsed;
            }

            if (timer.Enabled ==  false)
            {
                timer.Start();
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            TimeElapsed += 0.01;

            if (NextScriptLine==null)
            {
                return;
            }

            this.Dispatcher.BeginInvoke(new Action( 
                ( ) => 
                    {
                        //debugText.Text = TimeElapsed.ToString();
                        //debugText2.Text = NextScriptLine.Time.ToString();
                        //debugText3.Text = Media.Position.TotalSeconds.ToString();

                        if (Media.Position.TotalSeconds >= (NextScriptLine.Time - OFFSET))
                        {
                            PlayNextScriptLine();
                        }
                    } 
                ), null);
        }

        #endregion

        #region edit mode

        bool IsEditMode = true;

        private void SetMode()
        {
            IsEditMode = !IsEditMode;

            if (IsEditMode)
            {
                ScriptPreviewText.Visibility = Visibility.Visible;
                Buttons.Visibility = Visibility.Visible;
                Mouse.OverrideCursor = null;
            }
            else
            {
                ScriptPreviewText.Visibility = Visibility.Hidden;
                Buttons.Visibility = Visibility.Hidden;
                Mouse.OverrideCursor = Cursors.None;
            }

        }

        #endregion

        #region audio

        MediaPlayer Media = null;

        private void Media_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "WAV file (*.wav)|*.wav|MP3 file (*.mp3)|*.mp3|All files (*.*)|*.*";
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Media = new MediaPlayer();
                Media.Open(new Uri(ofd.FileName, UriKind.RelativeOrAbsolute));
                Media.MediaEnded += (ss, ee) =>
                {
                    Media.Position = new TimeSpan(0);
                    PlayScript();
                };
            }
        }

        #endregion

    }


}
