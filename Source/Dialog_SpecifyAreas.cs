using System;
using RimWorld;
using Verse;
using UnityEngine;

namespace RestrictedStorage
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
            this.optionalTitle="Test title";
        }
        public override void DoWindowContents(Rect inRect) { //todo: much better info
            float y=0f;
            //make big
            Rect r=new Rect(0,0,inRect.width, 22);
            Rect rl=new Rect(0,22,inRect.width/2,22);
            Rect rr=new Rect(inRect.width/2,22,inRect.width/2,22);
            Widgets.Label(r, "Contents are available to any colonists/animals who");
            Widgets.Label(rl, "ARE allowed in:");
            Widgets.Label(rr, "ARE NOT allowed in:");
            y+=44f;
            //make small a
            DoAreaRow(ref y, inRect.width, null); // "Unrestricted" area
            foreach (Area area in map.areaManager.AllAreas) {
                if (area.AssignableAsAllowed()) {
                    DoAreaRow(ref y, inRect.width, area);
                }
            }

        }
        void DoAreaRow(ref float y, float width, Area area) {
            // if they have this, unrestricted
            Rect rl=new Rect(5,y+1, (width/2)-10, 28);
            // if they don't, unrestricted
            Rect rr=new Rect((width/2)+5,y+1, (width/2)-10, 28);
            GUI.DrawTexture(rr, (area != null) ? area.ColorTexture : BaseContent.GreyTex);
            GUI.DrawTexture(rl, (area != null) ? area.ColorTexture : BaseContent.GreyTex);
            //Text.Anchor = TextAnchor.MiddleLeft; // no idea what unity assembly this is, so leave it out.
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
            /*
			GUI.DrawTexture(rect, (area != null) ? area.ColorTexture : BaseContent.GreyTex);
			Text.Anchor = TextAnchor.MiddleLeft;
			string text = AreaUtility.AreaAllowedLabel_Area(area);
			Rect rect2 = rect;
			rect2.xMin += 3f;
			rect2.yMin += 2f;
			Widgets.Label(rect2, text);
            */
//            Widgets.Label(new Rect(0,y,width,22f),"Looking at area: "+AreaUtility.AreaAllowedLabel_Area(area));
//            y+=22f;
        }


        Map map;
        CompRestrictedStorage crs;
    }
}
