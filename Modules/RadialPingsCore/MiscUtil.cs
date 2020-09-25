using RoR2;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ThinkInvisible.RadialPings {
    public static class MiscUtil {
        public static int ModifyTargetSelf(ref PingCatalog.PingData pingData, int catalogIndex, List<string> formatInserts) {
            pingData.targets[0] = pingData.owner?.GetComponent<PlayerCharacterMasterController>()?.master?.GetBodyObject();
            return 0;
        }

        public static bool TryGetObjDisplayName(GameObject targetObj, ref string name) {
            if(!targetObj) return false;
            var dnProv = targetObj.GetComponentInParent<IDisplayNameProvider>();
            if(dnProv != null) {
                name = Util.GetBestBodyName(((MonoBehaviour)dnProv).gameObject);
                return true;
            }
            return false;
        }
        
        public static bool TryGetPurchaseAppend(GameObject targetObj, ref string name) {
            if(!targetObj) return false;
            var tgtPurchase = targetObj.GetComponent<PurchaseInteraction>();
            var tgtShopTerminal = targetObj.GetComponent<ShopTerminalBehavior>();
            if(!tgtPurchase && !tgtShopTerminal) return false;
            if(tgtShopTerminal) {
                var pickup = PickupCatalog.GetPickupDef(tgtShopTerminal.pickupIndex);
                name += $" ({(tgtShopTerminal.pickupIndexIsHidden ? "?" : (pickup != null ? Language.GetString(pickup.nameToken) : PickupCatalog.invalidPickupToken))})";
            }
            if(tgtPurchase && tgtPurchase.costType != CostTypeIndex.None) {
                var builder = new StringBuilder();
                CostTypeCatalog.GetCostTypeDef(tgtPurchase.costType).BuildCostStringStyled(tgtPurchase.cost, builder, false, true);
                name += $" ({builder.ToString()})";
            }
            return true;
        }

        public static int CheckTarget2PRecipient(ref PingCatalog.PingData pingData, int catalogIndex, List<string> formatInserts) {
            return pingData.targets[1].GetComponent<PlayerCharacterMasterController>().hasEffectiveAuthority ? 1 : 0;
        }

        public static int ModifyTarget2PRecipient(ref PingCatalog.PingData pingData, int catalogIndex, List<string> formatInserts) {
            var pcmc = pingData.targets[1].GetComponent<PlayerCharacterMasterController>();
            if(pcmc && pcmc.hasEffectiveAuthority) {
                pingData.targets[0] = pcmc.body?.gameObject;
                return 1;
            }
            return 0;
        }

        public static void DefaultPingMenuAction(ProceduralRadialMenu sender, bool isHover) {
            var pingHelper = sender.GetComponent<PingMenuHelper>();
            if(pingHelper.owner && pingHelper.owner.body && pingHelper.owner.bodyInputs)
                DefaultPing(pingHelper.owner);
        }

        public static void DefaultPing(PlayerCharacterMasterController owner) {
            var menuTracker = owner.gameObject.GetComponent<PingMenuInstanceTracker>();
            if(!menuTracker) menuTracker = owner.gameObject.AddComponent<PingMenuInstanceTracker>();
            
            if(menuTracker.currentPingIsCustom && owner.pingerController.pingIndicator) {
                GameObject.Destroy(owner.pingerController.pingIndicator.gameObject);
                owner.pingerController.pingIndicator = null;
            }
            menuTracker.currentPingIsCustom = false;
            owner.pingerController.AttemptPing(new Ray(owner.bodyInputs.aimOrigin, owner.bodyInputs.aimDirection), owner.body.gameObject);
        }
    }
}
