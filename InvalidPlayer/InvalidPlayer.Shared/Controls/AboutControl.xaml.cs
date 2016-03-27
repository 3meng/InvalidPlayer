using InvalidPlayerPlugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using InvalidPlayerCore.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace InvalidPlayer.Control
{
    public sealed partial class AboutControl : YukiPopup
    {
        public AboutControl()
        {
            this.InitializeComponent();
        }
        //Main host = new Main();

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            string result = "";
            var s = JsInput.Text;
            await Task.Run(() =>
            {
                Main.Init();
                result = Main.RunScript(s);
                Main.JsDispose();
            });
            JsOutput.Text = result;
            JsOutput.UpdateLayout();
            JsOutputScroll.ChangeView(null, double.MaxValue, null);
        }

        protected override string GetTag()
        {
            return "AboutControl";
        }
    }
}
