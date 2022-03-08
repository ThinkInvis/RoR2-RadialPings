using RoR2;
using UnityEngine;

namespace ThinkInvisible.RadialPings {
    internal class DroneMenuBindings : RadialMenuBindings<DroneMenuBindings> {
        private BindingInfo returnBinding = new BindingInfo() {
            descriptionToken = "RADIALPINGS_CAPTION_RETURN",
            hoverActivationTime = 1f,
            onActivate = (sender, isHover) => {
                if(isHover) {
                    var pingHelper = sender.GetComponent<PingMenuHelper>();
                    pingHelper.owner.GetComponent<PingMenuInstanceTracker>().latestMenu = MainPingMenuBindings.instance.Instantiate(pingHelper.owner);
                } else
                    CancelMenuAction(sender, isHover);
            }
        };

        protected internal override GameObject Instantiate(PlayerCharacterMasterController owner) {
            buttonBindingInfos.Clear();
            buttonBindingInfos.Add(returnBinding);
            var buttonCount = Random.Range(1, 24);
            thetaOffsetDegr = -180/(buttonCount+1);
            for(int i = 0; i < buttonCount; i++)
                buttonBindingInfos.Add(new BindingInfo {
                    iconColor = Color.HSVToRGB((float)i/buttonCount, 1f, 1f),
                    descriptionToken = $"Test Button {i+1}",
                    sprite = LegacyResourcesAPI.Load<Sprite>("textures/miscicons/texWIPIcon")
                });

            var menuObj = base.Instantiate(owner);

            var pingHelper = menuObj.AddComponent<PingMenuHelper>();
            pingHelper.owner = owner;
            pingHelper.TryUpdatePingInfo();
            
            return menuObj;
        }
    }
}
