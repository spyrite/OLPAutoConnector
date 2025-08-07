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
        private const string TabName = "Олимп.ОСК";
        private const string PanelName = "Утилиты";

        private static readonly string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
        private static RibbonPanel ribbonPanel;
        private static List<PushButtonData> pushButtonsData;

        public Result OnStartup(UIControlledApplication application)
        {
            ribbonPanel = application.GetRibbonPanels(TabName).FirstOrDefault(p => p.Name == PanelName);
            ribbonPanel ??= application.CreateRibbonPanel(TabName, PanelName);
            pushButtonsData =
            [
                new PushButtonData("Button_IntermodelCopier", "Соединить\nзаглушки ЗД", thisAssemblyPath, typeof(JoinCICapsAndHost).FullName)
                {
                    ToolTip = "Соединяет вложенные в закладные детали \"бетонные заглушки\" с основой, согласует материалы заглушек и основы.",
                    LongDescription = "Бетонные заглушки, выполненные тем же материалом что и основа, в соединенном положении позволяют скрыть выемки, образованные закладными деталями в основе." +
                    "Таким образом, основа отображается сплошными поверхностями без излишних контруных линий от выемок.",
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