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
            this.labelKey="Restriction"; //todo
        }
        protected override void FillTab() {
            //Text.Font = GameFont.Small;
            Rect rect = new Rect(0f, 0f, this.size.x, this.size.y);
            Rect position = rect.ContractedBy(10f);

            var l = new Listing_Standard(GameFont.Small);
            l.Begin(position);
            CompRestrictedStorage crs=(this.SelThing as ThingWithComps).GetComp<CompRestrictedStorage>();
            Rect h=l.GetRect(22f);
            h.xMax-=22f;
            crs.DisplayMainOption(h);
            crs.DisplayFineOptions(l);
            l.End();
        }
    }

}
