using Heathen.Events;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Heathen.UX
{
    public class CommandParserExampleScript : MonoBehaviour
    {
        public UnityEngine.UI.InputField inputField;

        public string kbUrl;
        public string geUrl;
        public string discordUrl;
        public string assetUrl;

        public GameEvent voidEvent;
        public StringGameEvent echoEvent;

        private void Awake()
        {
            voidEvent.AddListener(HandleVoidEvent);
            echoEvent.AddListener(HandleEchoEvent);
        }

        private void OnDestroy()
        {
            voidEvent.RemoveListener(HandleVoidEvent);
            echoEvent.RemoveListener(HandleEchoEvent);
        }

        private void HandleVoidEvent(EventData arg0)
        {
            Debug.Log($"Void command raised by '{arg0.sender}'");
        }

        private void HandleEchoEvent(EventData<string> arg0)
        {
            Debug.Log($"Echo command raised by '{arg0.sender}' with argument '{arg0.value}'");
        }

        public void CommandRaised(CommandData data)
        {
            if(data.isValid)
            {
                Debug.Log("Detected event: '" + data.command.name + "' with the arguments of '" + data.arguments + "'");

                if (data.command.GetType() == typeof(StringGameEvent))
                {
                    //Example of invoking the command with an argument
                    ((StringGameEvent)data.command).Invoke(this, data.arguments);
                }
                else
                {
                    //Example of invoking the command without an argument
                    data.command.Invoke(this);
                }
            }
        }

        public void ClearInput()
        {
            inputField.text = string.Empty;
        }

        public void OpenKb()
        {
            UnityEngine.Application.OpenURL(kbUrl);
        }

        public void OpenGe()
        {
            UnityEngine.Application.OpenURL(geUrl);
        }

        public void OpenDiscord()
        {
            UnityEngine.Application.OpenURL(discordUrl);
        }

        public void OpenAsset()
        {
            UnityEngine.Application.OpenURL(assetUrl);
        }
    }
}