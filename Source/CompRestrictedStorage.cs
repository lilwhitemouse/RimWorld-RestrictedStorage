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

        public void DisplayFineOptions(Listing_Standard l) {
            Color c=GUI.color;
            if (AllowAll) {
                GUI.color=Color.gray;
            }
            //Much todo:
            l.Label("Who may take from here?");
            l.CheckboxLabeled("Humans"/*-like*/, ref allowHumans, null);
            l.CheckboxLabeled("Animals", ref allowAnimals, null);
            if (AllowAll) {
                GUI.color=c;
            }
        }

        public override void PostExposeData() {
            Scribe_Values.Look(ref allowAll, "allowAll", true);
            Scribe_Values.Look(ref allowHumans, "allowHumans", true);
            Scribe_Values.Look(ref allowAnimals, "allowAnimals", true);
        }
        public bool IsForbidden(Pawn p) {
            // obviously a lot to do here ;)
            if (allowAll) return false;
            if (allowHumans && p.RaceProps.Humanlike) return false;
            if (allowAnimals && p.RaceProps.Animal) return false;
            return true;
        }
        public bool AllowAll {
            get { return allowAll; }
            set { allowAll=value; }
        }
        public bool AllowHumans {
            get { return allowHumans; }
            set { allowHumans=value; }
        }
        public bool AllowAnimals {
            get { return AllowAnimals; }
            set { allowAnimals=value; }
        }
        bool allowAll=true;
        bool allowHumans=true;
        bool allowAnimals=true;
    }

}
