using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using Verse.AI;

namespace RestrictedStorage
{
    public class CompRestrictedStorage : ThingComp {
        // TODO:
        //   public override IEnumerable<Gizmo> CompGetGizmosExtra() {
        //   public override string TransformLabel(string label) {?????
        //   public override string CompInspectStringExtra() {


        public override void Initialize(CompProperties props) {
            base.Initialize(props);
        }

        public override void PostExposeData() {
            Scribe_Values.Look(ref allowAll, "allowAll", true);
        }
        public bool IsForbidden(Pawn p) {
            // obviously a lot to do here ;)
            return !this.allowAll;
        }
        public bool AllowAll {
            get { return allowAll; }
            set { allowAll=value; }
        }
        bool allowAll=true;
    }

}
