using System;
using System.Collections.Generic;
using System.Drawing;
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
using Point = System.Windows.Point;

namespace CorpusSearchEngine.CustomElements
{
    /// <summary>
    /// Interaction logic for TextTab.xaml
    /// </summary>
    public partial class TextTab : UserControl
    {
        string[] fullText = new string[3]; 
        string[] shortText = new string[3];
        bool isShortened = true;
        int tabState = 0;
        SolidColorBrush redBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 240, 79, 95));
        SolidColorBrush greenBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 113, 217, 121));

        //System.Windows.Media.Color highlight = System.Windows.Media.Color.FromRgb(114, 207, 103);
        public TextTab()
        {
            InitializeComponent();
        }

        public string bind
        {
            get { return Bind.Text; }
            set { Bind.Text = value; }
        }
        
        public string number
        {
            get { return Number.Text; }
            set { Number.Text = value; }
        }


        public string content
        {
            get { return Content2.Text; }
            set { Content2.Text = value;}
        }

        public string[] highlightedContent
        {
            set
            {
                ProcessTextParts(value);
            }
        }

        public void ProcessTextParts(string[] content)
        {
            fullText = content;
            if(Math.Max(0, content[0].Length - 300) > 0)
            {
                shortText[0] = "(...) " + content[0].Substring(content[0].Length - 300);
            }
            else
            {
                shortText[0] = content[0].Substring(0);
            }
            //shortText[0] = "... " + content[0].Substring(Math.Max(0, content[0].Length - 300));
            shortText[1] = content[1];

            if (Math.Min(content[2].Length, 300) < 300)
            {
                shortText[2] = content[2].Substring(0, content[2].Length);
            }
            else
            {
                shortText[2] = content[2].Substring(0, 300) + " (...)";
            }
            //shortText[2] = content[2].Substring(0, Math.Min(content[2].Length,  300)) + "...";

            isShortened = true;
            HighlightWord(shortText);
        }

        public void HighlightWord(string[] content)
        {
            Content1.Text = content[0];
            Content2.Text = content[1];
            Content2.Background = Brushes.PaleGreen;
            Content3.Text = content[2];
        }

        private void FlipExpandImage(int scale)
        {
            expandImg.RenderTransformOrigin = new Point(0.5, 0.5);
            ScaleTransform flipTransform = new ScaleTransform();
            flipTransform.ScaleY = scale;
            expandImg.RenderTransform = flipTransform;
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Bind.Text + " " + Number.Text + " " + Content1.Text + Content2.Text + Content3.Text);
        }

        private void ExpandButton_Click(object sender, RoutedEventArgs e)
        {
            if (isShortened)
            {
                Content1.Text = fullText[0];
                Content2.Text = fullText[1];
                Content3.Text = fullText[2];
                isShortened = false;

                FlipExpandImage(-1);
            }
            else
            {
                Content1.Text = shortText[0];
                Content2.Text = shortText[1];
                Content3.Text = shortText[2];
                isShortened = true;

                FlipExpandImage(1);
            }
        }

        private void TabButton_Click(object sender, RoutedEventArgs e)
        {
            if (tabState == 0)
            {
                tabState++;
                tabBorder.Background = redBrush;
            }
            else if (tabState == 1)
            {
                tabState++;
                tabBorder.Background = greenBrush;
            }
            else
            {
                tabState = 0;
                tabBorder.Background = Brushes.Transparent;
            }

        }
    }
}
