using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using Verse.AI;

namespace LWM.RestrictedStorage
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
            Color tmpColor;
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
            tmpColor=GUI.color;
            if (tmpColor!=Color.gray && allowHumans) { // gray options if animals are selected
                GUI.color=Color.gray;
            }
            if ( // human options
                // cannibals
                // non-cannials
                // depressives
                // non-depressives (chirpy people)?
                CheckboxChangedToTrue(ref y, w, "Colonists", ref allowColonists, "LWM.ColonistsDesc", 1) ||
                // if the player selects the first one, the rest won't be drawn, but who cares? they get drawn an instant later
                CheckboxChangedToTrue(ref y, w, "Guests", ref allowGuests, "LWM.ColonistsDesc", 1) ||
                CheckboxChangedToTrue(ref y, w, "Prisoners", ref allowPrisoners, "LWM.ColonistsDesc", 1)
                ) {
                noAllNone();
                if (allowHumans) allowHumans=false;
            }
            GUI.color=tmpColor;
            Widgets.DrawLineHorizontal(0, y+1, w);
            y+=2;
            if (CheckboxChangedToTrue(ref y, w, "All animals", ref allowAnimals, "LWM.AllAnimalsDesc")) {
                noAllNone();
            }
            tmpColor=GUI.color;
            if (tmpColor!=Color.gray && allowAnimals) { // gray options if animals are selected
                GUI.color=Color.gray;
            }
            if ( // animal options
                CheckboxChangedToTrue(ref y, w, "that can graze (plant eaters)", ref allowGrazers, "LWM.AnimalsThatGrazeDesc", 1) ||
                // if the player selects the first one, the rest won't be drawn, but who cares? they get drawn an instant later
                CheckboxChangedToTrue(ref y, w, "that cannot graze", ref allowNonGrazers, "LWM.AnimalsThatDoNotGrazeDesc", 1) ||
                CheckboxChangedToTrue(ref y, w, "that can eat meat", ref allowMeatEaters, "LWM.AnimalsThatEatMeatDesc", 1) ||
                CheckboxChangedToTrue(ref y, w, "that cannot eat meat", ref allowNonMeatEaters, "LWM.AnimalsThatDoNotEatMeatDesc", 1)
                //CheckboxChangedToTrue(ref y, w, ,1) ||
                ) {
                noAllNone();
                if (allowAnimals) allowAnimals=false;
            }
            GUI.color=tmpColor;

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
            if (Widgets.ButtonText(new Rect(0,y,w,22), "Pawn control"))
                Find.WindowStack.Add(new Dialog_SpecifyPawns(this));
            y+=22f;
            Pawn pawnToRemove=null;
            if (allowedPawns != null) {
                foreach (Pawn p in allowedPawns) {
                    bool tmp=true;
                    Widgets.CheckboxLabeled(new Rect(20f,y,w-20f,22), "Allowed: "+p.Name, ref tmp);
                    y+=22;
                    if (!tmp) {
                        pawnToRemove=p; // can't remove things mid-foreach
                    }
                }
                if (pawnToRemove!=null) allowedPawns.Remove(pawnToRemove);
            }
            if (disallowedPawns != null) {
                pawnToRemove=null;
                foreach (Pawn p in disallowedPawns) {
                    bool tmp=true;
                    Widgets.CheckboxLabeled(new Rect(20f,y,w-20f,22), "NOT allowed: "+p.Name, ref tmp);
                    y+=22;
                    if (!tmp) {
                        pawnToRemove=p; // can't remove things mid-foreach
                    }
                }
                if (pawnToRemove!=null) disallowedPawns.Remove(pawnToRemove);
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
        // Sometimes items that should not be in storage end up there
        //  (for example, butchering really close to shelves that should not hold anything
        //  can still fill them with leather)
        // Check for incorrect items when items are added to the Building_Storage
        //   (or when the player asks for it via Gizmo?)
        // If there are incorrect items, then the IsForbidden check can look for them,
        //   otherwise, it shouldn't bother checking ('cause that's overhead no one needs)
        // (Note: this will still fail in some cases - for example, with Deep Storage,
        //  if a storage unit is over capacity, pawns should take the stuff away, but
        //  that is logic specific to DS, and the items will simply be counted as
        //  forbidden here)
        public void CheckForIncorrectItems() {
            #if DEBUG
            Log.Message(""+parent+": checking for incorrect items");
            #endif
            shouldCheckForIncorrectItems=false;
            hasIncorrectItemsCounter=0;
            StorageSettings settings=(parent as IStoreSettingsParent)?.GetStoreSettings();
            if (settings==null) {
                Log.Warning("LWM Restricted Storage: "+parent+" failed to find storage settings - this is bad?");
                return;
            }
            foreach (IntVec3 c in (parent as ISlotGroupParent).AllSlotCells()) {
                foreach (Thing t in parent.Map.thingGrid.ThingsAt(c)) {
                    if (t.def.EverStorable(false) && !settings.AllowedToAccept(t)) {
                        #if DEBUG
                        Log.Message("  found incorrect thing "+t);
                        #endif
                        hasIncorrectItemsCounter=1;  //TODO: wait, what?
                        return;
                    }
                }
            }
        }
        public override void PostExposeData() {
            //if (Scribe.mode==LoadSaveMode.Saving &&
            //    this.IsDefault()) return;// don't bother saving.
            // Saving status:
            // We COULD do:
            //    Scribe_Values.Look(ref allowAll, "LWM_RS_allowAll", true);
            //    Scribe_Values.Look(ref allowNone, "LWM_RS_allowNone", false);
            //    Scribe_Values.Look(ref allowHumans, "LWM_RS_allowHumans", false);
            //    //etc etc etc
            // But there's this cool thing called reflection that means we don't have to:
            var allMyVars=typeof(CompRestrictedStorage).GetFields(HarmonyLib.AccessTools.allDeclared);
            object thisAsO=this;
            foreach (var f in allMyVars) {
                //Log.Warning("Looking at field "+f.Name+" of type "+f.FieldType);
                if (f.FieldType==typeof(bool) &&
                    String.Compare("allow", 0, f.Name, 0, 5)==0) {
                    //Log.Message("Scribe_Values.Looking at "+f.Name);
                    // Frankly, I have no idea if "ref f.GetValue(thisAsO)" would work.
                    //   Probably not. And I'm not inclined to find out, tbh.
                    bool x=(bool)f.GetValue(thisAsO);
                    // Scribe with default value:
                    // You can't actually get the "default" value (bool allowAll=true) because
                    // it's actually set inside the constructor. So we do it this way:
                    if (f.Name == "allowAll")
                        Scribe_Values.Look(ref x, "LWM_RS_"+f.Name, true);
                    else
                        Scribe_Values.Look(ref x, "LWM_RS_"+f.Name, false);
                    if (Scribe.mode==LoadSaveMode.LoadingVars)
                        f.SetValue(thisAsO, x);
                }
            }
            Log.ResetMessageCount();
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
            if (allowColonists) return false;
            if (allowGuests) return false;
            if (allowPrisoners) return false;
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
            // todo: disallowedPawns?
            if (allowHumans) return true;
            if (allowColonists && allowGuests && allowPrisoners) return true;
            return false;
        }
        bool AllowAllAnimals() {
            // todo: disallowedPawns?
            if (allowAnimals) return true;
            // todo: animals that don't eat?
            if (allowGrazers && allowNonGrazers) return true;
            if (allowMeatEaters && allowNonMeatEaters) return true;
            return false;
        }
        public bool IsForbidden(Pawn p, Thing t=null) {
            // obviously a lot to do here ;)
            if (allowAll) return false;
            if (p.Faction!=Faction.OfPlayer) return false;
            if (shouldCheckForIncorrectItems) CheckForIncorrectItems();
            // if thing t shouldn't be stored here, don't forbid it:
            // TODO: save hasIncorrectItemsCounter, so game behavior
            // doesn't change across save/load:
            if (hasIncorrectItemsCounter>0) {//TODO: This is totally whacked
                // a bit of logic to not ALWAYS check this.
                // Buildings_Storage don't tick, so
                // we need some logic to check from
                // time to time...
                // but we don't want to be checking
                // always if there are 8494 items in
                // storage...which happens.
                hasIncorrectItemsCounter++;
                if (hasIncorrectItemsCounter>100) // sure??
                    CheckForIncorrectItems();
                #if DEBUG
                Log.Message(""+parent+" is checking if "+t+" is allowed; Counter is at "+
                            hasIncorrectItemsCounter);
                #endif
                // actual check:
                if (t!=null &&
                    !((parent as IStoreSettingsParent).GetStoreSettings().AllowedToAccept(t))) return false;
            }
            if (allowNone) return true;
            //////////////////////////// Fine Logic /////////////////////////
            if (disallowedPawns!=null) {
                if (disallowedPawns.Contains(p)) return true;
            }
            RaceProperties race=p.RaceProps;
            if (allowHumans && race.Humanlike) return false;
            if (race.Humanlike) { // humanlike-specific logic:
                if (allowColonists && p.Faction==Faction.OfPlayer
                    && p.guest?.IsPrisoner != true) return false;
                if (allowPrisoners && p.guest?.IsPrisoner==true) return false;
                if (allowGuests    && p.Faction!=Faction.OfPlayer
                    && p.guest!=null && !p.guest.IsPrisoner
                    && p.guest.HostFaction==Faction.OfPlayer) return false;
            } else if (race.Animal) { // animal-specific logic:
                if (allowAnimals) return false;
                // TODO: Non-eaters
                if (!race.EatsFood) {
                    return true; // we don't have a category for them yet: TODO
                }
                // Ugh.  Tree eaters.  They aren't "Herbivorous."  They aren't "Omniverous."  We can't
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
                //Log.Message("Checking animal "+p+" about to fail");
            } // end animals
            // todo? else { // neither human nor animal?
            if (allowedPawns!=null) {
                if (allowedPawns.Contains(p)) return false;
            }
            if (allowedIfInAreas!=null) {
                //Log.Message("Checking pawn "+p+" with area restirction "+p.playerSettings.AreaRestriction+" vs "+allowedIfInAreas[0]+" (count "+allowedIfInAreas.Count+")");
                if (allowedIfInAreas.Contains(p.playerSettings.AreaRestriction)) return false;
            }
            if (!allowedIfNotInAreas.NullOrEmpty()) {
                if (!allowedIfNotInAreas.Contains(p.playerSettings.AreaRestriction)) return false;
            }
            // TODO: "Other" - prolly a mod setting
            // TODO: robots etc?
            return true;
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
        //// Outside access for Areas and Pawns (dialogs mostly):
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
        public void AddAllowedPawn(Pawn p) {
            if (allowedPawns==null) allowedPawns=new List<Pawn>();
            allowedPawns.Add(p);
            noAllNone();
        }
        public void RemoveAllowedPawn(Pawn p) {
            allowedPawns?.Remove(p);
        }
        public bool IsAllowedPawn(Pawn p) {
            return (allowedPawns?.Contains(p)==true);
        }
        public void AddDisallowedPawn(Pawn p) {
            if (disallowedPawns==null) disallowedPawns=new List<Pawn>();
            disallowedPawns.Add(p);
            noAllNone(); // todo: maybe set allowHumans and allowAnimals?
        }
        public void RemoveDisallowedPawn(Pawn p) {
            disallowedPawns?.Remove(p);
        }
        // my patience for typing has gone down:
        public bool IsDisallowedPawn(Pawn p) => (disallowedPawns?.Contains(p)==true);

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
            if (allowNone
                || allowHumans || allowColonists || allowPrisoners || allowGuests
                || allowAnimals || allowGrazers || allowNonGrazers || allowMeatEaters || allowNonMeatEaters )
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
            this.allowColonists=other.allowColonists;
            this.allowPrisoners=other.allowPrisoners;
            this.allowGuests=other.allowGuests;
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
        bool allowAll=true; // note: needs special logic when exposing
        bool allowNone=false;

        bool allowHumans=false;
        bool allowColonists=false;
        bool allowPrisoners=false;
        bool allowGuests=false;

        bool allowAnimals=false;
        bool allowGrazers = false;
        bool allowNonGrazers = false;
        bool allowMeatEaters = false;
        bool allowNonMeatEaters = false;
//        public bool AllowStarving { get { return _allowStarving;} set { } }
//            set { _allowStarving=value; Update();} }



        List<Area> allowedIfInAreas=null;
        List<Area> allowedIfNotInAreas=null;
        // todo: update this from time to time?  ...give pawns a comp if they are forbidden/allowed?
        //   or an hediff?
        List<Pawn> allowedPawns=null;
        List<Pawn> disallowedPawns=null;
        bool shouldCheckForIncorrectItems=true;
        int hasIncorrectItemsCounter=0;
    }

}
