using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        Random rng = new Random();

        //Hang so the hien mui ten
        const int LEFT_ARROW = 3000;
        const int RIGHT_ARROW = 3001;
        const int UP_ARROW = 3002;
        const int DOWN_ARROW = 3003;

        int[,] _numberMatrix;
        Image[,] _croppedImageMatrix;

        int h_cropped = 0;
        int w_cropped = 0;

        int h_cropped_preview = 0;
        int w_cropped_preview = 0;

        int padding_X = 0;
        int padding_Y = 0;



        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _numberMatrix = new int[3, 3];
            _croppedImageMatrix = new Image[3, 3];

            var screen = new OpenFileDialog();

            if (screen.ShowDialog() == true)
            {

                var source = new BitmapImage(
                    new Uri(screen.FileName, UriKind.Absolute));

                var h = (int)source.Height;
                var w = (int)source.Width;




                if (h > w)
                {
                    double ratio = h / (double)square_length;
                    double ratio_preview = h / (double)preview_length;

                    h_cropped = (int) (h / ratio);
                    w_cropped = (int) (w / ratio);
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

                Canvas.SetLeft(previewImage, 800);
                Canvas.SetTop(previewImage, 20);

                // Bat dau cat thanh 9 manh

                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (!((i == 2) && (j == 2)))
                        {
                            
                            //Debug.WriteLine($"Len = {len}");
                            var rect = new Int32Rect(j * w/3, i * h/3, w/3, h/3);
                            var cropBitmap = new CroppedBitmap(source,
                                rect);

                            var cropImage = new Image();
                            cropImage.Stretch = Stretch.Fill;
                            cropImage.Width = w_cropped/3;
                            cropImage.Height = h_cropped/3;
                            cropImage.Source = cropBitmap;
                            canvas.Children.Add(cropImage);
                            Canvas.SetLeft(cropImage, startX+padding_X + j * (w_cropped/3  + 2));
                            Canvas.SetTop(cropImage, startY+padding_Y + i * (h_cropped/3 + 2));

                           

                            cropImage.MouseLeftButtonDown += CropImage_MouseLeftButtonDown;
                            cropImage.PreviewMouseLeftButtonUp += CropImage_PreviewMouseLeftButtonUp;
                            //cropImage.Tag = new Tuple<int, int>(i, j);

                            _croppedImageMatrix[i, j] = cropImage;
                            _numberMatrix[i, j] = 3 * i + j+1;
                            //cropImage.MouseLeftButtonUp
                        }
                    }
                }
            }



            shuffleTheImage();
        }


        bool _isDragging = false;
        Image _selectedBitmap = null;
        Point _lastPosition;
        int _iOrigin;
        int _jOrigin;
        private void CropImage_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            var position = e.GetPosition(this);
            int i = ((int)position.Y - startY - padding_Y) / (h_cropped/3);
            int j = ((int)position.X - startX - padding_X) / (w_cropped/3);
            int x, y;
            int dy = Math.Abs(i - _iOrigin);
            int dx = Math.Abs(j - _jOrigin);


            if (!(((i >= 0 && i < 3 && j >= 0 && j < 3)) && (dx * dy == 0) && (dx + dy == 1)))
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

            int temp = _numberMatrix[i, j];
            _numberMatrix[i, j] = _numberMatrix[_iOrigin, _jOrigin];
            _numberMatrix[_iOrigin, _jOrigin] = temp;

            if (_croppedImageMatrix[i,j] == null)
            {
                _croppedImageMatrix[i, j] = _croppedImageMatrix[_iOrigin, _jOrigin];
                _croppedImageMatrix[_iOrigin, _jOrigin] = null;
            }

            if (checkWin() == true)
            {
                MessageBox.Show("You win!");
            }

        }

        private void CropImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _selectedBitmap = sender as Image;
            _lastPosition = e.GetPosition(this);
            _iOrigin = ((int)_lastPosition.Y - startY - padding_Y) / (h_cropped / 3);
            _jOrigin = ((int)_lastPosition.X - startX - padding_X) / (w_cropped / 3);
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition(this);

            int i = ((int)position.Y - startY) / (h_cropped/3);
            int j = ((int)position.X - startX) / (w_cropped/3);

         


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

        private void Window_KeyUp(object sender, KeyEventArgs e)
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

            if (checkWin() == true)
            {
                MessageBox.Show("You win!");
            }
        }


        private bool checkWin()
        {
            if (_numberMatrix[2, 2] != 0)
                return false;
                
            for(int i=0;i<3;i++)
            {
                for(int j=0;j<3;j++)
                {
                    if ((_numberMatrix[i,j] != 3*i+j+1) && (3*i+j != 8))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
