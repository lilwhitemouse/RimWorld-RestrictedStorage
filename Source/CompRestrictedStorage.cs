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
        //     animals restricted to areas (r)

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
        public void DisplayMainOption(Rect inRect) {
            Rect r=new Rect(inRect.xMin, inRect.yMin, inRect.width, inRect.height/2);
            bool tmp=allowAll;
            Widgets.CheckboxLabeled(r, "Anyone May Take?", ref allowAll);
            if (Mouse.IsOver(r)) {
                Widgets.DrawHighlight(r);
            }
            TooltipHandler.TipRegion(r, "Check this to allow anyone to take from this storage site.\nUncheck to allow only those specifically allowed to take things here.");
            if (allowAll && !tmp && allowNone) {
                allowNone=false;
            }
            tmp=allowNone;
            r=new Rect(inRect.xMin, inRect.yMin+inRect.height/2, inRect.width, inRect.height/2);
            Widgets.CheckboxLabeled(r, "No one May Take?", ref allowNone);
            if (Mouse.IsOver(r)) {
                Widgets.DrawHighlight(r);
            }
            TooltipHandler.TipRegion(r, "If this is checked, NO ONE may take anything from here.  This overrides everything else.");
            if (allowNone && !tmp && allowAll) {
                allowAll=false;
            }
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
        private static Vector2 scrollPosition=new Vector2(0f,0f);
        private static float scrollViewHeight=1000f;
        public void DisplayFineOptions(Rect outerRect) {
            Color origColor=GUI.color;
            if (allowAll || allowNone) {
                GUI.color=Color.gray;
            }
            float y=0f;
            float w=outerRect.width-6f; //width, with room for scrollbar  //todo: 16f??
            Rect viewRect = new Rect(0f, 0f, w, scrollViewHeight); //todo: 16f??
            //GUI.BeginGroup(outerRect); do scroll view instead
            Widgets.BeginScrollView(outerRect, ref scrollPosition, viewRect);
            //todo: maybe a bar showing what's actually selected?....??
            //Much todo:
            // todo: make this label a button that shows who CAN take from here?
            Widgets.Label(new Rect(0,0,w,22f),"Who may take from here?"); // todo: or "These options have no effect right now"
            y+=22f;
            if (CheckboxChangedToTrue(ref y, w, "All Humans"/*-like*/, ref allowHumans, "Humans, humanlike, etc")) {
                noAllNone();
            }
            // cannibals
            // non-cannials
            // depressives
            // non-depressives (chirpy people)?
            Widgets.DrawLineHorizontal(0, y+1, w);
            y+=2;
            if (CheckboxChangedToTrue(ref y, w, "All animals", ref allowAnimals, "LWM.AllAnimalsDesc")) {
                noAllNone();
            }
            Color d=GUI.color;
            if (d!=Color.gray && allowAnimals) { // gray options if animals are selected
                GUI.color=Color.gray;
            }
            if (
                CheckboxChangedToTrue(ref y, w, "that can graze (plant eaters)", ref allowGrazers, "LWM.AnimalsThatGrazeDesc", 1) ||
                CheckboxChangedToTrue(ref y, w, "that cannot graze", ref allowNonGrazers, "LWM.AnimalsThatDoNotGrazeDesc", 1) ||
                CheckboxChangedToTrue(ref y, w, "that can eat meat", ref allowMeatEaters, "LWM.AnimalsThatEatMeatDesc", 1) ||
                CheckboxChangedToTrue(ref y, w, "that cannot eat meat", ref allowNonMeatEaters, "LWM.AnimalsThatDoNotEatMeatDesc", 1)
                //CheckboxChangedToTrue(ref y, w, ,1) ||
                ) {
                noAllNone();
                if (allowAnimals) allowAnimals=false;
            }
            GUI.color=d;

            if (Widgets.ButtonText(new Rect(0,y,w,22), "Area control"))
                Find.WindowStack.Add(new Dialog_SpecifyAreas(this.parent.Map, this));
            y+=22f;
//todo: add buttons / info here
            bool needToRemove=false;
            Area toRemove=null; // "null" is a valid Area
            if (allowedIfInAreas != null) {
                foreach (Area a in allowedIfInAreas) {
                    bool tmp=true; // can do this MUCH better - with color!
                    Widgets.CheckboxLabeled(new Rect(20f,y,w-20f,22), "Anyone with area "+AreaUtility.AreaAllowedLabel_Area(a), ref tmp);
                    y+=22f;
                    if (!tmp) {
                        toRemove=a; // can't remove things mid-foreach
                        needToRemove=true;
                    }
                }
                if (needToRemove) allowedIfInAreas.Remove(toRemove);
            }
            if (allowedIfNotInAreas!=null) { //oh hey, it's stupid to have more than one...whatever.
                needToRemove=false;
                foreach (Area a in allowedIfNotInAreas) {
                    bool tmp=true; // can do this MUCH better - with color!
                    Widgets.CheckboxLabeled(new Rect(20f,y,w-20f,22), "Any area BESIDES "+AreaUtility.AreaAllowedLabel_Area(a), ref tmp);
                    y+=22f;
                    if (!tmp) toRemove=a;
                }
                if (needToRemove) allowedIfNotInAreas.Remove(toRemove);
            }
            GUI.color=origColor;
            scrollViewHeight=y;
            Widgets.EndScrollView();
        }

        // Helper function that does what it says on the box
        private static bool CheckboxChangedToTrue(ref float y, float width, string textKey, ref bool key, string tooltipKey, int indent=0) {
            // this could have been done in 5 linse with a Listing_Standard....if Listing_Standard did
            //   "painting" - which allows the player to drag X (or Check) to mark a bunch of thing X at once.
            //   But.  That is a super useful feature, so:
            bool tmp=key;
            Rect textRect=new Rect(indent*20f,y+1,width-(indent*20f)-24f,22f);
            //Rect checkRect=new Rect(width-24f,y,24f,24f);
            Rect highlightRect=new Rect(indent*20f,y,width-(indent*20f),24f);
            Widgets.Label(textRect,textKey); //todo: translate()
            Widgets.Checkbox(width-24f, y, ref key, 24f, false, true /*paintable*/, null, null);
            if (Mouse.IsOver(highlightRect)) {
                if (!tooltipKey.NullOrEmpty())
                    TooltipHandler.TipRegion(highlightRect,tooltipKey.Translate());
                Widgets.DrawHighlight(highlightRect);
                if (Widgets.ButtonInvisible(textRect)) {
                    key=!key;
                }
            }
            y+=24f;
            if (tmp==false &&
                key==true) return true;
            return false;
        }

        public override void PostExposeData() {
            //if (Scribe.mode==LoadSaveMode.Saving &&
            //    this.IsDefault()) return;// don't bother saving.
            Scribe_Values.Look(ref allowAll, "LWM_RS_allowAll", true);
            Scribe_Values.Look(ref allowNone, "LWM_RS_allowNone", false);
            Scribe_Values.Look(ref allowHumans, "LWM_RS_allowHumans", false);
            Scribe_Values.Look(ref allowAnimals, "LWM_RS_allowAnimals", false);
            Scribe_Values.Look(ref allowGrazers, "LWM_RS_allowGrazers", false);
            Scribe_Values.Look(ref allowNonGrazers, "LWM_RS_allowNonGrazers", false);
            Scribe_Values.Look(ref allowMeatEaters, "LWM_RS_allowMeatEaters", false);
            Scribe_Values.Look(ref allowNonMeatEaters, "LWM_RS_allowNonMeatEaters", false);
            // need to have this called both during loading vars and during cross-reference
            //   so cannot firewall it behind a variable that disappears after Scribe.mode of LoadingVars
//            bool tmp=(allowedIfInAreas !=null || allowedIfNotInAreas!=null);
//            bool tmp=(!allowedIfInAreas.NullOrEmpty() || !allowedIfNotInAreas.NullOrEmpty());
//            Scribe_Values.Look(ref tmp, "areas", false);
//            if (tmp) {
                Scribe_Collections.Look<Area>(ref this.allowedIfInAreas, false, "LWM_RS_areaIfIn", LookMode.Reference, Array.Empty<object>());
                Scribe_Collections.Look<Area>(ref this.allowedIfNotInAreas, false, "LWM_RS_areaIfNotIn", LookMode.Reference, Array.Empty<object>());
//                Log.Message("allowed if in areas is "+(allowedIfInAreas==null?"NULL":allowedIfInAreas.Count.ToString())+", mode "+Scribe.mode);
//            }
//            tmp=(allowedPawns!=null || disallowedPawns!=null);
//            Scribe_Values.Look(ref tmp, "pawns", false);
//            if (tmp) {
                Scribe_Collections.Look<Pawn>(ref this.allowedPawns, "LWM_RS_okPawns", LookMode.Reference, Array.Empty<object>());
                Scribe_Collections.Look<Pawn>(ref this.disallowedPawns, "LWM_RS_notThesePawns", LookMode.Reference, Array.Empty<object>());
//            }
                // clean up, just in case:
                if (allowedIfInAreas!=null && allowedIfInAreas.Count==0) allowedIfInAreas=null;
                if (allowedIfNotInAreas!=null && allowedIfNotInAreas.Count==0) allowedIfNotInAreas=null;
                if (allowedPawns!=null && allowedPawns.Count==0) allowedPawns=null;
                if (disallowedPawns!=null && disallowedPawns.Count==0) disallowedPawns=null;
        }
        bool AllForbidden() { //TODO: update this once, keep in variable
            if (allowAll) return false;
            if (allowNone) return true;
            if (allowHumans) return false;
            if (allowAnimals) return false;
            if (allowGrazers) return false;
            if (allowNonGrazers) return false;
            if (allowMeatEaters) return false;
            if (allowNonMeatEaters) return false;
            if (!allowedIfInAreas.NullOrEmpty()) return false;
            if (!allowedIfNotInAreas.NullOrEmpty()) return false;
            if (!allowedPawns.NullOrEmpty()) return false;
            return true;
        }
        bool AnyForbidden() {//TODO: maybe make this a flag?
            if (allowAll) return false;
            if (allowNone) return true;
            if (AllowAllHumans() && AllowAllAnimals()) return false;
            if (!disallowedPawns.NullOrEmpty()) return false;
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
            if (p.Faction!=Faction.OfPlayer) return false;
            if (allowNone) return true;
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
                if (allowedIfInAreas!=null) {
                    //Log.Message("Checking pawn "+p+" with area restirction "+p.playerSettings.AreaRestriction+" vs "+allowedIfInAreas[0]+" (count "+allowedIfInAreas.Count+")");
                    if (allowedIfInAreas.Contains(p.playerSettings.AreaRestriction)) return false;
                }
                if (!allowedIfNotInAreas.NullOrEmpty()) {
                    if (!allowedIfNotInAreas.Contains(p.playerSettings.AreaRestriction)) return false;
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
                //Log.Message("Checking animal "+p+" about to fail");
            }
            return true;
        }
        public void AddAllowedArea(Area a) {
            if (allowedIfInAreas==null) allowedIfInAreas=new List<Area>();
            allowedIfInAreas.Add(a);
            noAllNone();
        }
        public void RemoveAllowedArea(Area a) {
            if (allowedIfInAreas!=null) allowedIfInAreas.Remove(a);
        }
        public bool IsAllowedInArea(Area a) {
            return ((allowedIfInAreas!=null) && (allowedIfInAreas.Contains(a)));
        }
        public void AddAllowedNotInArea(Area a) {
            if (allowedIfNotInAreas==null) allowedIfNotInAreas=new List<Area>();
            allowedIfNotInAreas.Add(a);
            noAllNone();
        }
        public void RemoveAllowedNotInArea(Area a) {
            if (allowedIfNotInAreas!=null) allowedIfNotInAreas.Remove(a);
        }
        public bool IsAllowedNotInArea(Area a) {
            return ((allowedIfNotInAreas!=null) && (allowedIfNotInAreas.Contains(a)));
        }
        // When a lesser option is selected, turn off allowAll and allowNone:
        //   (by using this super duper easy to type function!)
        //   Why bother with the checks?  Wny not just "allowAll=false; allowNone=false;"?
        //   Because Multiplayer has to syncronise those changes across all clients.
        //   So only do it if we need to?  ...not that it happens often, so whatever.
        //   it's habit now.
        void noAllNone() {
            if (allowAll) allowAll=false;
            if (allowNone) allowNone=false;
        }

        /*public bool AllowAll {
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
        }*/
        // does it have only default options:
        public virtual bool IsDefault() {
            if (allowNone || allowHumans || allowAnimals || allowGrazers || allowNonGrazers ||
                allowMeatEaters || allowNonMeatEaters )
                return false;
            if (!allowedIfInAreas.NullOrEmpty() || !allowedIfNotInAreas.NullOrEmpty() ||
                !allowedPawns.NullOrEmpty() || !disallowedPawns.NullOrEmpty() )
                return false;
            return allowAll; //starts true
        }
        public virtual void CopyAllowancesFrom(CompRestrictedStorage other) {
            this.allowAll=other.allowAll;
            this.allowNone=other.allowNone;
            this.allowHumans=other.allowHumans;
            this.allowAnimals=other.allowAnimals;
            this.allowGrazers=other.allowGrazers;
            this.allowNonGrazers=other.allowNonGrazers;
            this.allowMeatEaters=other.allowMeatEaters;
            this.allowNonMeatEaters=other.allowNonMeatEaters;
            allowedIfInAreas=null;
            if (other.allowedIfInAreas!=null)
                allowedIfInAreas=new List<Area>(other.allowedIfInAreas);
            allowedIfNotInAreas=null;
            if (other.allowedIfNotInAreas!=null)
                allowedIfNotInAreas=new List<Area>(other.allowedIfNotInAreas);
            allowedPawns=null;
            if (other.allowedPawns!=null)
                allowedPawns=new List<Pawn>(allowedPawns);
            disallowedPawns=null;
            if (other.disallowedPawns!=null)
                disallowedPawns=new List<Pawn>(disallowedPawns);
        }
        bool allowAll=true;
        bool allowNone=false;
        bool allowHumans=false;
        bool allowAnimals=false;
        //bool allowHerbivores=false;
        //bool allowCarnivores = false;
        bool allowGrazers = false;
        bool allowNonGrazers = false;
        bool allowMeatEaters = false;
        bool allowNonMeatEaters = false;
        List<Area> allowedIfInAreas=null;
        List<Area> allowedIfNotInAreas=null;
        // todo: update this from time to time?  ...give pawns a comp if they are forbidden/allowed?
        //   or an hediff?
        List<Pawn> allowedPawns=null;
        List<Pawn> disallowedPawns=null;
    }

}
