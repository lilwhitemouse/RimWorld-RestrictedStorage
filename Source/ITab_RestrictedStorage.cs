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
        protected override void FillTab() {
            Text.Font=GameFont.Medium;
            CompRestrictedStorage crs=(this.SelThing as ThingWithComps).GetComp<CompRestrictedStorage>();
            Rect mainOptionRect=new Rect(10f, 10f, size.x-50f, 54f); // -10f for border, -30f for X to close the ITab
            crs.DisplayMainOption(mainOptionRect);
            Text.Font=GameFont.Small;

            Widgets.DrawLineHorizontal(10f, 65f, size.y-20f);

            Rect fineOptionsRect = new Rect(20f, 68f, this.size.x-35, this.size.y-70);
            crs.DisplayFineOptions(fineOptionsRect);
        }
    }

}
