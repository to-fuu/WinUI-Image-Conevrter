using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using Windows.UI.ViewManagement;
using WinRT.Interop;
using static PInvoke.User32;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace appsdk_test
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();

            var hwnd = WindowNative.GetWindowHandle(m_window);


            SetWindowDetails(hwnd, 400, 800);
            m_window.Title = "Winui Image Converter";
            m_window.Activate();

            //m_window.ExtendsContentIntoTitleBar = true;

            //// Set active window colors
            //titleBar.ForegroundColor = Windows.UI.Color.FromArgb(1, 255, 255, 255);
            //titleBar.BackgroundColor = Windows.UI.Color.FromArgb(1, 255, 255, 255);
            SetWindowDetails(hwnd, 520, 900);


        }

        private Window m_window;

        private static void SetWindowDetails(IntPtr hwnd, int width, int height)
        {
            var dpi = GetDpiForWindow(hwnd);
            float scalingFactor = (float)dpi / 96;
            width = (int)(width * scalingFactor);
            height = (int)(height * scalingFactor);

            _ = SetWindowPos(hwnd, SpecialWindowHandles.HWND_TOP,
                                        0, 0, width, height,
                                        SetWindowPosFlags.SWP_NOMOVE);
            _ = SetWindowLong(hwnd,
                   WindowLongIndexFlags.GWL_STYLE,
                   (SetWindowLongFlags)(GetWindowLong(hwnd,
                      WindowLongIndexFlags.GWL_STYLE) ));

        }

    }
}
