﻿using OverhaulMod.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverhaulMod.Utils
{
    public static class ModUIUtility
    {
        public static void MessagePopup(string header, string description, float height = 125f, MessageMenu.ButtonLayout buttonLayout = MessageMenu.ButtonLayout.OkButton, string okText = null, string yesText = null, string noText = null, Action okAction = null, Action yesAction = null, Action noAction = null)
        {
            UIMessagePopup messagePopup = UIConstants.ShowMessagePopup();
            messagePopup.SetTexts(header, description);
            messagePopup.SetHeight(height);
            messagePopup.SetButtonLayout(buttonLayout);
            messagePopup.SetButtonActions(okAction, yesAction, noAction);
            messagePopup.SetButtonTexts(okText, yesText, noText);
        }

        public static void MessagePopupOK(string header, string description)
        {
            MessagePopup(header, description, 150f, MessageMenu.ButtonLayout.OkButton, "ok");
        }

        public static void MessagePopupOK(string header, string description, float height = 125f)
        {
            MessagePopup(header, description, height, MessageMenu.ButtonLayout.OkButton, "ok");
        }

        public static void MessagePopupOK(string header, string description, string buttonText, float height = 125f)
        {
            MessagePopup(header, description, height, MessageMenu.ButtonLayout.OkButton, buttonText);
        }

        public static void MessagePopupOK(string header, string description, string buttonText, Action buttonAction, float height = 125f)
        {
            MessagePopup(header, description, height, MessageMenu.ButtonLayout.OkButton, buttonText, null, null, buttonAction);
        }
    }
}
