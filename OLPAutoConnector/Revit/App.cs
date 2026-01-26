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

        private const string TabName = "Olimproekt";
        private const string PanelName = "Моделирование";

        private static readonly string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

        private static RibbonPanel ribbonPanel;
        private static List<PushButtonData> pushButtonsData;
        private static List<SplitButtonData> splitButtonsData;

        public Result OnStartup(UIControlledApplication application)
        {

            try { application.CreateRibbonTab(TabName); } catch { }

            ribbonPanel = application.GetRibbonPanels(TabName).FirstOrDefault(p => p.Name == PanelName)
                         ?? application.CreateRibbonPanel(TabName, PanelName);

            pushButtonsData =
            [

                new PushButtonData("Button_ConnectRailings", "Автосоединение\nограждений", thisAssemblyPath, typeof(ConnectRailings).FullName)
                {
                    ToolTip = "Соединяет поручни выбранной пары ограждений",
                    LongDescription = "Выполняет соединение поручней путем настройки параметров концевиков. Работает с ограниченным списком семейств ограждений (см. справку).",
                    Image = new BitmapImage(new Uri(@"pack://application:,,,/AutoConnector;component/Resources/Images/ConnectRailings_16.png")),
                    LargeImage = new BitmapImage(new Uri(@"pack://application:,,,/AutoConnector;component/Resources/Images/ConnectRailings_32.png"))
                },

                new PushButtonData("Button_DisconnectRailings", "Авторазъединение\nограждений", thisAssemblyPath, typeof(DisconnectRailings).FullName)
                {
                    ToolTip = "Отсоединяет поручни выбранных ограждений",
                    LongDescription = "Работает по предварительно выбранным ограждений ограниченного списка семейств (см. справку).",
                    Image = new BitmapImage(new Uri(@"pack://application:,,,/AutoConnector;component/Resources/Images/DisconnectRailings_16.png")),
                    LargeImage = new BitmapImage(new Uri(@"pack://application:,,,/AutoConnector;component/Resources/Images/DisconnectRailings_32.png"))
                },
            ];

            splitButtonsData =
            [
                new SplitButtonData("SplitButton_AutoConnector", "Автосоединение"),
            ];

            splitButtonsData[0].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, Properties.AutoConnector.Default.AutoConnectorHelpURL));
            pushButtonsData[0].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, Properties.AutoConnector.Default.ConnectRailingsHelpURL));
            pushButtonsData[1].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, Properties.AutoConnector.Default.DisconnectRailingsHelpURL));


            SplitButton splitButton = ribbonPanel.AddItem(splitButtonsData[0]) as SplitButton;
            splitButton.AddPushButton(pushButtonsData[0]);
            splitButton.AddPushButton(pushButtonsData[1]);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}