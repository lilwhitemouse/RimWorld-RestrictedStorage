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

        /********************************************************
         *
         * Behavior:
         * If user selects suboption, unselect header options.
         *   Rationale: users will only select if they want
         *     to be more specific, so save clicks.
         *   (but only set header options to false if true -
         *    reduce sync spam for multiplayer)
         */
        public void DisplayFineOptions(Listing_Standard l) {
            Color origColor=GUI.color;
            Log.Message("Color is "+GUI.color);
            if (AllowAll) {
                GUI.color=Color.gray;
            }
            //todo: maybe a bar showing what's actually selected?....??
            //Much todo:
            // todo: make this label a button that shows who CAN take from here?
            l.Label("Who may take from here?"); // todo: or "These options have no effect right now"

            if (CheckboxChangedToTrue(l, "All Humans"/*-like*/, ref allowHumans, "Humans, humanlike, etc")
                && allowAll)
                allowAll=false;

                                      //l.CheckboxLabeled("All Humans"/*-like*/, ref allowHumans, "Humans, humanlike, etc");
            // cannibals
            // non-cannials
            // depressives
            // non-depressives (chirpy people)
            l.GapLine(2f);
            //l.CheckboxLabeled("All animals", ref allowAnimals, "LWM.AllAnimalsDesc".Translate());
            if (CheckboxChangedToTrue(l, "All animals", ref allowAnimals, "LWM.AllAnimalsDesc")
                && allowAll)
                allowAll=false;
            Color d=GUI.color;
            if (d!=Color.gray && allowAnimals) { // gray options if animals are selected
                GUI.color=Color.gray;
            }
            if (
                CheckboxChangedToTrue(l, "  that can graze (plant eaters)", ref allowGrazers, "LWM.AnimalsThatGrazeDesc") ||
                CheckboxChangedToTrue(l, "  that cannot graze", ref allowNonGrazers, "LWM.AnimalsThatDoNotGrazeDesc") ||
                CheckboxChangedToTrue(l, "  that can eat meat", ref allowMeatEaters, "LWM.AnimalsThatEatMeatDesc") ||
                CheckboxChangedToTrue(l, "  that cannot eat meat", ref allowNonMeatEaters, "LWM.AnimalsThatDoNotEatMeatDesc")
                //CheckboxChangedToTrue(l, ) ||
                ) {
                if (allowAll) allowAll=false;
                if (allowAnimals) allowAnimals=false;
            }
            /*l.CheckboxLabeled("  that can graze (plant eaters)", ref allowGrazers, "LWM.AnimalsThatGrazeDesc".Translate());
            l.CheckboxLabeled("  that cannot graze", ref allowNonGrazers, "LWM.AnimalsThatDoNotGrazeDesc".Translate());
            l.CheckboxLabeled("  that can eat meat", ref allowMeatEaters, "LWM.AnimalsThatEatMeatDesc".Translate());
            l.CheckboxLabeled("  that cannot eat meat", ref allowNonMeatEaters, "LWM.AnimalsThatDoNotEatMeatDesc".Translate());
            */
            //l.CheckboxLabeled("  Herbivores? (probably going away)", ref allowHerbivores, null);
            //l.CheckboxLabeled("  Carnivores? (probably going away)", ref allowCarnivores, null);
            GUI.color=d;
            //if (AllowAll) {
            GUI.color=origColor;
            //}
        }
        // Helper function that does what it says on the box
        private static bool CheckboxChangedToTrue(Listing_Standard l, string textKey, ref bool key, string tooltipKey) {
            bool tmp=key;
            l.CheckboxLabeled(textKey/*.Translate()TODO*/, ref key, tooltipKey.Translate());
            if (tmp==false &&
                key==true) return true;
            return false;
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
        bool AnyForbidden() {//TODO: maybe make this a flag?
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
