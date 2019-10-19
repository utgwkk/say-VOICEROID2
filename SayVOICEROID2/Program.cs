using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Automation;

namespace SayVOICEROID2
{
    class Program
    {
        static void Main(string[] args)
        {
            string phrase;
            if (args.Length < 1)
            {
                using (var sr = new StreamReader(Console.OpenStandardInput(), Encoding.UTF8))
                    phrase = sr.ReadToEnd();
            }
            else
                phrase = args[0];

            Say(phrase);
        }

        static void Say(string phrase)
        {
            // VOICEROID2のウィンドウを探す
            var root = AutomationElement.RootElement;
            var titleConditions = new [] {
                new PropertyCondition(AutomationElement.NameProperty, "VOICEROID2"),
                new PropertyCondition(AutomationElement.NameProperty, "VOICEROID2*")
            };
            var form = root.FindFirst(TreeScope.Element | TreeScope.Children, new OrCondition(titleConditions));
            if (form == null)
            {
                Console.Error.WriteLine("VOICEROID2のウィンドウが見つかりません。VOICEROID2を起動してください");
                return;
            }

            // 喋らせたいフレーズをセットする
            var textEditView = form.FindFirst(
                TreeScope.Element | TreeScope.Children,
                new PropertyCondition(AutomationElement.ClassNameProperty, "TextEditView")
            );
            var textBoxElem = textEditView.FindFirst(
                TreeScope.Element | TreeScope.Children,
                new PropertyCondition(AutomationElement.ClassNameProperty, "TextBox")
            );
            ValuePattern editValue = textBoxElem.GetCurrentPattern(ValuePattern.Pattern) as ValuePattern;
            editValue.SetValue(phrase);

            // 再生ボタンを探して押す
            // textEditViewの最初に出てくるボタンが再生ボタンなのでこれで動いているけど将来ずっと動くかは不明
            var playButton = textEditView.FindFirst(
                TreeScope.Element | TreeScope.Children,
                new PropertyCondition(AutomationElement.ClassNameProperty, "Button")
            );
            var playButtonControl = playButton.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
            playButtonControl.Invoke();

            // 喋っている間はテキストボックスがreadonlyになっているので、喋りおわるまで待つ
            // Invokeしてすぐはreadonlyになっていなくて抜けてしまうので、do-whileにすることでうまく待つという戦略
            do { Thread.Sleep(100); } while (editValue.Current.IsReadOnly);
        }
    }
}
