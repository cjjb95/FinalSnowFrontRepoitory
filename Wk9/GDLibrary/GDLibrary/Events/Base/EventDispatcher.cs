/*
Function: 		Represent a message broker for events received and routed through the game engine. 
                Allows the receiver to receive event messages with no reference to the publisher - decouples the sender and receiver.
Author: 		NMCG
Version:		1.0
Date Updated:	11/10/17
Bugs:			None
Fixes:			None
Comments:       Should consider making this class a Singleton because of the static message Stack - See https://msdn.microsoft.com/en-us/library/ff650316.aspx
*/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class EventDispatcher : GameComponent
    {
        //See Queue doc - https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.queue-1?view=netframework-4.7.1
        private static Queue<EventData> queue; //stores events in arrival sequence
        private static HashSet<EventData> uniqueSet; //prevents the same event from existing in the stack for a single update cycle (e.g. when playing a sound based on keyboard press)


        //a delegate is basically a list - the list contains a pointer to a function - this function pointer comes from the object wishing to be notified when the event occurs.
        public delegate void AddActorEventHandler(EventData eventData);
        public delegate void RemoveActorEventHandler(EventData eventData);

        public delegate void MenuChangedEventHandler(EventData eventData);
        public delegate void PlayerChangedEventHandler(EventData eventData);
        public delegate void GlobalSoundEventHandler(EventData eventData);
        public delegate void Sound3DEventHandler(EventData eventData);
        public delegate void Sound2DEventHandler(EventData eventData);
        public delegate void OpacityEventHandler(EventData eventData);
        public delegate void ObjectPickingEventHandler(EventData eventData);
        public delegate void DebugEventHandler(EventData eventData);

        #region SnowFront Delegates
        public delegate void SnowDriftCollisionHandler(EventData eventData);
        public delegate void EnterSnowDriftHandler(EventData eventData);
        public delegate void InsideSnowDriftHandler(EventData eventData);
        public delegate void ExitSnowDriftHandler(EventData eventData);
        public delegate void WinGameHandler(EventData eventData);
        public delegate void LoseGameHandler(EventData eventData);
        public delegate void EquipCoatHandler(EventData eventData);
        public delegate void EquipShovelHandler(EventData eventData);
        public delegate void ItemEquippedHandler(EventData eventData);
        public delegate void UseItemEventHandler(EventData eventData);
        public delegate void LowTemperatureEventHandler(EventData eventData);
        public delegate void ObstacleCollisionHandler(EventData eventData);
        public delegate void ObstacleEventHandler(EventData eventData);
        #endregion


        //an event is either null (not yet happened) or non-null - when the event occurs the delegate reads through its list and calls all the listening functions
        public event AddActorEventHandler AddActorChanged;              //add a new drawn 2D or 3D object
        public event RemoveActorEventHandler RemoveActorChanged;        //remove a pickup, enemy

        public event MenuChangedEventHandler MenuChanged;               //when menu events occur e.g. play, sound mute
        public event PlayerChangedEventHandler PlayerChanged; //when player state is updated e.g. increase health
        public event GlobalSoundEventHandler GlobalSoundChanged;
        public event Sound3DEventHandler Sound3DChanged;
        public event Sound2DEventHandler Sound2DChanged;
        public event OpacityEventHandler OpacityChanged;
        public event ObjectPickingEventHandler ObjectPickChanged;
        public event DebugEventHandler DebugChanged;

        #region SnowFront Events
        public event SnowDriftCollisionHandler SnowDriftCollided;
        public event EnterSnowDriftHandler SnowDriftEntered;
        public event InsideSnowDriftHandler SnowDriftIntersected;
        public event ExitSnowDriftHandler SnowDriftExited;
        public event WinGameHandler GameWon;
        public event LoseGameHandler GameLost;
        public event ItemEquippedHandler ItemEquipped;
        public event UseItemEventHandler UseItem;
        public event LowTemperatureEventHandler LowTemp;
        public event ObstacleCollisionHandler ObstacleCollision;
        public event ObstacleEventHandler ObstacleEvent;
        #endregion






        public EventDispatcher(Game game, int initialSize)
            : base(game)
        {
            queue = new Queue<EventData>(initialSize);
            uniqueSet = new HashSet<EventData>(new EventDataEqualityComparer());
        }
        public static void Publish(EventData eventData)
        {
            //this prevents the same event being added multiple times within a single update e.g. 10x bell ring sounds
            if (!uniqueSet.Contains(eventData))
            {
                queue.Enqueue(eventData);
                uniqueSet.Add(eventData);
            }
        }

        EventData eventData;
        public override void Update(GameTime gameTime)
        {
            for (int i = 0; i < queue.Count; i++)
            {
                eventData = queue.Dequeue();
                Process(eventData);
                uniqueSet.Remove(eventData);
            }

            base.Update(gameTime);
        }

        private void Process(EventData eventData)
        {
            //Switch - See https://msdn.microsoft.com/en-us/library/06tc147t.aspx
            //one case for each category type
            switch (eventData.EventCategoryType)
            {

                case EventCategoryType.SystemAdd:
                    OnAddActor(eventData);
                    break;

                case EventCategoryType.SystemRemove:
                    OnRemoveActor(eventData);
                    break;

                case EventCategoryType.Menu:
                    OnMenuChanged(eventData);
                    break;

                case EventCategoryType.Player:
                    OnPlayerChanged(eventData);
                    break;

                case EventCategoryType.Sound3D:
                    OnSound3D(eventData);
                    break;

                case EventCategoryType.Sound2D:
                    OnSound2D(eventData);
                    break;

                case EventCategoryType.GlobalSound:
                    OnGlobalSound(eventData);
                    break;

                case EventCategoryType.Opacity:
                    OnOpacity(eventData);
                    break;

                case EventCategoryType.ObjectPicking:
                    OnObjectPicking(eventData);
                    break;

                case EventCategoryType.Debug:
                    OnDebug(eventData);
                    break;

                case EventCategoryType.EnterSnowDrift:
                    OnEnterSnowDrift(eventData);
                    break;

                case EventCategoryType.CollideWithSnowDrift:
                    OnCollideWithSnowDrift(eventData);
                    break;

                case EventCategoryType.ExitSnowDrift:
                    OnExitSnowDrift(eventData);
                    break;
                case EventCategoryType.IntersectSnowDrift:
                    OnIntersectSnowDrift(eventData);
                    break;

                case EventCategoryType.GameWon:
                    OnGameWon(eventData);
                    break;
                case EventCategoryType.GameLost:
                    OnGameLost(eventData);
                    break;

                case EventCategoryType.ItemEquipped:
                    OnItemEquipped(eventData);
                    break;

                case EventCategoryType.Item:
                    OnUseItem(eventData);
                    break;

                case EventCategoryType.LowTemp:
                    OnLowTemp(eventData);
                    break;

                case EventCategoryType.Obstacle:
                    OnObstacleCollision(eventData);
                    break;

                case EventCategoryType.ObstacleEvent:
                    OnObstacleEvent(eventData);
                    break;

                default:
                    break;
            }
        }

        private void OnObstacleEvent(EventData eventData)
        {
            ObstacleEvent?.Invoke(eventData);
        }

        private void OnObstacleCollision(EventData eventData)
        {
            ObstacleCollision?.Invoke(eventData);
        }

        #region SnowFront Event broadcasting
        private void OnItemEquipped(EventData eventData)
        {
            ItemEquipped?.Invoke(eventData);
        }

        private void OnGameLost(EventData eventData)
        {
            GameLost?.Invoke(eventData);
        }

        private void OnGameWon(EventData eventData)
        {
            GameWon?.Invoke(eventData);
        }

        private void OnIntersectSnowDrift(EventData eventData)
        {
            SnowDriftIntersected?.Invoke(eventData);
        }

        private void OnExitSnowDrift(EventData eventData)
        {
            SnowDriftExited?.Invoke(eventData);
        }

        private void OnCollideWithSnowDrift(EventData eventData)
        {
            SnowDriftCollided?.Invoke(eventData);
        }

        private void OnEnterSnowDrift(EventData eventData)
        {
            SnowDriftEntered?.Invoke(eventData);
        }

        private void OnLowTemp(EventData eventData)
        {
            LowTemp?.Invoke(eventData);
        }

        private void OnUseItem(EventData eventData)
        {
            UseItem?.Invoke(eventData);
        }

        #endregion


        //called when the PickingManager picks an object
        protected virtual void OnObjectPicking(EventData eventData)
        {
            ObjectPickChanged?.Invoke(eventData);

        }
        //called when a debug related event occurs (e.g. show/hide debug info)
        protected virtual void OnDebug(EventData eventData)
        {
            DebugChanged?.Invoke(eventData);
        }

        //called when a drawn objects opacity changes - which necessitates moving from opaque <-> transparent list in ObjectManager - see ObjectManager::RegisterForEventHandling()
        protected virtual void OnOpacity(EventData eventData)
        {
            OpacityChanged?.Invoke(eventData);
        }


        //called when a global sound event is sent to set volume by category or mute all sounds
        protected virtual void OnGlobalSound(EventData eventData)
        {
            GlobalSoundChanged?.Invoke(eventData);
        }

        //called when a 3D sound event is sent e.g. play "boom"
        protected virtual void OnSound3D(EventData eventData)
        {
            Sound3DChanged?.Invoke(eventData);
        }

        //called when a 2D sound event is sent e.g. play "menu music"
        protected virtual void OnSound2D(EventData eventData)
        {
            Sound2DChanged?.Invoke(eventData);
        }

        //called when a player wins, loses, increments/decrements health
        protected virtual void OnPlayerChanged(EventData eventData)
        {
            PlayerChanged?.Invoke(eventData);
        }

        //called when a menu is shown, hidden or modified
        protected virtual void OnMenuChanged(EventData eventData)
        {
            MenuChanged?.Invoke(eventData);
        }

        //called when a drawn objects needs to be added - see PickingManager::DoFireNewObject()
        protected virtual void OnAddActor(EventData eventData)
        {
            AddActorChanged?.Invoke(eventData);
        }

        //called when a drawn objects needs to be removed - see UIMouseObject::HandlePickedObject()
        protected virtual void OnRemoveActor(EventData eventData)
        {
            RemoveActorChanged?.Invoke(eventData);
        }
    }
}
