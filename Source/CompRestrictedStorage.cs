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
        //   Overlay!

        public override void Initialize(CompProperties props) {
            base.Initialize(props);
        }

        //TODO: translate
        public void DisplayMainOption(Rect r) {
            Widgets.CheckboxLabeled(r, "Anyone May Take?", ref allowAll);
            if (Mouse.IsOver(r)) {
                Widgets.DrawHighlight(r);
            }
            TooltipHandler.TipRegion(r, "Check this to allow anyone to take from this storage site.\nUncheck to allow only those specifically allowed to take things here.");
        }

        public void DisplayFineOptions(Listing_Standard l) {
            Color c=GUI.color;
            if (AllowAll) {
                GUI.color=Color.gray;
            }
            //Much todo:
            l.Label("Who may take from here?");
            l.CheckboxLabeled("All Humans"/*-like*/, ref allowHumans, null);
            l.CheckboxLabeled("All Animals", ref allowAnimals, null);
            Color d=GUI.color;
            if (d!=Color.gray && allowAnimals) { // gray options if animals are selected
                GUI.color=Color.gray;
            }
            l.CheckboxLabeled("  Herbivores?", ref allowHerbivores, null);
            GUI.color=d;
            //if (AllowAll) {
            GUI.color=c;
            //}
        }

        public override void PostExposeData() {
            Scribe_Values.Look(ref allowAll, "allowAll", true);
            Scribe_Values.Look(ref allowHumans, "allowHumans", true);
            Scribe_Values.Look(ref allowAnimals, "allowAnimals", true);
            Scribe_Values.Look(ref allowHerbivores, "allowHerbivores", false);
        }
        public bool IsForbidden(Pawn p) {
            // obviously a lot to do here ;)
            if (allowAll) return false;
            RaceProperties race=p.RaceProps;
            if (allowHumans && race.Humanlike) return false;
            if (race.Animal) {
                if (allowAnimals) return false;
                // TODO: Non-eaters
                if (!race.EatsFood) {
                    return true; // we don't have a category for them yet: TODO
                }
                // Ugh.  Tree eaters.  They aren't Herbivorous.  They aren't Omniverous.  We can't
                //   look them up by ResolvedDietCategory :/
                if (allowHerbivores && !race.Eats(FoodTypeFlags.Meat)
                    && !race.Eats(FoodTypeFlags.AnimalProduct)) {
                    return false;
                }
                // TODO: "Other" - prolly a mod setting
                // TODO: robots etc?
            }
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
        bool allowHerbivores=false;
    }

}
