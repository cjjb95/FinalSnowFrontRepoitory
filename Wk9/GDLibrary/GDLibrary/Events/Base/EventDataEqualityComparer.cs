/*
Function: 		Used to compare a new event entering the EventDispatcher queue to existing events to prevent flooding by the same event within a single Update()
Author: 		NMCG
Version:		1.0
Date Updated:	
Bugs:			None
Fixes:			None
Comments:      
*/
using System.Collections.Generic;

namespace GDLibrary
{
    //used by the EventDispatcher to compare to events in the HashSet - remember that HashSets allow us to quickly prevent something from being added to a list/stack twice
    public class EventDataEqualityComparer : IEqualityComparer<EventData>
    {

        public bool Equals(EventData e1, EventData e2)
        {
            bool bEquals = true;

            //sometimes we don't specify ID or sender so run a test
            if (e1.ID != null && e2.ID != null)
                bEquals = e1.ID.Equals(e2.ID);

            bEquals = bEquals && e1.EventType.Equals(e2.EventType)
                    && e1.EventCategoryType.Equals(e2.EventCategoryType);

            if (e1.Sender != null && e2.Sender != null)
            {
                Actor actorE1 = e1.Sender as Actor;
                Actor actorE2 = e2.Sender as Actor;
                if (actorE1 != null && actorE2 != null)
                    bEquals = bEquals && (e1.Sender as Actor).GetID().Equals(e2.Sender as Actor);
                else
                    bEquals = false;
            }
            return bEquals;

        }

        public int GetHashCode(EventData e)
        {
            return e.GetHashCode();
        }
    }
}
