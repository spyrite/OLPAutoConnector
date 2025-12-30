using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;


namespace OLP.AutoConnectorKR.Revit
{
    public class App : IExternalApplication
    {
        private const string TabName = "OSK.Olimproekt";
        private const string PanelName = "КЖ";

        private static readonly string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
        private static RibbonPanel ribbonPanel;
        private static List<PushButtonData> pushButtonsData;

        public Result OnStartup(UIControlledApplication application)
        {
            try { application.CreateRibbonTab(TabName); } catch { }
            

            ribbonPanel = application.GetRibbonPanels(TabName).FirstOrDefault(p => p.Name == PanelName)
                         ?? application.CreateRibbonPanel(TabName, PanelName);

            

            pushButtonsData =
            [
                new PushButtonData("Button_JoinCICapsAndHost", "Соединить\nзаглушки ЗД", thisAssemblyPath, typeof(JoinCICapsAndHost).FullName)
                {
                    ToolTip = "Соединяет вложенные в закладные детали \"бетонные заглушки\" с основой, согласует материалы заглушек и основы.",
                    LongDescription = "Бетонные заглушки, выполненные тем же материалом что и основа, в соединенном положении позволяют скрыть выемки, образованные закладными деталями в основе." +
                    "Таким образом, основа отображается сплошными поверхностями без излишних контруных линий от выемок.",
                    Image = new BitmapImage(new Uri(@"pack://application:,,,/AutoConnectorKR;component/Resources/Images/JoinCICapsAndHost_16.png")),
                    LargeImage = new BitmapImage(new Uri(@"pack://application:,,,/AutoConnectorKR;component/Resources/Images/JoinCICapsAndHost_32.png"))
                },

            ];

            pushButtonsData[0].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, Properties.AutoConnectorKR.Default.JoinCICapsAndHostHelpURL));

            ribbonPanel.AddItem(pushButtonsData[0]);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}