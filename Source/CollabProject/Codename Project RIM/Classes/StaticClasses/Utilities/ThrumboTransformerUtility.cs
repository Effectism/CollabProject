﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace Codename_Project_RIM
{

    public static class ThrumboTransformerUtility
    {

        // Not entirely sure how to get this working; def and custom label are apparently null regardless, and it tries to add a missing body part record regardless
        //public static void TryTransferHediffs(Pawn fromPawn, ref Pawn toPawn, HediffCompProperties_ThrumboTransformer props)
        //{
        //    List<Hediff> pawnHediffs = fromPawn.health.hediffSet.GetHediffs<Hediff>().ToList();
        //    if (!pawnHediffs.NullOrEmpty())
        //    {
        //        foreach (Hediff hediff in pawnHediffs)
        //        {
        //            if (hediff.CanBeAddedToThrumbo())
        //            {
        //                BodyPartRecord part = hediff.Part;
        //                part.body = PR_BodyDefOf.QuadrupedAnimalWithHoovesAndHorn;
        //                if (part.def != null && props.partConversionsByDefNames.ContainsKey(part.def))
        //                    part.def = props.partConversionsByDefNames[part.def];
        //                if (part.customLabel != null && props.partConversionsByCustomLabels.ContainsKey(part.customLabel))
        //                    part.customLabel = props.partConversionsByCustomLabels[part.customLabel];
        //                if (!props.partConversionBlacklist.Contains(part.def))
        //                    toPawn.health.AddHediff(hediff, part);
        //            }
        //        }
        //    }
        //}

        public static bool CanBeAddedToThrumbo(this Hediff hediff)
        {
            return hediff.def != PR_HediffDefOf.ThrumboHornGrowth && hediff.def != PR_HediffDefOf.ThrumboCrackAddiction && !(hediff is Hediff_AddedPart) && !(hediff is Hediff_Implant);
        }

        public static bool TryGivePostTransformationBondRelation(ref Pawn thrumbo, Pawn pawn, out Pawn otherPawn)
        {
            otherPawn = null;

            // Queries, woo!
            int minimumOpinion = Pawn_RelationsTracker.FriendOpinionThreshold;
            Func<Pawn, bool> query = (Pawn oP) => pawn.relations.OpinionOf(oP) >= minimumOpinion && oP.relations.OpinionOf(pawn) >= minimumOpinion;
            List<Pawn> acceptablePlayerPawns = pawn.Map.mapPawns.FreeColonists.Where(query).ToList();

            if (!acceptablePlayerPawns.NullOrEmpty())
            {
                Dictionary<int, Pawn> candidatePairs = CandidateScorePairs(pawn, acceptablePlayerPawns);
                otherPawn = candidatePairs[candidatePairs.Keys.ToList().Max()];
                thrumbo.relations.AddDirectRelation(PawnRelationDefOf.Bond, otherPawn);
                for (int i = 0; i < otherPawn.relations.DirectRelations.Count; i++)
                {
                    DirectPawnRelation relation = otherPawn.relations.DirectRelations[i];
                    if (relation.otherPawn == pawn)
                        otherPawn.relations.RemoveDirectRelation(relation);
                }
                //otherPawn.relations.AddDirectRelation(PawnRelationDefOf.Bond, thrumbo);
            }

            return otherPawn != null;
        }

        public static Dictionary<int, Pawn> CandidateScorePairs (Pawn pawn, List<Pawn> candidateList)
        {
            Dictionary<int, Pawn> pairs = new Dictionary<int, Pawn>();

            for (int i = 0; i < candidateList.Count; i++)
            {
                Pawn candidate = candidateList[i];
                PawnRelationDef relation = pawn.GetMostImportantRelation(candidate);
                int pawnOpinionOfCandidate = pawn.relations.OpinionOf(candidate);
                int candidateOpinionOfPawn = candidate.relations.OpinionOf(pawn);
                int score = Mathf.RoundToInt(relation.importance + pawnOpinionOfCandidate + candidateOpinionOfPawn);
                pairs.Add(score, candidate);
            }

            return pairs;
        }

    }

}
