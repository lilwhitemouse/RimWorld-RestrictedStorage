using System;
using RimWorld;
using Verse;
using UnityEngine;

namespace RestrictedStorage
{
    public class Dialog_SpecifyPawns : Window {
        public override Vector2 InitialSize {
            get {
                return new Vector2(600,600);
            }
        }
        // Much useful from RimWorld.AreaAlowedGUI
        public Dialog_SpecifyPawns(CompRestrictedStorage comp) {
            this.crs=comp;
            this.doCloseX=true;
            this.doCloseButton=true;
            this.closeOnClickedOutside=true;
            this.forcePause=true;
            this.resizeable=true;
            this.optionalTitle="Access Control for Pawns";
        }
        public override void DoWindowContents(Rect inRect) { //todo: much better info
            float y=0f;
            //make big
            Rect r=new Rect(0,0,inRect.width, 22);
            Rect rl=new Rect(0,22,inRect.width/2,22);
            Rect rr=new Rect(inRect.width/2,22,inRect.width/2,22);
            Widgets.Label(r, "Are the contents of this storage");
            Widgets.Label(rl, "ALWAYS accessible to");
            Widgets.Label(rr, "NEVER accessible to");
            //make small a
            Widgets.Label(new Rect(0,44,inRect.width,22), "Humans");
            y+=66f;
            foreach (Map m in Find.Maps) {
                foreach (Pawn p in m.mapPawns.AllPawns) {
                    if (p.RaceProps.Humanlike && p.Faction==Faction.OfPlayer) {
                        DoPawnRow(ref y, inRect.width, p);
                    }
                }
            }
            Widgets.Label(new Rect(0,y,inRect.width,22), "Animals");
            y+=22;
            foreach (Map m in Find.Maps) {
                foreach (Pawn p in m.mapPawns.AllPawns) {
                    if (p.RaceProps.Animal && p.Faction==Faction.OfPlayer) {
                        DoPawnRow(ref y, inRect.width, p);
                    }
                }
            }
        }
        void DoPawnRow(ref float y, float width, Pawn p) {
            // if they have this, unrestricted
            float oneQuarter=width/4;
            float threeQuaters=oneQuarter*3;
            //Rect leftAllowedRect=new Rect(20,y+1, 24,24);
            //Rect rightDisallowedRect=new Rect(width-44, y+1, 24,24);
            Rect centerPawnName=new Rect(50,y+2, width-100,22);
            if (this.crs.IsAllowedPawn(p)) { // already allowed
                bool X=true;
                Widgets.Checkbox(new Vector2(20,y+1), ref X);
                if (!X) {
                    crs.RemoveAllowedPawn(p);
                }
            } else { // should we allow it?
                Rect leftAllowedRect=new Rect(20,y+1, 24,24);
                if (Widgets.ButtonText(leftAllowedRect, "?")) {
                    crs.RemoveDisallowedPawn(p);
                    crs.AddAllowedPawn(p);
                }
            }
            Widgets.Label(centerPawnName, p.NameFullColored);
            if (this.crs.IsDisallowedPawn(p)) { // already disallowed
                bool X=false;
                Widgets.Checkbox(new Vector2(width-44,y+1), ref X);
                if (X) {
                    crs.RemoveDisallowedPawn(p);
                }
            } else {
                Rect rightDisallowedRect=new Rect(width-44, y+1, 24,24);
                if (Widgets.ButtonText(rightDisallowedRect, "?")) {
                    crs.RemoveAllowedPawn(p);
                    crs.AddDisallowedPawn(p);
                }
            }
            y+=26;
        }
        CompRestrictedStorage crs;
    }
}
