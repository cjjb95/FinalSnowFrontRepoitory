using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class TreeFallingController : Controller
    {
        private int elapsed;
        private bool once;

        public TreeFallingController(string id, ControllerType controllerType,
            PlayStatusType playStatusType, EventDispatcher eventDispatcher) : base(id, controllerType, playStatusType)
        {
            this.once = true;
            eventDispatcher.ObstacleCollision += EventDispatcher_ObstacleCollision;
        }

        private void EventDispatcher_ObstacleCollision(EventData eventData)
        {
            if (eventData.EventType == EventActionType.OnTreeZone)
            {
                this.PlayStatusType = PlayStatusType.Play;
            }
        }

        public override void Update(GameTime gameTime, IActor actor)
        {
            this.elapsed += gameTime.ElapsedGameTime.Milliseconds;
           
            EventDispatcher.Publish(new EventData(EventActionType.OnTreeZone, EventCategoryType.ObstacleEvent));
            if(this.elapsed >= 7000)
            {
                if(this.once)
                {
                    EventDispatcher.Publish(new EventData(EventActionType.OnTreeTouchGround, EventCategoryType.ObstacleEvent));
                    this.once = false;
                }
                
            }

        }
    }
}
