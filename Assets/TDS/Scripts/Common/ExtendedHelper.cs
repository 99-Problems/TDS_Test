using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data.Managers
{
    public static class ExtendedHelper
    {
        public static bool UseIngameExitBtn(this Define.ECONTENT_TYPE contentType)
        {
            switch (contentType)
            {
                case Define.ECONTENT_TYPE.MAIN:
                    return true;
                case Define.ECONTENT_TYPE.INGAME:
                default:
                    return false;
            }
        }

        public static Vector3 GetDirecton(this Define.EUNIT_DIRECTION direction)
        {
            switch (direction)
            {
                case Define.EUNIT_DIRECTION.UP:
                    return Vector3.forward;
                case Define.EUNIT_DIRECTION.DOWN:
                    return Vector3.back;
                case Define.EUNIT_DIRECTION.LEFT:
                    return Vector3.left;
                case Define.EUNIT_DIRECTION.RIGHT:
                    return Vector3.right;
                default:
                    return Vector3.zero;
            }
        }

        public static string GetLabelString(this Define.AssetLabel _label)
        {
            string label = "";
            switch (_label) 
            {
                case Define.AssetLabel.Default:
                    label = "default";
                    break;
                case Define.AssetLabel.Popup:
                    label = "Popup";
                    break;
                case Define.AssetLabel.Script:
                    label = "Script";
                    break;
                case Define.AssetLabel.Font:
                    label = "font";
                    break;
                case Define.AssetLabel.UI:
                    label = "ui";
                    break;
                case Define.AssetLabel.Material:
                    label = "Mat";
                    break;
                case Define.AssetLabel.Particle:
                    label = "Particle";
                    break;
                default:
                    break;
            }

            return label;
        }

        public static bool IsLoadLabel(this Define.AssetLabel label)
        {
            switch (label)
            {
                case Define.AssetLabel.Default:
                case Define.AssetLabel.Popup:
                case Define.AssetLabel.Script:
                    return true;
                
                default:
                    return false;
            }
        }
    }
}
