using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using PlayerData;

namespace PlayerStates
{
    public abstract class PlayerState 
    {
        public bool doExecute;
        public bool onEnterLock;

        public Action onExitCallback;
        public Action onEnterCallback;

        public PlayerState()
        {
            onExitCallback = () => 
            {  
                doExecute = false;
                onEnterLock = false;
            };

            onEnterCallback = () => 
            {  
                onEnterLock = true;
            };
        }


        public virtual void CheckTransitions(PlayerRuntimeData playerDataManagement) {}
        public virtual void Execute(PlayerRuntimeData playerDataManagement) {}
        public virtual void OnEnter(PlayerRuntimeData playerDataManagement)
        {
            Action callback = onEnterCallback;
            callback();
        }

        public virtual void OnExit(PlayerRuntimeData playerDataManagement, Action action)
        {
            Action callback = onExitCallback;
            callback();
        }
    }
}
