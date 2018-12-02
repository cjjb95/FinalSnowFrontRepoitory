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
        private bool onSnow;

        //FOR SNOW DRIFTS
        float movementSpeed = 2;

        public float MovementSpeed
        {
            get
            {
                return this.movementSpeed;
            }
            set
            {
                this.movementSpeed = value >= 0 ? value : 0;
            }
        }
        bool shovelEquipped = false;
        private bool bOnce;

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
            this.bOnce = true;
            this.onSnow = false;
            this.Body.CollisionSkin.callbackFn += CollisionSkin_callbackFn;
            eventDispatcher.ObstacleCollision += EventDispatcher_ObstacleCollision;
            eventDispatcher.ObstacleEvent += EventDispatcher_ObstacleEvent;
            eventDispatcher.ItemEquipped += EventDispatcher_ItemEquipped;
        }

        private void EventDispatcher_ObstacleCollision(EventData eventData)
        {
            if (eventData.EventType == EventActionType.OnSnowDrift)
            {
                this.bOnce = true;

            }
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
                this.movementSpeed = 2;
                Console.WriteLine("Ice");
                object[] additionalParameter = { true };
                EventDispatcher.Publish(new EventData(EventActionType.OnIce, EventCategoryType.Obstacle, additionalParameter));
                this.bOnce = true;
            }
            else if (thingHit.ActorType == ActorType.Snow)
            {

                //movementSpeed = 0.3f;
                if (this.shovelEquipped)
                {
                    object[] additionalParameters = { "Oof" };
                    EventDispatcher.Publish(new EventData(EventActionType.OnPlay, EventCategoryType.Sound2D, additionalParameters));
                    EventDispatcher.Publish(new EventData(thingHit, EventActionType.OnRemoveActor, EventCategoryType.SystemRemove));
                    this.shovelEquipped = false;
                    this.bOnce = true;
                }

            }
            else if (thingHit.ActorType == ActorType.CollidableGround)
            {
                this.stunned = false;
                //Console.WriteLine("mov");
                this.movementSpeed = 2;
                if (this.bOnce)
                {
                    Console.WriteLine("ground");
                    EventDispatcher.Publish(new EventData(EventActionType.OnGround, EventCategoryType.Obstacle));
                    this.bOnce = false;
                }
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
