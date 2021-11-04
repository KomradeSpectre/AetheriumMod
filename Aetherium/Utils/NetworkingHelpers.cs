using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace Aetherium.Utils
{
    public static class NetworkingHelpers
    {
        public static T GetObjectFromNetIdValue<T>(uint netIdValue)
        {
            NetworkInstanceId netInstanceId = new NetworkInstanceId(netIdValue);
            NetworkIdentity foundNetworkIdentity = null;
            if (NetworkServer.active)
            {
                NetworkServer.objects.TryGetValue(netInstanceId, out foundNetworkIdentity);
            }
            else
            {
                ClientScene.objects.TryGetValue(netInstanceId, out foundNetworkIdentity);
            }

            if (foundNetworkIdentity)
            {
                T foundObject = foundNetworkIdentity.GetComponent<T>();
                if (foundObject != null)
                {
                    return foundObject;
                }
            }

            return default(T);
        }
    }
}
