using System;
using RimWorld;
using Verse;
using UnityEngine;

namespace LWM.RestrictedStorage
{
    public class Dialog_SpecifyAreas : Window {
        public override Vector2 InitialSize {
            get {
                return new Vector2(600,600);
            }
        }
        // Much useful from RimWorld.AreaAlowedGUI
        public Dialog_SpecifyAreas(Map map, CompRestrictedStorage comp) {
            this.map=map;
            this.crs=comp;
            this.doCloseX=true;
            this.doCloseButton=true;
            this.closeOnClickedOutside=true;
            this.forcePause=true;
            this.resizeable=true;
            this.optionalTitle="Specify Access Control by Area";//TODO: translate entire thing...
        }
        public override void DoWindowContents(Rect inRect) { //todo: much better info
            //make big
            Rect r=new Rect(0,0,inRect.width, 22);
            Rect rl=new Rect(1,22,inRect.width/2-11,22); // to fit over 1/2 innerRect below
            Rect rr=new Rect(inRect.width/2-10,22,inRect.width/2,22);
            Widgets.Label(r, "Contents are available to any colonists/animals who");
            Widgets.Label(rl, "ARE allowed in:");
            Widgets.Label(rr, "ARE NOT allowed in:");

            r = new Rect(1,44,inRect.width-1, inRect.height-48-48); //-height-button
            Rect innerRect=new Rect(0,0,inRect.width-20,totalHeight); // room for scroll bar
            Widgets.BeginScrollView(r, ref scrollPos, innerRect);

            float y=0f;
            DoAreaRow(ref y, innerRect.width, null); // "Unrestricted" area
            foreach (Area area in map.areaManager.AllAreas) {
                if (area.AssignableAsAllowed()) {
                    DoAreaRow(ref y, innerRect.width, area);
                }
            }
            this.totalHeight = y; // quick and dirty way to handle this
            Widgets.EndScrollView();
        }
        void DoAreaRow(ref float y, float width, Area area) {
            // if they have this, unrestricted
            Rect rl=new Rect(5,y+1, (width/2)-10, 28);
            // if they don't, unrestricted
            Rect rr=new Rect((width/2)+5,y+1, (width/2)-10, 28);
            GUI.DrawTexture(rr, (area != null) ? area.ColorTexture : BaseContent.GreyTex);
            GUI.DrawTexture(rl, (area != null) ? area.ColorTexture : BaseContent.GreyTex);
            //Text.Anchor = TextAnchor.MiddleLeft; // no idea what unity assembly this is, so leave it out.

            // note that AreaAllowedLabel_Area (and all this) takes `null` for "unrestricted"
			string text = AreaUtility.AreaAllowedLabel_Area(area);
            bool left=this.crs.IsAllowedInArea(area);
            bool right=this.crs.IsAllowedNotInArea(area);
            Widgets.CheckboxLabeled(rl, text, ref left);
            Widgets.CheckboxLabeled(rr, text, ref right);
            if (left!=this.crs.IsAllowedInArea(area)) {
                if (left) crs.AddAllowedArea(area);
                else this.crs.RemoveAllowedArea(area);
            }
            if (right!=this.crs.IsAllowedNotInArea(area)) {
                if (right) this.crs.AddAllowedNotInArea(area);
                else this.crs.RemoveAllowedNotInArea(area);
            }
            y+=30;
        }

        Map map;
        CompRestrictedStorage crs;
        Vector2 scrollPos;
        float totalHeight;
    }
}
