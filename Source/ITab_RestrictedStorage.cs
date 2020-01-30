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
            //TODO: move this logic to Comp:
            bool allowed=crs.AllowAll;
            l.CheckboxLabeled("Anyone May Take", ref allowed, null); //todo
            // Multiplayer would appreciate not spamming sync requests for setter, so we do this:
            if (allowed!=crs.AllowAll) crs.AllowAll=allowed;
            crs.DisplayFineOptions(l);
            l.End();
        }
        // TODO: find some way of doing this:
        /*public void RestrictedHeader(CompRestrictedStorage crs) {
            bool allowed=crs.AllowAll;
            bool orig=allowed;
            //Widgets.CheckboxL   .CheckboxLabeled("Anyone May Take", ref allowed, null); //todo
        }*/
    }

}
