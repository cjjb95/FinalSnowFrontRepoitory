using GDLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDApp
{
    public class HeroPlayerObject : PlayerObject
    {
        int health;
        private bool stunned;

        //FOR SNOW DRIFTS
        float movementSpeed = 2;
        bool shovelEquipped = false;
        public HeroPlayerObject(string id,
            ActorType actorType,
            Transform3D transform,
            EffectParameters effectParameters,
            Model model,
            Keys[] moveKeys,
            float radius,
            float height,
            float accelerationRate,
            float decelerationRate,
            float jumpHeight,
            Vector3 translationOffset,
            KeyboardManager keyboardManager,
            EventDispatcher eventDispatcher)
            : base(id, actorType, transform, effectParameters, model, moveKeys, radius, height, accelerationRate, decelerationRate, jumpHeight, translationOffset, keyboardManager)
        {
            this.stunned = false;
            this.Body.CollisionSkin.callbackFn += CollisionSkin_callbackFn;
            eventDispatcher.ObstacleEvent += EventDispatcher_ObstacleEvent;
            eventDispatcher.ItemEquipped += EventDispatcher_ItemEquipped;
        }

        private void EventDispatcher_ItemEquipped(EventData eventData)
        {
            string additionalParameters = eventData.AdditionalParameters[0] as string;
            if (eventData.EventType == EventActionType.OnItem && additionalParameters == "shovel")
            {
                this.shovelEquipped = true;

            }
        }

        private void EventDispatcher_ObstacleEvent(EventData eventData)
        {
            if (eventData.EventType == EventActionType.OnSlip)
            {
                this.stunned = true;
            }
            else if (eventData.EventType == EventActionType.SlipOver)
            {
                this.stunned = false;
            }
        }

        private bool CollisionSkin_callbackFn(JigLibX.Collision.CollisionSkin collider, JigLibX.Collision.CollisionSkin collidee)
        {
            CollidableObject thingHit = collidee.Owner.ExternalData as CollidableObject;

            if (thingHit.ActorType == ActorType.Ice)
            {
                object[] additionalParameter = { true };
                EventDispatcher.Publish(new EventData(EventActionType.OnIce, EventCategoryType.Obstacle, additionalParameter));
            }
            else
            {
                this.stunned = false;
                object[] additionalParameter = { false };
                EventDispatcher.Publish(new EventData(EventActionType.OnIce, EventCategoryType.Obstacle, additionalParameter));
            }

            if (thingHit.ActorType == ActorType.Snow)
            {
                if (this.shovelEquipped)
                {

                    EventDispatcher.Publish(new EventData(thingHit, EventActionType.OnRemoveActor, EventCategoryType.SystemRemove));
                    this.shovelEquipped = false;
                }
                movementSpeed = 0.1f;
                object[] additionalParameter = { true };
                EventDispatcher.Publish(new EventData(EventActionType.OnSnowDrift, EventCategoryType.IntersectSnowDrift, additionalParameter));
            }
            else
            {
                movementSpeed = 2;
                object[] additionalParameter = { false };
                EventDispatcher.Publish(new EventData(EventActionType.OnSnowDrift, EventCategoryType.IntersectSnowDrift, additionalParameter));
            }

            return true;
        }

        protected override void HandleKeyboardInput(GameTime gameTime)
        {
            if (!this.stunned)
            {
                //forward/backward
                if (this.KeyboardManager.IsKeyDown(this.MoveKeys[0]))
                {
                    this.CharacterBody.Velocity += this.Transform.Look * movementSpeed * gameTime.ElapsedGameTime.Milliseconds;
                }
                else if (this.KeyboardManager.IsKeyDown(this.MoveKeys[1]))
                {
                    this.CharacterBody.Velocity -= this.Transform.Look * movementSpeed * gameTime.ElapsedGameTime.Milliseconds;
                }

                //strafe left/right
                if (this.KeyboardManager.IsKeyDown(this.MoveKeys[2]))
                {
                    this.CharacterBody.Velocity -= this.Transform.Right * movementSpeed * gameTime.ElapsedGameTime.Milliseconds;
                }
                else if (this.KeyboardManager.IsKeyDown(this.MoveKeys[3]))
                {
                    this.CharacterBody.Velocity += this.Transform.Right * movementSpeed * gameTime.ElapsedGameTime.Milliseconds;
                }


            }


            //to do - clone, dispose

        }


        protected override void HandleMouseInput(GameTime gameTime)
        {
            base.HandleMouseInput(gameTime);
        }
    }
}
