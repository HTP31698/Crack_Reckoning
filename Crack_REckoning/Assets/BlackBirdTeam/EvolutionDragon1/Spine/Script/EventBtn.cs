using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace demoDragon_1
{
    public class EventBtn : MonoBehaviour
    {
        public List<Animator> animators = new();
        void ActionAnim(string nameAnim)
        {
            foreach (Animator anim in animators)
            {
                anim.SetTrigger(nameAnim);
            }
        }

        public void Idle()
        {
            ActionAnim("idle");
        }
        public void Move()
        {
            ActionAnim("move");
        }
        public void Attack()
        {
            ActionAnim("attack");
        }
        public void Hurt()
        {
            ActionAnim("hurt");
        }
        public void Death()
        {
            ActionAnim("death");
        }
    }
}

