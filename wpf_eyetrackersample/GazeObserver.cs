using System;
using Tobii.Gaze;

namespace wpf_eyetrackersample
{
    public class GazeObserver : IObserver<GazeData>
    {
        public GazeData pointer;
        public string view;
        public void OnCompleted()
        {
            view = "something completed";
        }

        public void OnError(Exception error)
        {
            view = "Some error" + error;
        }

        public void OnNext(GazeData value)
        {
            view = value.X + " " + value.Y;
            pointer = value;
        }

        public GazeObserver()
        {
            view = "";
        }
    }
}
