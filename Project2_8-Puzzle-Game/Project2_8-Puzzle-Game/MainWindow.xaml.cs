﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Project2_8_Puzzle_Game
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int startX = 30;
        const int startY = 30;
        const int width = 50;
        const int height = 50;
        const int square_length = 500;
        const int preview_length = 300;
        const int time_allowed = 180;
        Random rng = new Random();

        //Hang so the hien mui ten
        const int LEFT_ARROW = 3000;
        const int RIGHT_ARROW = 3001;
        const int UP_ARROW = 3002;
        const int DOWN_ARROW = 3003;

        //Mảng chứa các ma trận thông tin bức ảnh
        int[,] _numberMatrix;
        Image[,] _croppedImageMatrix;

        //Chiều dài, chiều rộng ảnh đã được chỉnh để chuẩn bị cắt
        int h_cropped = 0;
        int w_cropped = 0;

        //Chiều dài, chiều rộng ảnh preview
        int h_cropped_preview = 0;
        int w_cropped_preview = 0;

        //Padding ở hai chiều của khung hình
        int padding_X = 0;
        int padding_Y = 0;


        //Timer
        DispatcherTimer _timer;
        TimeSpan _time;


        string DefaultFileName = "sample.jpg";



        bool _isPlayable = false; //Biến xác định trò chơi có thể tiếp tục hay không
        bool _isContinue = false; //Biến xác định game có được load từ một game có sẵn hay không

        //Màn hình mở file
        OpenFileDialog screen = new OpenFileDialog();

        public MainWindow()
        {
            InitializeComponent();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _numberMatrix = new int[3, 3];
            _croppedImageMatrix = new Image[3, 3];

            var source = new BitmapImage(
                new Uri("sample.jpg", UriKind.RelativeOrAbsolute));

            var h = (int)source.Height;
            var w = (int)source.Width;


            if (h > w)
            {
                double ratio = h / (double)square_length;
                double ratio_preview = h / (double)preview_length;

                h_cropped = (int)(h / ratio);
                w_cropped = (int)(w / ratio);
                h_cropped_preview = (int)(h / ratio_preview);
                w_cropped_preview = (int)(w / ratio_preview);
                padding_X = (square_length - w_cropped) / 2;
                padding_Y = 0;

            }
            else
            {
                double ratio = w / (double)square_length;
                double ratio_preview = w / (double)preview_length;

                h_cropped = (int)(h / ratio);
                w_cropped = (int)(w / ratio);
                h_cropped_preview = (int)(h / ratio_preview);
                w_cropped_preview = (int)(w / ratio_preview);
                padding_X = 0;
                padding_Y = (square_length - h_cropped) / 2;
            }


            previewImage.Width = w_cropped_preview;
            previewImage.Height = h_cropped_preview;
            previewImage.Source = source;



            // Bat dau cat thanh 9 manh

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (!((i == 2) && (j == 2)))
                    {

                        //Debug.WriteLine($"Len = {len}");
                        var rect = new Int32Rect(j * w / 3, i * h / 3, w / 3, h / 3);
                        var cropBitmap = new CroppedBitmap(source,
                            rect);

                        var cropImage = new Image();
                        cropImage.Stretch = Stretch.Fill;
                        cropImage.Width = w_cropped / 3;
                        cropImage.Height = h_cropped / 3;
                        cropImage.Source = cropBitmap;
                        canvas.Children.Add(cropImage);
                        Canvas.SetLeft(cropImage, startX + padding_X + j * (w_cropped / 3 + 2));
                        Canvas.SetTop(cropImage, startY + padding_Y + i * (h_cropped / 3 + 2));



                        cropImage.MouseLeftButtonDown += CropImage_MouseLeftButtonDown;
                        cropImage.PreviewMouseLeftButtonUp += CropImage_PreviewMouseLeftButtonUp;


                        _croppedImageMatrix[i, j] = cropImage;
                        _numberMatrix[i, j] = 3 * i + j + 1;

                    }
                }
            }
        }

        bool _isDragging = false;
        Image _selectedBitmap = null;
        Point _lastPosition;
        int _iOrigin;
        int _jOrigin;
        private void CropImage_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isPlayable == true)
            {
                _isDragging = false;
                var position = e.GetPosition(this);
                int i = ((int)position.Y - startY - padding_Y) / (h_cropped / 3);
                int j = ((int)position.X - startX - padding_X) / (w_cropped / 3);
                int x, y;
                int dy = Math.Abs(i - _iOrigin);
                int dx = Math.Abs(j - _jOrigin);

                //Back lại chỗ cũ nếu một trong các điều kiện không thỏa
                if (!(((i >= 0 && i < 3 && j >= 0 && j < 3)) && (dx * dy == 0) && (dx + dy == 1) && (_croppedImageMatrix[i,j] == null)))
                {
                    i = _iOrigin;
                    j = _jOrigin;

                }

                x = j * (w_cropped / 3 + 2) + startX + padding_X;
                y = i * (h_cropped / 3 + 2) + startY + padding_Y;
                if (_selectedBitmap != null)
                {
                    Canvas.SetLeft(_selectedBitmap, x);
                    Canvas.SetTop(_selectedBitmap, y);
                }

                int temp;
                temp = _numberMatrix[i, j];
                  
                
                _numberMatrix[i, j] = _numberMatrix[_iOrigin, _jOrigin];
                _numberMatrix[_iOrigin, _jOrigin] = temp;

                if (_croppedImageMatrix[i, j] == null)
                {
                    _croppedImageMatrix[i, j] = _croppedImageMatrix[_iOrigin, _jOrigin];
                    _croppedImageMatrix[_iOrigin, _jOrigin] = null;
                }

                checkWin();
            }
        }

        private void CropImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_isPlayable == true)
            {
                _isDragging = true;
                _selectedBitmap = sender as Image;
                _lastPosition = e.GetPosition(this);
                _iOrigin = ((int)_lastPosition.Y - startY - padding_Y) / (h_cropped / 3);
                _jOrigin = ((int)_lastPosition.X - startX - padding_X) / (w_cropped / 3);
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isPlayable != false)
            {
                var position = e.GetPosition(this);

                int i = ((int)position.Y - startY) / (h_cropped / 3);
                int j = ((int)position.X - startX) / (w_cropped / 3);

                if (_isDragging)
                {
                    var dx = position.X - _lastPosition.X;
                    var dy = position.Y - _lastPosition.Y;

                    var lastLeft = Canvas.GetLeft(_selectedBitmap);
                    var lastTop = Canvas.GetTop(_selectedBitmap);
                    Canvas.SetLeft(_selectedBitmap, lastLeft + dx);
                    Canvas.SetTop(_selectedBitmap, lastTop + dy);

                    _lastPosition = position;
                }
            }
        }

        /// <summary>
        /// Hàm swap ô trống qua hình 
        /// </summary>
        /// <param name="blank_i"></param>
        /// <param name="blank_j"></param>
        /// <param name="img_i"></param>
        /// <param name="img_j"></param>
        private void swapCroppedImage(int blank_i, int blank_j, int img_i, int img_j)
        {
            Image img = _croppedImageMatrix[img_i, img_j];
            Canvas.SetLeft(img, startX + padding_X + blank_j * (w_cropped / 3 + 2));
            Canvas.SetTop(img, startY + padding_Y + blank_i * (h_cropped / 3 + 2));
            int temp = _numberMatrix[blank_i, blank_j];
            _numberMatrix[blank_i, blank_j] = _numberMatrix[img_i, img_j];
            _numberMatrix[img_i, img_j] = temp;

            _croppedImageMatrix[blank_i, blank_j] = _croppedImageMatrix[img_i, img_j];
            _croppedImageMatrix[img_i, img_j] = null;

        }

        private bool movePartialImage(int arrow)
        {
            

            if (arrow == LEFT_ARROW)
            {
                int i = 0;
                int j = 0;
                int flag = 0;

                for (; i < 3; i++)
                {
                    for (j = 0; j < 3; j++)
                    {
                        if (_numberMatrix[i, j] == 0)
                        {
                            flag = 1;
                            break;
                        }
                    }
                    if (flag == 1)
                        break;
                }

                if (j == 2)
                    return false;

                swapCroppedImage(i, j, i, j + 1);
                return true;
            }
            else if (arrow == RIGHT_ARROW)
            {
                int i = 0;
                int j = 0;
                int flag = 0;

                for (; i < 3; i++)
                {
                    for (j = 0; j < 3; j++)
                    {
                        if (_numberMatrix[i, j] == 0)
                        {
                            flag = 1;
                            break;
                        }
                    }
                    if (flag == 1)
                        break;
                }

                if (j == 0)
                    return false;

                swapCroppedImage(i, j, i, j - 1);
                return true;

            }
            else if(arrow == UP_ARROW)
            {
                int i = 0;
                int j = 0;
                int flag = 0;

                for (; i < 3; i++)
                {
                    for (j = 0; j < 3; j++)
                    {
                        if (_numberMatrix[i, j] == 0)
                        {
                            flag = 1;
                            break;
                        }
                    }
                    if (flag == 1)
                        break;
                }

                if (i == 2)
                    return false;

                swapCroppedImage(i, j, i + 1, j);
                return true;

            }
            else
            {
                int i = 0;
                int j = 0;
                int flag = 0;

                for (; i < 3; i++)
                {
                    for (j = 0; j < 3; j++)
                    {
                        if (_numberMatrix[i, j] == 0)
                        {
                            flag = 1;
                            break;
                        }
                    }
                    if (flag == 1)
                        break;
                }

                if (i == 0)
                    return false;

                swapCroppedImage(i, j, i - 1, j);
                return true;

            }

        }

        private void shuffleTheImage()
        {           
            for (int i = 0; i < 1000;)
            {
                int temp = rng.Next(LEFT_ARROW, DOWN_ARROW + 1);
                bool flag = movePartialImage(temp);
                if (flag == true)
                   i++;
                
            }
        }

        /// <summary>
        /// Xử lý khi bấm các mũi tên
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (_isPlayable == true)
            {
                if (e.Key == Key.Left)
                {
                    movePartialImage(LEFT_ARROW);

                }
                else if (e.Key == Key.Right)
                {
                    movePartialImage(RIGHT_ARROW);
                }
                else if (e.Key == Key.Up)
                {
                    movePartialImage(UP_ARROW);


                }
                else if (e.Key == Key.Down)
                {
                    movePartialImage(DOWN_ARROW);

                }
                else
                    return;

                checkWin();
            }
        }


        /// <summary>
        /// Hàm kiểm tra chiến thắng
        /// -1 khi hết giờ
        /// 0 khi còn thời gian và chưa thắng
        /// 1 khi đã thắng
        /// </summary>
        /// <returns></returns>
        private void checkWin()
        {

            if (_numberMatrix[2, 2] != 0)
                return;
                
            for(int i=0;i<3;i++)
            {
                for(int j=0;j<3;j++)
                {
                    if ((_numberMatrix[i,j] != 3*i+j+1) && (3*i+j != 8))
                    {
                        return;
                    }
                }
            }
            _timer.Stop();
            MessageBox.Show("You win!");
            _isPlayable = false;
            isStarted = false;
        }

        private void ChooseImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_timer.IsEnabled == true)
            {
                _timer.Stop();
            }
            if (screen.ShowDialog() == true)
            {
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        canvas.Children.Remove(_croppedImageMatrix[i, j]);
                    }
                }
                ImageChosenHandling();
            }
            else
            {
                _isPlayable = false;
            }
        }

        private void ImageChosenHandling()
        {
            var source = new BitmapImage(
                    new Uri(screen.FileName, UriKind.Absolute));

            var h = (int)source.Height;
            var w = (int)source.Width;


            if (h > w)
            {
                double ratio = h / (double)square_length;
                double ratio_preview = h / (double)preview_length;

                h_cropped = (int)(h / ratio);
                w_cropped = (int)(w / ratio);
                h_cropped_preview = (int)(h / ratio_preview);
                w_cropped_preview = (int)(w / ratio_preview);
                padding_X = (square_length - w_cropped) / 2;
                padding_Y = 0;

            }
            else
            {
                double ratio = w / (double)square_length;
                double ratio_preview = w / (double)preview_length;

                h_cropped = (int)(h / ratio);
                w_cropped = (int)(w / ratio);
                h_cropped_preview = (int)(h / ratio_preview);
                w_cropped_preview = (int)(w / ratio_preview);
                padding_X = (square_length - w_cropped) / 2; 
                padding_Y = (square_length - h_cropped) / 2;
            }


            previewImage.Width = w_cropped_preview;
            previewImage.Height = h_cropped_preview;
            previewImage.Source = source;

            Canvas.SetLeft(previewImage, 776 + 150 - previewImage.Width/2);
            Canvas.SetTop(previewImage, 350 + 150 - previewImage.Height/2);




            // Bat dau cat thanh 9 manh

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (!((i == 2) && (j == 2)))
                    {

                        //Debug.WriteLine($"Len = {len}");
                        var rect = new Int32Rect(j * w / 3, i * h / 3, w / 3, h / 3);
                        var cropBitmap = new CroppedBitmap(source,
                            rect);

                        var cropImage = new Image();
                        cropImage.Stretch = Stretch.Fill;
                        cropImage.Width = w_cropped / 3;
                        cropImage.Height = h_cropped / 3;
                        cropImage.Source = cropBitmap;
                        canvas.Children.Add(cropImage);
                        Canvas.SetLeft(cropImage, startX + padding_X + j * (w_cropped / 3 + 2));
                        Canvas.SetTop(cropImage, startY + padding_Y + i * (h_cropped / 3 + 2));



                        cropImage.MouseLeftButtonDown += CropImage_MouseLeftButtonDown;
                        cropImage.PreviewMouseLeftButtonUp += CropImage_PreviewMouseLeftButtonUp;

                       
                        _croppedImageMatrix[i, j] = cropImage;
                        _numberMatrix[i, j] = 3 * i + j + 1;
            
                    }
                }
            }
            
           
        }

        bool isStarted = false;
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (isStarted == false)
            {
                isStarted = true;
                _isPlayable = true;
                if (_isContinue == false)
                {
                    shuffleTheImage();
                    _time = TimeSpan.FromSeconds(time_allowed);
                    labelTimer.Text = _time.ToString("c");
                    startButton.Content = "Start";
                }
                _timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
                {
                    labelTimer.Text = _time.ToString("c");
                    if (_time == TimeSpan.Zero)
                    {
                        _timer.Stop();
                        _isPlayable = false;
                        MessageBox.Show("Time up!");
                    }
                    _time = _time.Add(TimeSpan.FromSeconds(-1));
                }, Application.Current.Dispatcher);
                _timer.Start();
                _isContinue = false;
            }
            
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            StreamWriter streamWriter = new StreamWriter("Save Game.txt");
            streamWriter.WriteLine(screen.FileName.ToString());
            for(int i = 0; i<3;i++)
            {
                for(int j=0;j<3;j++)
                {
                    streamWriter.Write(_numberMatrix[i, j].ToString() + " ");
                   
                }
                streamWriter.WriteLine();
            }
            string timeLabel = labelTimer.Text;
            const string colon = ":";
            string[] tokens = timeLabel.Split(new string[] { colon }, StringSplitOptions.RemoveEmptyEntries);
            
            streamWriter.WriteLine(Int32.Parse(tokens[0]) * 3600 + Int32.Parse(tokens[1]) * 60 + Int32.Parse(tokens[2]));
            MessageBox.Show("Save successfully");
            streamWriter.Close();
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            

            StreamReader streamReader = new StreamReader("Save Game.txt");
            string directory = streamReader.ReadLine();

            for (int i = 0; i < 3; i++)
            {
                string s = streamReader.ReadLine();
                const string space = " ";
                string[] tokens = s.Split(new string[] { space }, StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < 3; j++)
                {
                    _numberMatrix[i, j] = Int32.Parse(tokens[j]);
                    canvas.Children.Remove(_croppedImageMatrix[i, j]);
                }
            }
            int timeRemain = Int32.Parse(streamReader.ReadLine());
            _time = TimeSpan.FromSeconds(timeRemain);
            labelTimer.Text = _time.ToString("c");



            var source = new BitmapImage(
                new Uri(directory, UriKind.Absolute));

            var h = (int)source.Height;
            var w = (int)source.Width;

            if (h > w)
            {
                double ratio = h / (double)square_length;
                double ratio_preview = h / (double)preview_length;

                h_cropped = (int)(h / ratio);
                w_cropped = (int)(w / ratio);
                h_cropped_preview = (int)(h / ratio_preview);
                w_cropped_preview = (int)(w / ratio_preview);
                padding_X = (square_length - w_cropped) / 2;
                padding_Y = 0;

            }
            else
            {
                double ratio = w / (double)square_length;
                double ratio_preview = w / (double)preview_length;

                h_cropped = (int)(h / ratio);
                w_cropped = (int)(w / ratio);
                h_cropped_preview = (int)(h / ratio_preview);
                w_cropped_preview = (int)(w / ratio_preview);
                padding_X = 0;
                padding_Y = (square_length - h_cropped) / 2;
            }

            previewImage.Width = w_cropped_preview;
            previewImage.Height = h_cropped_preview;
            previewImage.Source = source;


            for (int u = 0; u < 3; u++)
            {
                for (int v = 0; v < 3; v++)
                {
                    int index = _numberMatrix[u, v];
                    if (index == 0)
                        continue;

                    int i = (index - 1) / 3;
                    int j = (index - 1) % 3;

                    var rect = new Int32Rect(j * w / 3, i * h / 3, w / 3, h / 3);
                    var cropBitmap = new CroppedBitmap(source,
                        rect);

                    var cropImage = new Image();
                    cropImage.Stretch = Stretch.Fill;
                    cropImage.Width = w_cropped / 3;
                    cropImage.Height = h_cropped / 3;
                    cropImage.Source = cropBitmap;
                    canvas.Children.Add(cropImage);
                    Canvas.SetLeft(cropImage, startX + padding_X + v * (w_cropped / 3 + 2));
                    Canvas.SetTop(cropImage, startY + padding_Y + u * (h_cropped / 3 + 2));



                    cropImage.MouseLeftButtonDown += CropImage_MouseLeftButtonDown;
                    cropImage.PreviewMouseLeftButtonUp += CropImage_PreviewMouseLeftButtonUp;


                    _croppedImageMatrix[u, v] = cropImage;
                }
            }

            streamReader.Close();
            startButton.Content = "Continue";
            MessageBox.Show("Load successfully!");
            _isContinue = true;

        }
    }


}
