using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class IcicleController : Controller
    {
        private Random rnd;
        private int randomNum;
        private bool dropping;
        private List<int> picked;
        private int elapsedTime;

        public IcicleController(string id, ControllerType controllerType,
            PlayStatusType playStatusType, EventDispatcher eventDispatcher) : base(id, controllerType, playStatusType)
        {
            this.picked = new List<int>(6);
            this.rnd = new Random();
            this.dropping = false;
            eventDispatcher.ObstacleCollision += EventDispatcher_ObstacleCollision;
        }

        private void EventDispatcher_ObstacleCollision(EventData eventData)
        {
            if (eventData.EventType == EventActionType.OnIcicleZone)
            {
                this.PlayStatusType = PlayStatusType.Play;

            }
        }

        public override void Update(GameTime gameTime, IActor actor)
        {
            if (!this.dropping)
            {

                if (this.picked.Count < 6)
                {
                    this.randomNum = this.rnd.Next(0, 6);

                    if (this.picked.IndexOf(this.randomNum) == -1)
                    {
                        Console.WriteLine(this.randomNum);
                        this.picked.Add(this.randomNum);
                        object[] additionalParameters = { this.randomNum };
                        EventDispatcher.Publish(new EventData(EventActionType.OnIcicleZone,
                            EventCategoryType.ObstacleEvent, additionalParameters));
                        this.dropping = true;
                    }
                }
            }
            else
            {

                this.elapsedTime += gameTime.ElapsedGameTime.Milliseconds;
                if (this.elapsedTime % 300 == 0)
                {
                    this.dropping = false;
                }
            }
        }
    }
}
