﻿using System;
using UnityEngine;

namespace RPG.Questing {
    public class Goal {
        public bool Completed { get; set; } = false;
        public string Description { get; set; }
        public event Action onComplete;
        protected Quest Quest { get; set; }
        protected int CurrentAmount { get; set; }
        protected int RequiredAmount { get; set; }

        public virtual void Init() { }

        protected bool Evaluate() {
            if (CurrentAmount >= RequiredAmount) {
                Complete();
            }

            return Completed;
        }

        void Complete() {
            Completed = true;
            Debug.Log("Goal " + Description + " has been completed!");
            onComplete();
            Quest.CheckGols();
        }
    }
}
