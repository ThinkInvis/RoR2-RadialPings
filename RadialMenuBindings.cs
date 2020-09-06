using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ThinkInvisible.RadialPings {
    public class RadialMenuBindings<T> : RadialMenuBindings where T:RadialMenuBindings {
        public static T instance {get;private set;}

        public RadialMenuBindings() {
            if(instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting RadialMenuBindings was instantiated twice");
            instance = this as T;
        }
    }

    public abstract class RadialMenuBindings {
        public static void CancelMenuAction(ProceduralRadialMenu sender, bool isHover) {
            sender.extraOutroScale = -1f;
            sender.inOutTwistAngleDegr = -sender.inOutTwistAngleDegr;
        }

        public class BindingInfo {
            public string descriptionToken = "RADIALPINGS_CAPTION_NYI";
            public ProceduralRadialButton.OnActivate onActivate = null;
            public ProceduralRadialButton.ContextStringProvider contextStringProvider = null;
            public Sprite sprite = Resources.Load<Sprite>("textures/miscicons/texWIPIcon");
            public Color iconColor = Color.white;
            public Color hoverFillColor = new Color(1f, 1f, 1f, 0.5f);
            public float hoverActivationTime = 0f;
            public float hoverColorWobble = 0.2f;
        }

        protected readonly List<BindingInfo> buttonBindingInfos = new List<BindingInfo>();
        protected BindingInfo innerDeadZoneBindingInfo = new BindingInfo {descriptionToken="RADIALPINGS_CAPTION_CANCEL", onActivate=CancelMenuAction};
        protected BindingInfo outerDeadZoneBindingInfo = new BindingInfo {descriptionToken="RADIALPINGS_CAPTION_CANCEL", onActivate=CancelMenuAction};

        protected float thetaOffsetDegr = 0f;

        public delegate void ModifyButton(BindingInfo sourceInfo, GameObject button);
        public event ModifyButton onModifyButton;

        internal protected virtual GameObject Instantiate(PlayerCharacterMasterController owner) {
            var targetHUD = HUD.readOnlyInstanceList.First(x => x.targetMaster == owner.master);
            var menuObj = GameObject.Instantiate(RadialPingsPlugin.genericRadialMenuPrefab, targetHUD.transform.Find("MainContainer").Find("MainUIArea"));

            var menuCtrl = menuObj.GetComponent<ProceduralRadialMenu>();

            foreach(var bindingInfo in buttonBindingInfos) {
                var button = menuCtrl.Add(bindingInfo.descriptionToken, bindingInfo.onActivate, bindingInfo.contextStringProvider, bindingInfo.sprite, bindingInfo.iconColor, bindingInfo.hoverFillColor, bindingInfo.hoverActivationTime, bindingInfo.hoverColorWobble);
                onModifyButton?.Invoke(bindingInfo, button);
            }

            menuCtrl.innerDeadZoneContext = innerDeadZoneBindingInfo.contextStringProvider;
            menuCtrl.innerDeadZoneToken = innerDeadZoneBindingInfo.descriptionToken;
            menuCtrl.onInnerDeadZoneActivate = innerDeadZoneBindingInfo.onActivate;
            
            menuCtrl.outerDeadZoneContext = outerDeadZoneBindingInfo.contextStringProvider;
            menuCtrl.outerDeadZoneToken = outerDeadZoneBindingInfo.descriptionToken;
            menuCtrl.onOuterDeadZoneActivate = outerDeadZoneBindingInfo.onActivate;
            
            menuCtrl.targetCam = targetHUD.canvas.worldCamera;

            menuCtrl.thetaOffsetDegr = thetaOffsetDegr;

            return menuObj;
        }
    }
}
