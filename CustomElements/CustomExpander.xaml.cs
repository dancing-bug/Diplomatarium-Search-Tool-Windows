using System;
using System.Collections.Generic;
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

namespace CorpusSearchEngine.CustomElements
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class CustomExpander : UserControl
    {
        string[] fullText = new string[3];
        string[] shortText = new string[3];
        bool isShortened = true;
        int tabState = 0;
        SolidColorBrush redBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 240, 79, 95));
        SolidColorBrush greenBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 113, 217, 121));

        //System.Windows.Media.Color highlight = System.Windows.Media.Color.FromRgb(114, 207, 103);

        public CustomExpander()
        {
            InitializeComponent();
        }

        public string word
        {
            //get { return ExpanderTitle.Header; }
            set { ExpanderTitle.Header = value; }
        }

        public bool isExpanded
        {
            //get { return ExpanderTitle.Header; }
            set { ExpanderTitle.IsExpanded = value; }
        }

        /*public string[] highlightedContent
        {
            set
            {
                ProcessTextParts(value);
            }
        }*/

        public void AddNewEntry(string bind, string number, string[] content)
        {
            ExpanderStackPanel.Children.Add(new TextTab { bind = bind, number = number, highlightedContent = content });
        }
    }
}
