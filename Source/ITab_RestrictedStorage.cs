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
    // TODO: can we show at least part of this if multiple storage buildings are selected?
    public class ITab_RestrictedStorage : ITab {
        public ITab_RestrictedStorage() {
            this.size=new Vector2(460f, 450f);
            this.labelKey = "LWM.RS.Restriction"; //.Translate(); //todo
        }
        private static Vector2 scrollPosition=new Vector2(0f,0f);
        private static Rect viewRect=new Rect(0,0,100f,10000f); // I got scrollView in Listing_Standard to work!
        protected override void FillTab() {
            Text.Font=GameFont.Medium;
            CompRestrictedStorage crs=(this.SelThing as ThingWithComps).GetComp<CompRestrictedStorage>();
            Rect mainOptionRect=new Rect(10f, 10f, size.x-60f, 32f); // -10f for border, -40f for X to close the ITab
            crs.DisplayMainOption(mainOptionRect);
            Text.Font=GameFont.Small;

            Widgets.DrawLineHorizontal(10f, 43f, size.y-20f);

            Rect fineOptionsRect = new Rect(20f, 46f, this.size.x-35, this.size.y-50);
            var l = new Listing_Standard(GameFont.Small);
            l.BeginScrollView(fineOptionsRect, ref scrollPosition, ref viewRect);
            crs.DisplayFineOptions(l);
            l.EndScrollView(ref viewRect);
        }
    }

}
