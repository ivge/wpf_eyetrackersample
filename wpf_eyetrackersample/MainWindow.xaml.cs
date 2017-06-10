using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using Tobii.Gaze;

namespace wpf_eyetrackersample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        //button which is selected at this moment 
        private Button selectedButton;
        //timer which append button content to text string 
        private Timer timer;
        //subscription to gaze stream 
        private IDisposable subscription;

        public MainWindow()
        {
            InitializeComponent();
            var eyeTrackerFactory = new EyeTrackerFactory();
            var stream = eyeTrackerFactory.CreateMouseTracker().GetGazeStream();
            var gazeObserver = new GazeObserver();
            subscription = stream.Subscribe(gazeObserver);
            InitializeTimer(gazeObserver);
            gazeObserver.onChange += () => { PointerCheck(gazeObserver); };
        }

        
        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //dispose subscription to gaze stream
            subscription.Dispose();
        }

        private void InitializeTimer(GazeObserver gazeObserver)
        {
            //create timer which will add selected button to text box  
            timer = new Timer((object obj) =>
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    //make sure that some button is selected
                    if (selectedButton != null)
                    {
                        textBox1.Text += selectedButton.Content;
                    }
                }));
            });
        }

        private void PointerCheck(GazeObserver gazeObserver)
        {
            //get list of all objects on MainWindow
            UIElementCollection UIelements = null;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                UIelements = ((Grid)Application.Current.MainWindow?.Content)?.Children;
                checkUIElementsUnderPointer(UIelements, gazeObserver);
            }));
        }

        private void checkUIElementsUnderPointer(UIElementCollection UIelements, GazeObserver gazeObserver)
        {
            //set local flag to see if any button selected 
            bool isAnyButtonSelected = false;

            //if window is disposed and there is no more objects on mainwindow then exit 
            if (UIelements == null) return;
            foreach (var element in UIelements)
            {
                //check if element is a button 
                if (element is Button)
                {
                    var button = (Button)element;
                    //Check if pointer on button
                    if (CheckButtonUnderPointer(button, gazeObserver))
                    {
                        //Highlight button
                        button.Opacity = 1;
                        isAnyButtonSelected = true;

                        //Check if button under pointer is previously selected button 
                        if (button != selectedButton)
                        {
                            //if no then assign button under pointer to selected button
                            selectedButton = button;
                            //and start timer
                            timer.Change(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
                        }
                    }

                    //if button isn't under pointer 
                    else
                    {
                        //make it less visible
                        button.Opacity = 0.5;
                    }
                }
            }
            //if none of the button is selected  
            if (!isAnyButtonSelected)
            {
                //disable timer
                timer.Change(Timeout.Infinite, Timeout.Infinite);
                selectedButton = null;
            }
        }

        private bool CheckButtonUnderPointer(Button button, GazeObserver gazeObserver)
        {
            // Get absolute location on screen of upper left and bottom right corners of button
            Point topLeft = button.PointToScreen(new Point(0, 0));
            Point bottomRight = button.PointToScreen(new Point(button.ActualWidth, button.ActualHeight));

            // Transform screen point to WPF device independent point
            PresentationSource source = PresentationSource.FromVisual(this);

            Point absolutePointTopLeft = source.CompositionTarget.TransformFromDevice.Transform(topLeft);
            Point absolutePointBottomRight = source.CompositionTarget.TransformFromDevice.Transform(bottomRight);

            if (gazeObserver.pointer.X > (topLeft.X / SystemParameters.VirtualScreenWidth) &&
                gazeObserver.pointer.X < (bottomRight.X / SystemParameters.VirtualScreenWidth) &&
                gazeObserver.pointer.Y > (topLeft.Y / SystemParameters.VirtualScreenHeight) &&
                gazeObserver.pointer.Y < (bottomRight.Y / SystemParameters.VirtualScreenHeight))
            { return true; }
            else
            { return false; }
        }
    }

}
