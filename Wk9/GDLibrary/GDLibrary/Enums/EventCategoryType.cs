/*
Function: 		Enum to define the category type of the event e.g. MainMenu (category type) is sending an OnRestart (event type) message
Author: 		NMCG
Version:		1.0
Date Updated:	
Bugs:			None
Fixes:			None
*/

namespace GDLibrary
{
    public enum EventCategoryType : sbyte
    {
        //one category for each group of events in EventType
        Camera,
        Player,
        //win/lose, state change
        NonPlayer,
        Pickup,
        //add remove objects
        SystemAdd,
        SystemRemove,
        //menu clicks and events
        Menu,
        //sound related
        GlobalSound, // menu music, menu mouse clicks
        Sound2D,
        Sound3D,
        //when an object goes from opaque <-> transparent and vice versa
        Opacity,
        Debug,
        ObjectPicking,
        Item,
        ItemEquipped,
        Obstacle,
        EnterSnowDrift,
        CollideWithSnowDrift,
        ExitSnowDrift,
        IntersectSnowDrift,
        LowTemp,
        GameWon,
        GameLost,
        ObstacleEvent,
    }
}
