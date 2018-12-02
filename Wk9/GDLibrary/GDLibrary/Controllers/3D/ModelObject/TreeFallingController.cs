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

        public TreeFallingController(string id, ControllerType controllerType,
            PlayStatusType playStatusType, EventDispatcher eventDispatcher) : base(id, controllerType, playStatusType)
        {
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
            if (this.elapsed % 2000 == 0)
            {
                EventDispatcher.Publish(new EventData(EventActionType.OnTreeZone, EventCategoryType.ObstacleEvent));
            }

        }
    }
}
