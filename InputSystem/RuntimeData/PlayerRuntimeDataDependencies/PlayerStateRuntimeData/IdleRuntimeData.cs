using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;

namespace PlayerData
{
    [System.Serializable]
    public class IdleRuntimeData 
    { 
        [ReadOnly] private IdleActionData idleActionData;

        public IdleRuntimeData(IdleActionData idleActionData)
        {
            this.idleActionData = idleActionData;

        }
    }
}
