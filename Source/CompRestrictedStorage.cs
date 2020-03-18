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
        //
        //   Other (possible) restriction categories
        //     cannibals (r)
        //     animals restricted to zones (r)

        public override void Initialize(CompProperties props) {
            base.Initialize(props);
        }
        public override void PostDraw() {
            base.PostDraw();
            if (AllForbidden()) {
                this.parent.Map.overlayDrawer.DrawOverlay(this.parent, OverlayTypes.ForbiddenBig);
                return;
            }
            if (AnyForbidden()) {
                this.parent.Map.overlayDrawer.DrawOverlay(this.parent, OverlayTypes.Forbidden);
                return;
            }
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
            //todo: maybe a bar showing what's actually selected?
            //Much todo:
            l.Label("Who may take from here?"); // todo: or "These options have no effect right now"
            l.CheckboxLabeled("All Humans"/*-like*/, ref allowHumans, null);
            // cannibals
            // non-cannials
            // depressives
            // non-depressives (chirpy people)
            l.CheckboxLabeled("All animals", ref allowAnimals, null);
            Color d=GUI.color;
            if (d!=Color.gray && allowAnimals) { // gray options if animals are selected
                GUI.color=Color.gray;
            }
            l.CheckboxLabeled("  that can graze (plant eaters)", ref allowGrazers, null);
            l.CheckboxLabeled("  that cannot graze", ref allowNonGrazers, null);
            l.CheckboxLabeled("  that can eat meat", ref allowMeatEaters, null);
            l.CheckboxLabeled("  that cannot eat meat", ref allowNonMeatEaters, null);
            //l.CheckboxLabeled("  Herbivores? (probably going away)", ref allowHerbivores, null);
            //l.CheckboxLabeled("  Carnivores? (probably going away)", ref allowCarnivores, null);
            GUI.color=d;
            //if (AllowAll) {
            GUI.color=c;
            //}
        }

        public override void PostExposeData() {
            Scribe_Values.Look(ref allowAll, "allowAll", true);
            Scribe_Values.Look(ref allowHumans, "allowHumans", false);
            Scribe_Values.Look(ref allowAnimals, "allowAnimals", false);
            Scribe_Values.Look(ref allowGrazers, "allowGrazers", false);
            Scribe_Values.Look(ref allowNonGrazers, "allowNonGrazers", false);
            Scribe_Values.Look(ref allowMeatEaters, "allowMeatEaters", false);
            Scribe_Values.Look(ref allowNonMeatEaters, "allowNonMeatEaters", false);
            //Scribe_Values.Look(ref allowHerbivores, "allowHerbivores", false);
            //Scribe_Values.Look(ref allowCarnivores, "allowCarnivores", false);
        }
        bool AllForbidden() {
            if (AllowAll) return false;
            if (allowHumans) return false;
            if (allowAnimals) return false;
            if (allowGrazers) return false;
            if (allowNonGrazers) return false;
            if (allowMeatEaters) return false;
            if (allowNonMeatEaters) return false;
            return true;
        }
        bool AnyForbidden() {
            if (allowAll) return false;
            if (AllowAllHumans() && AllowAllAnimals()) return false;
            return true;
        }
        bool AllowAllHumans() {
            if (allowHumans) return true;
            return false;
        }
        bool AllowAllAnimals() {
            if (allowAnimals) return true;
            if (allowGrazers && allowNonGrazers) return true;
            if (allowMeatEaters && allowNonMeatEaters) return true;
            return false;
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
                if (allowGrazers && ((race.foodType & (FoodTypeFlags.Plant |
                                                       FoodTypeFlags.Tree))>0)) {
                    return false;
                }
                if (allowNonGrazers && ((race.foodType & (FoodTypeFlags.Plant |
                                                       FoodTypeFlags.Tree)) == 0)) {
                    return false;
                }
                if (allowMeatEaters && ((race.foodType & (FoodTypeFlags.CarnivoreAnimalStrict)) > 0)) {
                    return false;
                }
                if (allowNonMeatEaters && ((race.foodType & (FoodTypeFlags.CarnivoreAnimalStrict)) == 0)) {
                    return false;
                }
                /*if (allowHerbivores && !race.Eats(FoodTypeFlags.Meat)
                    && !race.Eats(FoodTypeFlags.AnimalProduct)) {
                    return false;
                }
                //todo:
                if (allowCarnivores && ((race.foodType & (FoodTypeFlags.VegetableOrFruit |
                                                          FoodTypeFlags.Seed |
                                                          FoodTypeFlags.Tree |
                                                          FoodTypeFlags.Plant)) == 0)) {
                    return false;
                }*/
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
        bool allowHumans=false;
        bool allowAnimals=false;
        //bool allowHerbivores=false;
        //bool allowCarnivores = false;
        bool allowGrazers = false;
        bool allowNonGrazers = false;
        bool allowMeatEaters = false;
        bool allowNonMeatEaters = false;

    }

}
