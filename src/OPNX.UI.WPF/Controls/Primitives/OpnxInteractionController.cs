using System.Windows.Input;

namespace OPNX.UI.WPF.Controls
{
    /// <summary>
    /// 행위들에 대한 정의.
    /// </summary>
    public abstract class OpnxInteractionController
    {
        /// <summary>
        /// 마우스 오른쪽 버튼 다운.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        public abstract void MouseRightButtonDownProcess(object sender, MouseButtonEventArgs e);

        /// <summary>
        /// 마우스 오른쪽 버튼 업.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        public abstract void MouseRightButtonUpProcess(object sender, MouseButtonEventArgs e);

        /// <summary>
        /// 마우스 왼쪽 버튼 다운.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        public abstract void MouseLeftButtonDownProcess(object sender, MouseButtonEventArgs e);

        /// <summary>
        /// 스타일러스 버튼 다운.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public abstract void StylusDownProcess(object sender, StylusDownEventArgs e);

        /// <summary>
        /// 터치 버튼 다운.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public abstract void TouchDownProcess(object sender, TouchEventArgs e);

        /// <summary>
        /// 마우스 왼쪽 버튼 업.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        public abstract void MouseLeftButtonUpProcess(object sender, MouseButtonEventArgs e);

        public abstract void PreviewMouseLeftButtonUpProcess(object sender, MouseButtonEventArgs e);

        /// <summary>
        /// 스타일러스 버튼 업.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public abstract void StylusUpProcess(object sender, StylusEventArgs e);

        /// <summary>
        /// 터치 버튼 업.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public abstract void TouchUpProcess(object sender, TouchEventArgs e);

        /// <summary>
        /// 마우스 휠.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        public abstract void MouseWheelProcess(object sender, MouseWheelEventArgs e);

        /// <summary>
        /// 마우스 움직이기.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        public abstract void MouseMoveProcess(object sender, MouseEventArgs e);

        /// <summary>
        /// 스타일러스 움직이기.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public abstract void StylusMoveProcess(object sender, StylusEventArgs e);

        /// <summary>
        /// 터치 움직이기.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public abstract void TouchMoveProcess(object sender, TouchEventArgs e);

        public abstract void ManipulationStarting(object sender, ManipulationStartingEventArgs e);

        public abstract void ManipulationDelta(object sender, ManipulationDeltaEventArgs e);

        public abstract void ManipulationInertiaStarting(object sender, ManipulationInertiaStartingEventArgs e);

        public abstract void StylusSystemGesture(object sender, StylusSystemGestureEventArgs e);
    }
}




