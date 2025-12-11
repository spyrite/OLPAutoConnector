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
        private const string TabName = "OSK.Olimproekt";
        private const string PanelName1 = " ∆";

        private static readonly string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
        private static RibbonPanel ribbonPanel;
        private static List<PushButtonData> pushButtonsData;

        public Result OnStartup(UIControlledApplication application)
        {
            ribbonPanel = application.GetRibbonPanels(TabName).FirstOrDefault(p => p.Name == PanelName1);
            ribbonPanel ??= application.CreateRibbonPanel(TabName, PanelName1);
            pushButtonsData =
            [
                new PushButtonData("Button_JoinCICapsAndHost", "—оединить\nзаглушки «ƒ", thisAssemblyPath, typeof(JoinCICapsAndHost).FullName)
                {
                    ToolTip = "—оедин€ет вложенные в закладные детали \"бетонные заглушки\" с основой, согласует материалы заглушек и основы.",
                    LongDescription = "Ѕетонные заглушки, выполненные тем же материалом что и основа, в соединенном положении позвол€ют скрыть выемки, образованные закладными детал€ми в основе." +
                    "“аким образом, основа отображаетс€ сплошными поверхност€ми без излишних контруных линий от выемок.",
                    Image = new BitmapImage(new Uri(@"pack://application:,,,/AutoConnector;component/Resources/Images/JoinCICapsAndHost_16.ico")),
                    LargeImage = new BitmapImage(new Uri(@"pack://application:,,,/AutoConnector;component/Resources/Images/JoinCICapsAndHost_32.ico"))
                }
            ];

            ribbonPanel.AddItem(pushButtonsData[0]);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}