using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace OLP.AutoConnector.ViewModels
{
    public class ActionsVM : BindableBase
    {
        public enum TargetElementKind
        {
            CIs,
        }

        public enum NextAction
        {
            AllowUserSelection,
            SelectAllOnActiveView,
            SelectAllInModel,
            Cancel
        }

        private int _windowHeight;
        public int WindowHeight { get => _windowHeight; set => SetProperty(ref _windowHeight, value); }

        private string _instruction;
        public string Instruction { get => _instruction; set => SetProperty(ref _instruction, value); }

        private Visibility _allowDisplayNextActions;
        public Visibility AllowDisplayNextActions { get => _allowDisplayNextActions; set => SetProperty(ref _allowDisplayNextActions, value); }


        private Visibility _allowDisplayCancelButton;
        public Visibility AllowDisplayCancelButton { get => _allowDisplayCancelButton; set => SetProperty(ref _allowDisplayCancelButton, value); }

        private Visibility _allowDisplayDoNotShowThisWindowCheckBox;
        public Visibility AllowDisplayDoNotShowThisWindowCheckBox { get => _allowDisplayDoNotShowThisWindowCheckBox; set => SetProperty(ref _allowDisplayDoNotShowThisWindowCheckBox, value); }

        private NextAction _selectedNextAction;
        public NextAction SelectedNextAction 
        { 
            get => _selectedNextAction; 
            set 
            {
                if (SetProperty(ref _selectedNextAction, value))
                {
                    Properties.Actions.Default.SelectedNextAction = (int)value;
                    Properties.Actions.Default.Save();
                }
            }
        }


        public ActionsVM(TargetElementKind targetElementKind, int userSelectedCIsCount)
        {
            switch (targetElementKind)
            {
                case TargetElementKind.CIs:
                    if (userSelectedCIsCount > 0)
                    {
                        _instruction = $"Будет выполнено соединение бетонных заглушек с основой для {userSelectedCIsCount} выбранных закладных деталей. Желаете продолжить?";
                        _allowDisplayNextActions = Visibility.Collapsed;
                        _allowDisplayCancelButton = Visibility.Visible;
                        _allowDisplayDoNotShowThisWindowCheckBox = Visibility.Visible;
                        _windowHeight = 180;
                    }
                    else
                    {
                        _instruction = "Не выбрано ни одной закладной детали. Для продолжения выберите дальнейшее действие:";
                        _allowDisplayNextActions = Visibility.Visible;
                        _allowDisplayCancelButton = Visibility.Visible;
                        _allowDisplayDoNotShowThisWindowCheckBox = Visibility.Collapsed;
                        _windowHeight = 250;
                    } 
                    break;

                default:
                    _instruction = "В настоящее время плагин не работает с текущими выбранными элементами. Для расширения функционала данного плагина обратитесь в BIM-отдел.";
                    _allowDisplayNextActions = Visibility.Collapsed;
                    _allowDisplayCancelButton = Visibility.Collapsed;
                    _allowDisplayDoNotShowThisWindowCheckBox = Visibility.Collapsed;
                    _windowHeight = 180;
                    break;
            }
            _selectedNextAction = (NextAction)Properties.Actions.Default.SelectedNextAction;
        }

    }
}
