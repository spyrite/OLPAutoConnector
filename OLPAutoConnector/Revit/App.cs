using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;


namespace OLP.AutoConnector.Revit
{
    public class App : IExternalApplication
    {
        private const string TabName1 = "OSK.Olimproekt";
        private const string TabName2 = "Olimproekt";
        private const string PanelName1 = " ∆";
        private const string PanelName2 = "ћоделирование";

        private static readonly string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
        private static RibbonPanel ribbonPanel1;
        private static RibbonPanel ribbonPanel2;
        private static List<PushButtonData> pushButtonsData;
        private static List<SplitButtonData> splitButtonsData;

        public Result OnStartup(UIControlledApplication application)
        {
            ribbonPanel1 = application.GetRibbonPanels(TabName1).FirstOrDefault(p => p.Name == PanelName1);
            ribbonPanel1 ??= application.CreateRibbonPanel(TabName1, PanelName1);

            ribbonPanel2 = application.GetRibbonPanels(TabName2).FirstOrDefault(p => p.Name == PanelName1);
            ribbonPanel2 ??= application.CreateRibbonPanel(TabName2, PanelName2);

            pushButtonsData =
            [
                new PushButtonData("Button_JoinCICapsAndHost", "—оединить\nзаглушки «ƒ", thisAssemblyPath, typeof(JoinCICapsAndHost).FullName)
                {
                    ToolTip = "—оедин€ет вложенные в закладные детали \"бетонные заглушки\" с основой, согласует материалы заглушек и основы.",
                    LongDescription = "Ѕетонные заглушки, выполненные тем же материалом что и основа, в соединенном положении позвол€ют скрыть выемки, образованные закладными детал€ми в основе." +
                    "“аким образом, основа отображаетс€ сплошными поверхност€ми без излишних контруных линий от выемок.",
                    Image = new BitmapImage(new Uri(@"pack://application:,,,/AutoConnector;component/Resources/Images/JoinCICapsAndHost_16.png")),
                    LargeImage = new BitmapImage(new Uri(@"pack://application:,,,/AutoConnector;component/Resources/Images/JoinCICapsAndHost_32.png"))
                },

                new PushButtonData("Button_ConnectRailings", "јвтосоедиение\nограждений", thisAssemblyPath, typeof(ConnectRailings).FullName)
                {
                    ToolTip = "—оедин€ет поручни выбранной пары ограждений",
                    LongDescription = "¬ыполн€ет соеднение поручней путем настройки параметров концевиков. –аботает с ограниченным списком семейств ограждений (см. справку).",
                    Image = new BitmapImage(new Uri(@"pack://application:,,,/AutoConnector;component/Resources/Images/ConnectRailings_16.png")),
                    LargeImage = new BitmapImage(new Uri(@"pack://application:,,,/AutoConnector;component/Resources/Images/ConnectRailings_32.png"))
                },

                new PushButtonData("Button_DisconnectRailings", "јвто-отсоедиение\nограждений", thisAssemblyPath, typeof(DisconnectRailings).FullName)
                {
                    ToolTip = "ќтсоедин€ет поручни выбранных ограждений",
                    LongDescription = "–аботает по предварительно выбранным ограждений ограниченного списка семейств (см. справку).",
                    Image = new BitmapImage(new Uri(@"pack://application:,,,/AutoConnector;component/Resources/Images/DisconnectRailings_16.png")),
                    LargeImage = new BitmapImage(new Uri(@"pack://application:,,,/AutoConnector;component/Resources/Images/DisconnectRailings_32.png"))
                },
            ];

            splitButtonsData =
            [
                new SplitButtonData("SplitButton_AutoConnector2", "јвтосоединение"),
            ];

            pushButtonsData[0].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, Properties.AutoConnector.Default.JoinCICapsAndHostHelpURL));
            pushButtonsData[1].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, Properties.AutoConnector.Default.ConnectRailingsHelpURL));
            pushButtonsData[2].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, Properties.AutoConnector.Default.DisconnectRailingsHelpURL));

            ribbonPanel1.AddItem(pushButtonsData[0]);

            SplitButton splitButton2 = ribbonPanel2.AddItem(splitButtonsData[0]) as SplitButton;
            splitButton2.AddPushButton(pushButtonsData[1]);
            splitButton2.AddPushButton(pushButtonsData[2]);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}