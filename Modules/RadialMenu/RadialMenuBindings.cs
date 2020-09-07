using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ThinkInvisible.RadialPings {
    /// <summary>
    /// Wrapper for RadialMenuBindings which provides an instance getter. <typeparamref name="T"/> should always be the same type as the inheriting class.
    /// </summary>
    /// <typeparam name="T">The type to retrieve instance information for.</typeparam>
    public class RadialMenuBindings<T> : RadialMenuBindings where T:RadialMenuBindings {
        public static T instance {get;private set;}

        public RadialMenuBindings() {
            if(instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting RadialMenuBindings was instantiated twice");
            instance = this as T;
        }
    }

    ///<summary>Contains information about a specific RadialMenu and the buttons belonging to it, for purposes of interop with mod code. Use RadialMenuBindings&lt;T&gt; while inheriting, and create an instance of the class during plugin Awake to perform initialization.</summary>
    public abstract class RadialMenuBindings {
        ///<summary>Delegate for ProceduralRadialButton.OnActivate. Causes the menu to perform a cancel animation.</summary>
        public static void CancelMenuAction(ProceduralRadialMenu sender, bool isHover) {
            sender.extraOutroScale = -1f;
            sender.inOutTwistAngleDegr = -sender.inOutTwistAngleDegr;
        }

        ///<summary>Contains information about a ProceduralRadialButton belonging to the menu specified by a RadialMenuBindings.</summary>
        public class BindingInfo {
            ///<summary>The language token to use while looking up the button's main caption.</summary>
            public string descriptionToken = "RADIALPINGS_CAPTION_NYI";
            ///<summary>The action to perform when the button is activated.</summary>
            public ProceduralRadialButton.OnActivate onActivate = null;
            ///<summary>The string provider to use as the button's subcaption. Should emit information about e.g. the target object or the action to be performed.</summary>
            public ProceduralRadialButton.ContextStringProvider contextStringProvider = null;
            ///<summary>The sprite to use as the button's icon.</summary>
            public Sprite sprite = Resources.Load<Sprite>("textures/miscicons/texWIPIcon");
            ///<summary>The tint color to apply to the button's icon sprite.</summary>
            public Color iconColor = Color.white;
            ///<summary>The tint color to apply to the button's hover fill overlay.</summary>
            public Color hoverFillColor = new Color(1f, 1f, 1f, 0.5f);
            ///<summary>The time the cursor must hover over the button before activating it. Set to &lt;= 0f to disable hover activation.</summary>
            public float hoverActivationTime = 0f;
            ///<summary>The strength at which the button's hover fill overlay's color will animate over time.</summary>
            public float hoverColorWobble = 0.2f;
        }

        ///<summary>BindingInfos in this list will be used to add buttons to the menu upon instantiation.</summary>
        protected readonly List<BindingInfo> buttonBindingInfos = new List<BindingInfo>();
        ///<summary>This BindingInfo will be used while the cursor is inside all buttons.</summary>
        protected BindingInfo innerDeadZoneBindingInfo = new BindingInfo {descriptionToken="RADIALPINGS_CAPTION_CANCEL", onActivate=CancelMenuAction};
        ///<summary>This BindingInfo will be used while the cursor is outside all buttons.</summary>
        protected BindingInfo outerDeadZoneBindingInfo = new BindingInfo {descriptionToken="RADIALPINGS_CAPTION_CANCEL", onActivate=CancelMenuAction};

        ///<summary>Rotates the entire menu, for purposes of aesthetic alignment of specific buttons.</summary>
        protected float thetaOffsetDegr = 0f;

        /// <summary>
        /// Use to perform late setup on a button object (e.g. adding components).
        /// </summary>
        /// <param name="sourceInfo">The BindingInfo that was used to create this button.</param>
        /// <param name="button">The instantiated button object, which has just been setup and added to a new ProceduralRadialMenu.</param>
        public delegate void ModifyButton(BindingInfo sourceInfo, GameObject button);
        ///<summary>Use to perform late setup on a button object (e.g. adding components).</summary>
        public event ModifyButton onModifyButton;

        /// <summary>
        /// Creates a ProceduralRadialMenu instance, sets it up with the information contained in this RadialMenuBindings, and attaches it to the owner's HUD.
        /// </summary>
        /// <param name="owner">The owner PlayerCharacterMasterController to assign a new ProceduralRadialMenu to. Must be non-null and have a HUD instance associated with it.</param>
        /// <returns>The instantiated ProceduralRadialMenu for further modification.</returns>
        internal protected virtual GameObject Instantiate(PlayerCharacterMasterController owner) {
            var targetHUD = HUD.readOnlyInstanceList.First(x => x.targetMaster == owner.master);
            var menuObj = GameObject.Instantiate(RadialMenu.genericRadialMenuPrefab, targetHUD.transform.Find("MainContainer").Find("MainUIArea"));

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
