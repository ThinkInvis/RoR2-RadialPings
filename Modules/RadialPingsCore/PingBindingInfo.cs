using RoR2;

namespace ThinkInvisible.RadialPings {
    public class PingBindingInfo : RadialMenuBindings.BindingInfo {
        public PingCatalog.PingType[] orderedTypes = new PingCatalog.PingType[0];

        public PingBindingInfo() {
            onActivate = PingOnActivate;
            contextStringProvider = PingContext;
        }

        public virtual void PingOnActivate(ProceduralRadialMenu sender, bool isHover) {
            var pingHelper = sender.GetComponent<PingMenuHelper>();
            pingHelper.AuthorityPerformCustomPing(null, orderedTypes);
        }

        public virtual string PingContext(ProceduralRadialMenu sender) {
            var pingHelper = sender.GetComponent<PingMenuHelper>();
            return pingHelper.GetFormattedContext(null, orderedTypes);
        }
    }

    public class DirectedPingBindingInfo : PingBindingInfo {
        public PlayerCharacterMasterController targetPCMC;

        public override void PingOnActivate(ProceduralRadialMenu sender, bool isHover) {
            var pingHelper = sender.GetComponent<PingMenuHelper>();
            pingHelper.AuthorityPerformCustomPing(new[] {targetPCMC.gameObject}, orderedTypes);
        }
        
        public override string PingContext(ProceduralRadialMenu sender) {
            var pingHelper = sender.GetComponent<PingMenuHelper>();
            return pingHelper.GetFormattedContext(new[] {targetPCMC.gameObject}, orderedTypes);
        }
    }

    public class HybridHoverPingBindingInfo : PingBindingInfo {
        public ProceduralRadialButton.OnActivate onHoverActivate;

        public override void PingOnActivate(ProceduralRadialMenu sender, bool isHover) {
            if(isHover)
                onHoverActivate?.Invoke(sender, isHover);
            else
                base.PingOnActivate(sender, isHover);
        }
    }
}
